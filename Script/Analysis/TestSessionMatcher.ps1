$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'SessionMatcher.ps1')

$script:passed = 0
$script:failed = 0
$script:failures = [System.Collections.Generic.List[string]]::new()
$script:baseTime = [datetime]::ParseExact('2026-11-21 01:00:00.000', 'yyyy-MM-dd HH:mm:ss.fff', [System.Globalization.CultureInfo]::InvariantCulture)

function New-TestSession {
    param(
        [string]$Id = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa',
        [double]$EndMs = 10000,
        [object[]]$Samples = @()
    )

    return [pscustomobject]@{
        Id = $Id
        EndTime = $script:baseTime.AddMilliseconds($EndMs)
        Samples = [System.Collections.Generic.List[object]]::new()
    } | ForEach-Object {
        $session = $_
        foreach ($sample in $Samples) { $session.Samples.Add($sample) }
        $session
    }
}

function New-TestWindow {
    param(
        [string]$FileName = 'ScenarioRunner_20261121_010000_Test.scenario.log',
        [string]$Scenario = 'Test.scenario',
        [int]$Order = 1,
        [double]$StartMs = 0,
        [double]$EndMs = 10000,
        [bool]$PreciseTime = $true
    )

    return [pscustomobject]@{
        RunId = '12345'
        Scenario = $Scenario
        FileName = $FileName
        WindowOrder = $Order
        OpenCommand = 'LAUNCH GUI'
        OpenLine = 1
        CloseLine = 10
        Start = $script:baseTime.AddMilliseconds($StartMs)
        End = $script:baseTime.AddMilliseconds($EndMs)
        OpenPassed = $true
        PreciseTime = $PreciseTime
        SessionId = ''
        MatchQuality = 'unmatched'
    }
}

function Assert-Equal {
    param([object]$Expected, [object]$Actual, [string]$Description)

    if ($Expected -cne $Actual) {
        throw "$Description`: expected=[$Expected], actual=[$Actual]"
    }
}

function Assert-Count {
    param([int]$Expected, [object[]]$Items, [string]$Description)

    Assert-Equal $Expected $Items.Count $Description
}

function Invoke-Test {
    param([string]$Name, [scriptblock]$Body)

    try {
        & $Body
        $script:passed++
        Write-Host "PASS: $Name" -ForegroundColor Green
    }
    catch {
        $script:failed++
        $message = "FAIL: ${Name}: $($_.Exception.Message)"
        $script:failures.Add($message)
        Write-Host $message -ForegroundColor Red
    }
}

Invoke-Test 'Single matching window' {
    $s = New-TestSession
    $w = New-TestWindow
    $r = Match-GuiSessions -RunId '12345' -Sessions @($s) -GuiWindows @($w)
    Assert-Count 1 $r.SessionRows 'session rows'
    Assert-Equal 'window-time' $r.SessionRows[0].MatchQuality 'match quality'
    Assert-Equal 'Test.scenario' $r.SessionRows[0].Scenario 'scenario'
    Assert-Equal $s.Id $w.SessionId 'window SessionId'
    Assert-Count 0 $r.Warnings 'warnings'
}

Invoke-Test 'No candidate leaves session unmatched' {
    $s = New-TestSession -EndMs 30000
    $w = New-TestWindow
    $r = Match-GuiSessions -RunId '12345' -Sessions @($s) -GuiWindows @($w)
    Assert-Equal 'unmatched' $r.SessionRows[0].MatchQuality 'match quality'
    Assert-Equal '' $r.SessionRows[0].Scenario 'scenario'
    Assert-Equal '' $w.SessionId 'window SessionId'
    Assert-Count 1 $r.Warnings 'warnings'
}

Invoke-Test 'Clearly closer precise candidate is selected' {
    $s = New-TestSession -EndMs 10000
    $near = New-TestWindow -Order 1 -EndMs 10000
    $far = New-TestWindow -Order 2 -EndMs 10300
    $r = Match-GuiSessions -RunId '12345' -Sessions @($s) -GuiWindows @($near, $far)
    Assert-Equal 'window-time' $r.SessionRows[0].MatchQuality 'match quality'
    Assert-Equal 1 $r.SessionRows[0].WindowOrder 'selected window order'
    Assert-Equal '' $far.SessionId 'far window SessionId'
}

Invoke-Test 'Close precise candidates are ambiguous' {
    $s = New-TestSession
    $a = New-TestWindow -Order 1 -EndMs 10000
    $b = New-TestWindow -Order 2 -EndMs 10050
    $r = Match-GuiSessions -RunId '12345' -Sessions @($s) -GuiWindows @($a, $b)
    Assert-Equal 'ambiguous' $r.SessionRows[0].MatchQuality 'match quality'
    Assert-Equal '' $a.SessionId 'first window SessionId'
    Assert-Equal '' $b.SessionId 'second window SessionId'
    Assert-Count 1 $r.Warnings 'warnings'
}

Invoke-Test 'Precise candidate distance threshold at 100ms' {
    $s = New-TestSession
    $a = New-TestWindow -Order 1 -EndMs 10000
    $b = New-TestWindow -Order 2 -EndMs 10100
    $r = Match-GuiSessions -RunId '12345' -Sessions @($s) -GuiWindows @($a, $b)
    Assert-Equal 'window-time' $r.SessionRows[0].MatchQuality 'match quality'
    Assert-Equal 1 $r.SessionRows[0].WindowOrder 'window order'
}

Invoke-Test 'Legacy second-precision candidate threshold at 1000ms' {
    $s = New-TestSession
    $a = New-TestWindow -Order 1 -EndMs 10000 -PreciseTime $false
    $b = New-TestWindow -Order 2 -EndMs 11000 -PreciseTime $false
    $r = Match-GuiSessions -RunId '12345' -Sessions @($s) -GuiWindows @($a, $b)
    Assert-Equal 'window-time' $r.SessionRows[0].MatchQuality 'match quality'
    Assert-Equal 1 $r.SessionRows[0].WindowOrder 'window order'
}

Invoke-Test 'Legacy second-precision candidates below threshold are ambiguous' {
    $s = New-TestSession
    $a = New-TestWindow -Order 1 -EndMs 10000 -PreciseTime $false
    $b = New-TestWindow -Order 2 -EndMs 10999 -PreciseTime $false
    $r = Match-GuiSessions -RunId '12345' -Sessions @($s) -GuiWindows @($a, $b)
    Assert-Equal 'ambiguous' $r.SessionRows[0].MatchQuality 'match quality'
}

Invoke-Test 'Precise window lower and upper tolerance boundaries' {
    foreach ($endMs in @(-250, 10500)) {
        $s = New-TestSession -EndMs $endMs
        $w = New-TestWindow
        $r = Match-GuiSessions -RunId '12345' -Sessions @($s) -GuiWindows @($w)
        Assert-Equal 'window-time' $r.SessionRows[0].MatchQuality "boundary $endMs"
    }
    foreach ($endMs in @(-251, 10501)) {
        $s = New-TestSession -EndMs $endMs
        $w = New-TestWindow
        $r = Match-GuiSessions -RunId '12345' -Sessions @($s) -GuiWindows @($w)
        Assert-Equal 'unmatched' $r.SessionRows[0].MatchQuality "outside boundary $endMs"
    }
}

Invoke-Test 'Legacy window lower and upper tolerance boundaries' {
    foreach ($endMs in @(-1000, 12000)) {
        $s = New-TestSession -EndMs $endMs
        $w = New-TestWindow -PreciseTime $false
        $r = Match-GuiSessions -RunId '12345' -Sessions @($s) -GuiWindows @($w)
        Assert-Equal 'window-time' $r.SessionRows[0].MatchQuality "boundary $endMs"
    }
    foreach ($endMs in @(-1001, 12001)) {
        $s = New-TestSession -EndMs $endMs
        $w = New-TestWindow -PreciseTime $false
        $r = Match-GuiSessions -RunId '12345' -Sessions @($s) -GuiWindows @($w)
        Assert-Equal 'unmatched' $r.SessionRows[0].MatchQuality "outside boundary $endMs"
    }
}

Invoke-Test 'A window cannot be assigned to two sessions' {
    $s1 = New-TestSession -Id 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa' -EndMs 10000
    $s2 = New-TestSession -Id 'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb' -EndMs 10000
    $w = New-TestWindow
    $r = Match-GuiSessions -RunId '12345' -Sessions @($s1, $s2) -GuiWindows @($w)
    $matched = @($r.SessionRows | Where-Object { $_.MatchQuality -eq 'window-time' })
    $unmatched = @($r.SessionRows | Where-Object { $_.MatchQuality -eq 'unmatched' })
    Assert-Count 1 $matched 'matched sessions'
    Assert-Count 1 $unmatched 'unmatched sessions'
    Assert-Equal $matched[0].SessionId $w.SessionId 'assigned session ID'
    Assert-Equal $null $unmatched[0].WindowOrder 'unmatched session window order'
    Assert-Count 1 $r.Warnings 'warnings'
}

Invoke-Test 'Sessions are ordered by end time' {
    $late = New-TestSession -Id 'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb' -EndMs 20000
    $early = New-TestSession -Id 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa' -EndMs 10000
    $a = New-TestWindow -Order 1 -EndMs 10000
    $b = New-TestWindow -Order 2 -StartMs 11000 -EndMs 20000
    $r = Match-GuiSessions -RunId '12345' -Sessions @($late, $early) -GuiWindows @($b, $a)
    Assert-Equal $early.Id $r.SessionRows[0].SessionId 'first SessionId'
    Assert-Equal 1 $r.SessionRows[0].SessionOrder 'first SessionOrder'
    Assert-Equal $late.Id $r.SessionRows[1].SessionId 'second SessionId'
    Assert-Equal 2 $r.SessionRows[1].SessionOrder 'second SessionOrder'
}

Invoke-Test 'Sample data and matching metadata are preserved' {
    $sample = [pscustomobject]@{ Target='Calculation'; Count=[long]2; TotalMs=30.0; AverageMs=15.0; MaxMs=20.0 }
    $s = New-TestSession -Samples @($sample)
    $w = New-TestWindow
    $r = Match-GuiSessions -RunId '12345' -Sessions @($s) -GuiWindows @($w)
    Assert-Count 1 $r.SampleRows 'sample count'
    $row = $r.SampleRows[0]
    Assert-Equal '12345' $row.RunId 'RunId'
    Assert-Equal $s.Id $row.SessionId 'sample SessionId'
    Assert-Equal 'Test.scenario' $row.Scenario 'sample Scenario'
    Assert-Equal 'window-time' $row.MatchQuality 'sample MatchQuality'
    Assert-Equal 'Calculation' $row.Target 'Target'
    Assert-Equal 2 $row.Count 'Count'
    Assert-Equal 30.0 $row.TotalMs 'TotalMs'
    Assert-Equal 15.0 $row.AverageMs 'AverageMs'
    Assert-Equal 20.0 $row.MaxMs 'MaxMs'
}

Invoke-Test 'Empty sessions and windows return empty rows' {
    $r = Match-GuiSessions -RunId '12345' -Sessions @() -GuiWindows @()
    Assert-Count 0 $r.SessionRows 'SessionRows'
    Assert-Count 0 $r.SampleRows 'SampleRows'
    Assert-Count 0 $r.GuiWindows 'GuiWindows'
    Assert-Count 0 $r.Warnings 'Warnings'
}

Invoke-Test 'Window without sessions remains unassigned' {
    $w = New-TestWindow
    $r = Match-GuiSessions -RunId '12345' -Sessions @() -GuiWindows @($w)
    Assert-Count 0 $r.SessionRows 'SessionRows'
    Assert-Count 1 $r.GuiWindows 'GuiWindows'
    Assert-Equal '' $w.SessionId 'unassigned window'
}

Write-Host ''
Write-Host "SessionMatcher tests: $script:passed passed, $script:failed failed."
if ($script:failed -gt 0) {
    exit 1
}
exit 0

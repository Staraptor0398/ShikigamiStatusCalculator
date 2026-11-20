$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'ScenarioSessionParser.ps1')

$script:passed = 0
$script:failed = 0
$script:baseDate = '20261121'
$script:baseTime = [datetime]::ParseExact('2026-11-21 01:00:00.000', 'yyyy-MM-dd HH:mm:ss.fff', [System.Globalization.CultureInfo]::InvariantCulture)

function Invoke-Parser {
    param(
        [string[]]$Lines = @(),
        [string]$Date = $script:baseDate,
        [datetime]$Start = $script:baseTime,
        [string]$Scenario = 'Test.scenario',
        [string]$RunId = '12345',
        [string]$FileName = 'ScenarioRunner_20261121_010000_Test.scenario.log'
    )

    Get-ScenarioGuiWindows -RunId $RunId -FileName $FileName -Date $Date -Start $Start -Scenario $Scenario -Lines $Lines
}

function Assert-Equal {
    param([object]$Expected, [object]$Actual, [string]$Description)

    if ($Expected -cne $Actual) {
        throw "${Description}: expected=[$Expected], actual=[$Actual]"
    }
}

function Assert-Count {
    param([int]$Expected, [object[]]$Items, [string]$Description)

    Assert-Equal $Expected $Items.Count $Description
}

function Assert-Contains {
    param([string]$Needle, [string[]]$Items, [string]$Description)

    if (-not (@($Items | Where-Object { $_.Contains($Needle) }).Count -gt 0)) {
        throw "${Description}: expected text containing [$Needle]"
    }
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
        Write-Host "FAIL: ${Name}: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Invoke-Test 'OPEN GUI START PASS CLOSE PASS creates one window' {
    $r = Invoke-Parser -Lines @(
        '[01:00:01.100] [1] OPEN GUI START',
        '[01:00:02.200] [1] OPEN GUI PASS',
        '[01:00:05.300] [9] CLOSE GUI PASS'
    )
    Assert-Count 1 $r.GuiWindows 'window count'
    Assert-Count 0 $r.ValidationIssues 'issue count'
    Assert-Equal $false $r.HasPendingOpen 'pending open'
    $w = $r.GuiWindows[0]
    Assert-Equal '12345' $w.RunId 'RunId'
    Assert-Equal 'Test.scenario' $w.Scenario 'Scenario'
    Assert-Equal 'ScenarioRunner_20261121_010000_Test.scenario.log' $w.FileName 'FileName'
    Assert-Equal 1 $w.WindowOrder 'WindowOrder'
    Assert-Equal 'OPEN GUI' $w.OpenCommand 'OpenCommand'
    Assert-Equal 1 $w.OpenLine 'OpenLine'
    Assert-Equal 9 $w.CloseLine 'CloseLine'
    Assert-Equal $script:baseTime.AddMilliseconds(1100) $w.Start 'Start'
    Assert-Equal $script:baseTime.AddMilliseconds(5300) $w.End 'End'
    Assert-Equal $true $w.OpenPassed 'OpenPassed'
    Assert-Equal $true $w.PreciseTime 'PreciseTime'
    Assert-Equal '' $w.SessionId 'initial SessionId'
    Assert-Equal 'unmatched' $w.MatchQuality 'initial MatchQuality'
}

Invoke-Test 'LAUNCH GUI without PASS can still close' {
    $r = Invoke-Parser -Lines @(
        '[01:00:01.000] [2] LAUNCH GUI START',
        '[01:00:03.000] [4] CLOSE GUI PASS'
    )
    Assert-Count 1 $r.GuiWindows 'window count'
    Assert-Equal 'LAUNCH GUI' $r.GuiWindows[0].OpenCommand 'OpenCommand'
    Assert-Equal $false $r.GuiWindows[0].OpenPassed 'OpenPassed'
    Assert-Count 0 $r.ValidationIssues 'issue count'
}

Invoke-Test 'OPEN PASS must use the matching step line' {
    $r = Invoke-Parser -Lines @(
        '[01:00:01.000] [2] OPEN GUI START',
        '[01:00:02.000] [3] OPEN GUI PASS',
        '[01:00:03.000] [4] CLOSE GUI PASS'
    )
    Assert-Equal $false $r.GuiWindows[0].OpenPassed 'OpenPassed'
}

Invoke-Test 'CLOSE FAIL does not close a pending window' {
    $r = Invoke-Parser -Lines @(
        '[01:00:01.000] [1] OPEN GUI START',
        '[01:00:02.000] [2] CLOSE GUI FAIL'
    )
    Assert-Count 0 $r.GuiWindows 'window count'
    Assert-Equal $true $r.HasPendingOpen 'pending open'
    Assert-Count 0 $r.ValidationIssues 'issue count'
}

Invoke-Test 'CLOSE START does not close a pending window' {
    $r = Invoke-Parser -Lines @(
        '[01:00:01.000] [1] LAUNCH GUI START',
        '[01:00:02.000] [2] CLOSE GUI START'
    )
    Assert-Count 0 $r.GuiWindows 'window count'
    Assert-Equal $true $r.HasPendingOpen 'pending open'
}

Invoke-Test 'Orphan CLOSE PASS produces a structural issue' {
    $r = Invoke-Parser -Lines @('[01:00:03.000] [4] CLOSE GUI PASS')
    Assert-Count 0 $r.GuiWindows 'window count'
    Assert-Count 1 $r.ValidationIssues 'issue count'
    Assert-Contains 'CLOSE GUI without OPEN/LAUNCH GUI in 12345 / ScenarioRunner_20261121_010000_Test.scenario.log' $r.ValidationIssues 'issue message'
    Assert-Equal $false $r.HasPendingOpen 'pending open'
}

Invoke-Test 'Overlapping START reports issue and replaces pending open' {
    $r = Invoke-Parser -Lines @(
        '[01:00:01.000] [1] OPEN GUI START',
        '[01:00:02.000] [2] LAUNCH GUI START',
        '[01:00:03.000] [2] LAUNCH GUI PASS',
        '[01:00:05.000] [5] CLOSE GUI PASS'
    )
    Assert-Count 1 $r.GuiWindows 'window count'
    Assert-Count 1 $r.ValidationIssues 'issue count'
    Assert-Contains 'Unclosed Gui window before another launch' $r.ValidationIssues 'issue message'
    Assert-Equal 2 $r.GuiWindows[0].OpenLine 'new open line'
    Assert-Equal 'LAUNCH GUI' $r.GuiWindows[0].OpenCommand 'new open command'
    Assert-Equal $true $r.GuiWindows[0].OpenPassed 'OpenPassed'
    Assert-Equal $false $r.HasPendingOpen 'pending open'
}

Invoke-Test 'Unfinished OPEN reports HasPendingOpen' {
    $r = Invoke-Parser -Lines @('[01:00:01.000] [1] OPEN GUI START')
    Assert-Count 0 $r.GuiWindows 'window count'
    Assert-Count 0 $r.ValidationIssues 'issue count'
    Assert-Equal $true $r.HasPendingOpen 'pending open'
}

Invoke-Test 'Empty input returns no windows or issues' {
    $r = Invoke-Parser -Lines @()
    Assert-Count 0 $r.GuiWindows 'window count'
    Assert-Count 0 $r.ValidationIssues 'issue count'
    Assert-Equal $false $r.HasPendingOpen 'pending open'
}

Invoke-Test 'Unrelated and malformed lines are ignored' {
    $r = Invoke-Parser -Lines @(
        'Scenario: Test.scenario',
        '[01:00:00.000] [1] CALC START',
        '[01:00:01.000] [2] OPEN GUI COMPLETED',
        '[01:00:02.000] [3] CLOSE GUI PASS extra',
        'garbage'
    )
    Assert-Count 0 $r.GuiWindows 'window count'
    Assert-Count 0 $r.ValidationIssues 'issue count'
    Assert-Equal $false $r.HasPendingOpen 'pending open'
}

Invoke-Test 'Two consecutive windows preserve order and step numbers' {
    $r = Invoke-Parser -Lines @(
        '[01:00:01.000] [1] LAUNCH GUI START',
        '[01:00:02.000] [2] CLOSE GUI PASS',
        '[01:00:03.000] [5] OPEN GUI START',
        '[01:00:04.000] [5] OPEN GUI PASS',
        '[01:00:06.000] [8] CLOSE GUI PASS'
    )
    Assert-Count 2 $r.GuiWindows 'window count'
    Assert-Equal 1 $r.GuiWindows[0].WindowOrder 'first order'
    Assert-Equal 2 $r.GuiWindows[1].WindowOrder 'second order'
    Assert-Equal 1 $r.GuiWindows[0].OpenLine 'first open line'
    Assert-Equal 2 $r.GuiWindows[0].CloseLine 'first close line'
    Assert-Equal 5 $r.GuiWindows[1].OpenLine 'second open line'
    Assert-Equal 8 $r.GuiWindows[1].CloseLine 'second close line'
    Assert-Equal $false $r.HasPendingOpen 'pending open'
}

Invoke-Test 'Legacy second-precision logs use whole seconds' {
    $r = Invoke-Parser -Lines @(
        '[01:00:01] [1] OPEN GUI START',
        '[01:00:02] [1] OPEN GUI PASS',
        '[01:00:05] [2] CLOSE GUI PASS'
    )
    Assert-Equal $false $r.GuiWindows[0].PreciseTime 'PreciseTime'
    Assert-Equal $script:baseTime.AddSeconds(1) $r.GuiWindows[0].Start 'Start'
    Assert-Equal $script:baseTime.AddSeconds(5) $r.GuiWindows[0].End 'End'
}

Invoke-Test 'Mixed timestamp precision is not considered precise' {
    $first = Invoke-Parser -Lines @('[01:00:01] [1] OPEN GUI START', '[01:00:05.000] [2] CLOSE GUI PASS')
    $second = Invoke-Parser -Lines @('[01:00:01.000] [1] OPEN GUI START', '[01:00:05] [2] CLOSE GUI PASS')
    Assert-Equal $false $first.GuiWindows[0].PreciseTime 'legacy start'
    Assert-Equal $false $second.GuiWindows[0].PreciseTime 'legacy close'
}

Invoke-Test 'Midnight rollover advances next-day timestamps' {
    $start = [datetime]::ParseExact('2026-11-21 23:59:58.000', 'yyyy-MM-dd HH:mm:ss.fff', [System.Globalization.CultureInfo]::InvariantCulture)
    $r = Invoke-Parser -Start $start -Lines @(
        '[23:59:59.000] [1] LAUNCH GUI START',
        '[00:00:02.000] [2] CLOSE GUI PASS'
    )
    Assert-Equal $start.AddSeconds(1) $r.GuiWindows[0].Start 'Start'
    Assert-Equal $start.AddSeconds(4) $r.GuiWindows[0].End 'End'
}

Invoke-Test 'Daytime timestamps do not trigger rollover' {
    $r = Invoke-Parser -Lines @(
        '[00:59:59.000] [1] OPEN GUI START',
        '[01:00:02.000] [2] CLOSE GUI PASS'
    )
    Assert-Equal ([datetime]::new(2026, 11, 21, 0, 59, 59)) $r.GuiWindows[0].Start 'Start'
    Assert-Equal ([datetime]::new(2026, 11, 21, 1, 0, 2)) $r.GuiWindows[0].End 'End'
}

Invoke-Test 'Parse-ScenarioTime supports precise and legacy clocks' {
    Assert-Equal $script:baseTime.AddMilliseconds(123) (Parse-ScenarioTime '20261121' '01:00:00.123') 'precise clock'
    Assert-Equal $script:baseTime (Parse-ScenarioTime '20261121' '01:00:00') 'legacy clock'
}

Invoke-Test 'Parse-ScenarioTime rejects invalid clocks and dates' {
    foreach ($value in @('20261121|01:00', '20261121|24:00:00', '20261121|01:60:00', '20261121|01:00:00.12', '20260230|01:00:00')) {
        $parts = $value.Split('|')
        $threw = $false
        try { $null = Parse-ScenarioTime $parts[0] $parts[1] }
        catch { $threw = $true }
        Assert-Equal $true $threw "invalid timestamp [$value]"
    }
}

Invoke-Test 'Caller-supplied metadata is preserved' {
    $r = Invoke-Parser -RunId '998877' -FileName 'custom.log' -Scenario 'Different.scenario' -Lines @(
        '[01:00:01.000] [1] LAUNCH GUI START',
        '[01:00:03.000] [2] CLOSE GUI PASS'
    )
    $w = $r.GuiWindows[0]
    Assert-Equal '998877' $w.RunId 'RunId'
    Assert-Equal 'custom.log' $w.FileName 'FileName'
    Assert-Equal 'Different.scenario' $w.Scenario 'Scenario'
}

Write-Host ''
Write-Host "ScenarioSessionParser tests: $script:passed passed, $script:failed failed."
if ($script:failed -gt 0) { exit 1 }
exit 0

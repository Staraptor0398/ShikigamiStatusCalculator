$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'PerformanceSessionParser.ps1')

$script:passed = 0
$script:failed = 0
$script:failures = [System.Collections.Generic.List[string]]::new()
$script:runId = '12345'
$script:idA = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
$script:idB = 'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb'
$script:tempDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ('TestPerformanceSessionParser_' + [guid]::NewGuid().ToString('N'))
New-Item -Path $script:tempDirectory -ItemType Directory -Force | Out-Null

function New-LogLine {
    param(
        [string]$Time = '2026-11-21 01:00:00.000',
        [string]$SessionId = $script:idA,
        [ValidateSet('Begin', 'End')]
        [string]$Event = 'Begin'
    )

    return "$Time Session=$SessionId Event=$Event"
}

function New-TargetLine {
    param(
        [string]$Time = '2026-11-21 01:00:00.500',
        [string]$SessionId = $script:idA,
        [string]$Target = 'Calculation',
        [string]$Count = '2',
        [string]$TotalMs = '30.500',
        [string]$AverageMs = '15.250',
        [string]$MaxMs = '20.250'
    )

    return "$Time Session=$SessionId Target=$Target Count=$Count TotalMs=$TotalMs AverageMs=$AverageMs MaxMs=$MaxMs"
}

function Invoke-Parse {
    param([AllowEmptyCollection()][string[]]$Lines = @())

    $path = Join-Path $script:tempDirectory 'Performance.log'
    [System.IO.File]::WriteAllLines($path, [string[]]$Lines, [System.Text.UTF8Encoding]::new($false))
    return Get-PerformanceSessions -RunId $script:runId -Path $path
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
    param([string]$Expected, [object[]]$Items, [string]$Description)

    if (-not (@($Items | Where-Object { $_ -ceq $Expected }).Count -gt 0)) {
        throw "${Description}: expected item=[$Expected], actual=[$($Items -join '; ')]"
    }
}

function Assert-Throws {
    param([scriptblock]$Body, [string]$Description)

    $thrown = $false
    try {
        & $Body | Out-Null
    }
    catch {
        $thrown = $true
    }
    if (-not $thrown) { throw "${Description}: expected an exception" }
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

try {
    Invoke-Test 'Single session with one target' {
        $r = Invoke-Parse @((New-LogLine), (New-TargetLine), (New-LogLine -Time '2026-11-21 01:00:01.000' -Event End))
        Assert-Count 1 $r.Sessions 'session count'
        Assert-Count 0 $r.Warnings 'warning count'
        Assert-Equal $script:idA $r.Sessions[0].Id 'session ID'
        Assert-Count 1 $r.Sessions[0].Samples 'sample count'
        Assert-Equal 'Calculation' $r.Sessions[0].Samples[0].Target 'target'
    }

    Invoke-Test 'Several targets retain order and values' {
        $r = Invoke-Parse @(
            (New-LogLine)
            (New-TargetLine -Target 'Load' -Count 3 -TotalMs '12.125' -AverageMs '4.041' -MaxMs '8.000')
            (New-TargetLine -Target 'Save' -Count 1 -TotalMs '7.500' -AverageMs '7.500' -MaxMs '7.500')
            (New-LogLine -Time '2026-11-21 01:00:02.000' -Event End)
        )
        Assert-Count 2 $r.Sessions[0].Samples 'sample count'
        Assert-Equal 'Load' $r.Sessions[0].Samples[0].Target 'first target'
        Assert-Equal 'Save' $r.Sessions[0].Samples[1].Target 'second target'
        Assert-Equal ([long]3) $r.Sessions[0].Samples[0].Count 'count'
        Assert-Equal 12.125 $r.Sessions[0].Samples[0].TotalMs 'total'
        Assert-Equal 4.041 $r.Sessions[0].Samples[0].AverageMs 'average'
        Assert-Equal 8.0 $r.Sessions[0].Samples[0].MaxMs 'maximum'
    }

    Invoke-Test 'Multiple completed sessions preserve file order' {
        $r = Invoke-Parse @(
            (New-LogLine -SessionId $script:idB)
            (New-TargetLine -SessionId $script:idB)
            (New-LogLine -SessionId $script:idB -Time '2026-11-21 01:00:02.000' -Event End)
            (New-LogLine -SessionId $script:idA -Time '2026-11-21 01:00:03.000')
            (New-LogLine -SessionId $script:idA -Time '2026-11-21 01:00:04.000' -Event End)
        )
        Assert-Count 2 $r.Sessions 'session count'
        Assert-Equal $script:idB $r.Sessions[0].Id 'first session'
        Assert-Equal $script:idA $r.Sessions[1].Id 'second session'
        Assert-Count 1 $r.Sessions[0].Samples 'first samples'
        Assert-Count 0 $r.Sessions[1].Samples 'second samples'
    }

    Invoke-Test 'End timestamp replaces Begin timestamp' {
        $r = Invoke-Parse @(
            (New-LogLine -Time '2026-11-21 01:00:00.125')
            (New-LogLine -Time '2026-11-21 01:02:03.456' -Event End)
        )
        Assert-Equal '2026-11-21 01:02:03.456' $r.Sessions[0].EndTime.ToString('yyyy-MM-dd HH:mm:ss.fff') 'end time'
    }

    Invoke-Test 'Orphan Target generates warning' {
        $r = Invoke-Parse @((New-TargetLine))
        Assert-Count 0 $r.Sessions 'sessions'
        Assert-Count 1 $r.Warnings 'warnings'
        Assert-Contains "Orphan performance target in run $script:runId" $r.Warnings 'orphan warning'
    }

    Invoke-Test 'Target with wrong SessionId is skipped' {
        $r = Invoke-Parse @(
            (New-LogLine)
            (New-TargetLine -SessionId $script:idB)
            (New-LogLine -Time '2026-11-21 01:00:01.000' -Event End)
        )
        Assert-Count 1 $r.Sessions 'sessions'
        Assert-Count 0 $r.Sessions[0].Samples 'samples'
        Assert-Count 1 $r.Warnings 'warnings'
    }

    Invoke-Test 'Orphan End generates warning' {
        $r = Invoke-Parse @((New-LogLine -Event End))
        Assert-Count 0 $r.Sessions 'sessions'
        Assert-Contains "Unmatched session end in run $script:runId" $r.Warnings 'unmatched End warning'
    }

    Invoke-Test 'Mismatched End discards current session' {
        $r = Invoke-Parse @(
            (New-LogLine)
            (New-LogLine -SessionId $script:idB -Event End)
            (New-LogLine -Time '2026-11-21 01:00:02.000' -Event End)
        )
        Assert-Count 0 $r.Sessions 'sessions'
        Assert-Count 2 $r.Warnings 'warnings'
        Assert-Contains "Unmatched session end in run $script:runId" $r.Warnings 'unmatched End warning'
    }

    Invoke-Test 'Second Begin warns and replaces incomplete session' {
        $r = Invoke-Parse @(
            (New-LogLine)
            (New-TargetLine)
            (New-LogLine -SessionId $script:idB -Time '2026-11-21 01:00:02.000')
            (New-TargetLine -SessionId $script:idB)
            (New-LogLine -SessionId $script:idB -Time '2026-11-21 01:00:03.000' -Event End)
        )
        Assert-Count 1 $r.Sessions 'sessions'
        Assert-Equal $script:idB $r.Sessions[0].Id 'completed session ID'
        Assert-Count 1 $r.Sessions[0].Samples 'completed session samples'
        Assert-Contains "Unclosed performance session in $script:runId" $r.Warnings 'unclosed warning'
    }

    Invoke-Test 'Unclosed session at EOF is not returned' {
        $r = Invoke-Parse @((New-LogLine), (New-TargetLine))
        Assert-Count 0 $r.Sessions 'completed sessions'
        Assert-Count 0 $r.Warnings 'warnings from current behavior'
    }

    Invoke-Test 'Empty file returns empty arrays' {
        $r = Invoke-Parse @()
        Assert-Count 0 $r.Sessions 'sessions'
        Assert-Count 0 $r.Warnings 'warnings'
    }

    Invoke-Test 'Unrelated and malformed lines are ignored' {
        $r = Invoke-Parse @(
            'some unrelated line'
            "2026-11-21 01:00:00 Session=$script:idA Event=Begin"
            "2026-11-21 01:00:00.000 Session=INVALID Event=Begin"
            "2026-11-21 01:00:00.000 Session=$script:idA Event=Unknown"
            (New-LogLine)
            "2026-11-21 01:00:00.500 Session=$script:idA Target=Load Count=2 TotalMs=abc AverageMs=1 MaxMs=1"
            (New-LogLine -Time '2026-11-21 01:00:01.000' -Event End)
        )
        Assert-Count 1 $r.Sessions 'sessions'
        Assert-Count 0 $r.Sessions[0].Samples 'samples'
        Assert-Count 0 $r.Warnings 'warnings'
    }

    Invoke-Test 'Zero count and decimal values are parsed without validation' {
        $r = Invoke-Parse @(
            (New-LogLine)
            (New-TargetLine -Count 0 -TotalMs '0.000' -AverageMs '0.0' -MaxMs '0')
            (New-LogLine -Event End)
        )
        Assert-Count 1 $r.Sessions[0].Samples 'sample count'
        Assert-Equal 0 $r.Sessions[0].Samples[0].Count 'zero count'
        Assert-Equal 0.0 $r.Sessions[0].Samples[0].TotalMs 'zero total'
    }

    Invoke-Test 'Invalid numeric format is ignored rather than counted as Target' {
        $r = Invoke-Parse @(
            (New-LogLine)
            (New-TargetLine -Count '-1')
            (New-TargetLine -TotalMs '-1.0')
            (New-TargetLine -MaxMs '1e2')
            (New-LogLine -Event End)
        )
        Assert-Count 1 $r.Sessions 'sessions'
        Assert-Count 0 $r.Sessions[0].Samples 'samples'
    }

    Invoke-Test 'Duplicate Targets are preserved for downstream validation' {
        $r = Invoke-Parse @(
            (New-LogLine)
            (New-TargetLine)
            (New-TargetLine)
            (New-LogLine -Event End)
        )
        Assert-Count 2 $r.Sessions[0].Samples 'duplicate target samples'
    }

    Invoke-Test 'Malformed End is ignored and does not close session' {
        $r = Invoke-Parse @(
            (New-LogLine)
            "2026-11-21 01:00:01 Session=$script:idA Event=End"
            (New-LogLine -Time '2026-11-21 01:00:02.000' -Event End)
        )
        Assert-Count 1 $r.Sessions 'sessions'
        Assert-Count 0 $r.Warnings 'warnings'
        Assert-Equal '2026-11-21 01:00:02.000' $r.Sessions[0].EndTime.ToString('yyyy-MM-dd HH:mm:ss.fff') 'end time'
    }

    Invoke-Test 'Invalid Begin date raises an error' {
        Assert-Throws {
            Invoke-Parse @((New-LogLine -Time '2026-13-21 01:00:00.000'))
        } 'invalid Begin date'
    }

    Invoke-Test 'Invalid End date raises an error' {
        Assert-Throws {
            Invoke-Parse @(
                (New-LogLine)
                (New-LogLine -Time '2026-13-21 01:00:01.000' -Event End)
            )
        } 'invalid End date'
    }
}
finally {
    if (Test-Path -LiteralPath $script:tempDirectory) {
        Remove-Item -LiteralPath $script:tempDirectory -Recurse -Force
    }
}

Write-Host ''
Write-Host "PerformanceSessionParser tests: $script:passed passed, $script:failed failed."
if ($script:failed -gt 0) { exit 1 }
exit 0

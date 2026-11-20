$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'ScenarioSessionParser.ps1')
. (Join-Path $PSScriptRoot 'ScenarioCommandParser.ps1')

$script:passed = 0
$script:failed = 0
$script:failures = [System.Collections.Generic.List[string]]::new()
$script:baseTime = [datetime]::ParseExact('2026-11-21 01:00:00', 'yyyy-MM-dd HH:mm:ss', [System.Globalization.CultureInfo]::InvariantCulture)

function New-TestState {
    param(
        [string]$Scenario = 'Test.scenario',
        [string]$Date = '20261121',
        [datetime]$Start = $script:baseTime,
        [System.Collections.Generic.List[object]]$CommandDetails,
        [System.Collections.Generic.List[string]]$ValidationIssues
    )

    $parameters = @{
        RunId = '12345'
        FileName = 'ScenarioRunner_20261121_010000_Test.scenario.log'
        Date = $Date
        Start = $Start
        Scenario = $Scenario
    }
    if ($null -ne $CommandDetails) { $parameters.CommandDetails = $CommandDetails }
    if ($null -ne $ValidationIssues) { $parameters.ValidationIssues = $ValidationIssues }
    return New-ScenarioCommandState @parameters
}

function New-StepLine {
    param(
        [string]$Clock,
        [int]$Number,
        [string]$Command,
        [string]$Result
    )
    return "[$Clock] [$Number] $Command    $Result"
}

function Add-Step {
    param(
        [object]$State,
        [string]$Clock,
        [int]$Number,
        [string]$Command,
        [string]$Result
    )
    Update-ScenarioCommandState -State $State -Line (New-StepLine $Clock $Number $Command $Result)
}

function Assert-Equal {
    param([object]$Expected, [object]$Actual, [string]$Description)
    if ($Expected -is [string] -and $Actual -is [string]) {
        if (-not [string]::Equals($Expected, $Actual, [System.StringComparison]::Ordinal)) {
            throw "${Description}: expected=[$Expected], actual=[$Actual]"
        }
    }
    elseif ($Expected -cne $Actual) {
        throw "${Description}: expected=[$Expected], actual=[$Actual]"
    }
}

function Assert-Count {
    param([int]$Expected, [object]$Items, [string]$Description)
    Assert-Equal $Expected @($Items).Count $Description
}

function Assert-Contains {
    param([string]$Fragment, [object]$Items, [string]$Description)
    if (-not (@($Items) | Where-Object { $_.Contains($Fragment) })) {
        throw "${Description}: expected fragment [$Fragment]"
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
        $message = "FAIL: ${Name}: $($_.Exception.Message)"
        $script:failures.Add($message)
        Write-Host $message -ForegroundColor Red
    }
}

Invoke-Test 'New state initializes non-null empty collections' {
    $s = New-TestState
    if ($null -eq $s.CommandDetails) { throw 'CommandDetails is null' }
    if ($null -eq $s.ValidationIssues) { throw 'ValidationIssues is null' }
    if ($null -eq $s.PendingSteps) { throw 'PendingSteps is null' }
    Assert-Count 0 $s.CommandDetails 'CommandDetails'
    Assert-Count 0 $s.ValidationIssues 'ValidationIssues'
    Assert-Equal 0 $s.PendingSteps.Count 'PendingSteps'
    Assert-Equal $null $s.ActiveCommand 'ActiveCommand'
}

Invoke-Test 'Provided collection references are preserved' {
    $details = [System.Collections.Generic.List[object]]::new()
    $issues = [System.Collections.Generic.List[string]]::new()
    $s = New-TestState -CommandDetails $details -ValidationIssues $issues
    if (-not [object]::ReferenceEquals($details, $s.CommandDetails)) { throw 'CommandDetails reference changed' }
    if (-not [object]::ReferenceEquals($issues, $s.ValidationIssues)) { throw 'ValidationIssues reference changed' }
    Add-Step $s '01:00:00.000' 1 'CALC' 'START'
    Add-Step $s '01:00:00.010' 1 'CALC' 'PASS'
    Assert-Count 1 $details 'shared details'
    Assert-Count 0 $issues 'shared issues'
}

Invoke-Test 'START exposes active command to timing instrumentation' {
    $s = New-TestState
    Add-Step $s '01:00:00.100' 7 'SAVE BUILD' 'START'
    Assert-Equal '7' $s.ActiveCommand 'active line'
    Assert-Equal $true ($s.PendingSteps.ContainsKey('7')) 'pending entry exists'
    Assert-Equal 'SAVE BUILD' $s.PendingSteps['7'].Type 'pending type'
    Assert-Equal 'SAVE BUILD' $s.PendingSteps['7'].RawText 'pending text'
    Assert-Equal $script:baseTime.AddMilliseconds(100) $s.PendingSteps['7'].Time 'pending timestamp'
}

Invoke-Test 'START and PASS produce one complete command row' {
    $s = New-TestState
    Add-Step $s '01:00:00.100' 7 'CALCULATE' 'START'
    Add-Step $s '01:00:00.350' 7 'CALCULATE' 'PASS'
    Assert-Count 1 $s.CommandDetails 'rows'
    $r = $s.CommandDetails[0]
    Assert-Equal '12345' $r.RunId 'RunId'
    Assert-Equal 'Test.scenario' $r.Scenario 'Scenario'
    Assert-Equal $s.FileName $r.FileName 'FileName'
    Assert-Equal 7 $r.LineNumber 'line'
    Assert-Equal 'CALCULATE' $r.CommandType 'type'
    Assert-Equal 'CALCULATE' $r.CommandText 'text'
    Assert-Equal 'PASS' $r.Result 'result'
    Assert-Equal 250 $r.DurationMs 'duration'
    Assert-Equal $true $r.PreciseTime 'precision'
    Assert-Equal $null $s.ActiveCommand 'active cleared'
    Assert-Equal 0 $s.PendingSteps.Count 'pending cleared'
    Assert-Count 0 $s.ValidationIssues 'issues'
}

Invoke-Test 'START and FAIL retain failed command details' {
    $s = New-TestState
    Add-Step $s '01:00:00.000' 2 'TRY LOAD BUILD' 'START'
    Add-Step $s '01:00:00.050' 2 'TRY LOAD BUILD' 'FAIL'
    Assert-Count 1 $s.CommandDetails 'rows'
    Assert-Equal 'FAIL' $s.CommandDetails[0].Result 'result'
    Assert-Equal 'TRY LOAD BUILD' $s.CommandDetails[0].CommandType 'type'
    Assert-Equal 50 $s.CommandDetails[0].DurationMs 'duration'
    Assert-Count 0 $s.ValidationIssues 'issues'
}

Invoke-Test 'Command type recognizes longest matching prefix' {
    Assert-Equal 'CHECK SHIKIGAMI SELECTION' (Get-ScenarioCommandType 'CHECK SHIKIGAMI SELECTION "x"') 'longest selection'
    Assert-Equal 'CHECK CALC DETAIL UNCHANGED' (Get-ScenarioCommandType 'CHECK CALC DETAIL UNCHANGED') 'longest detail'
    Assert-Equal 'CHECK CALC DETAIL' (Get-ScenarioCommandType 'CHECK CALC DETAIL "x"') 'detail'
    Assert-Equal 'CHECK CALC' (Get-ScenarioCommandType 'CHECK CALC') 'calc check'
    Assert-Equal 'CALCULATE' (Get-ScenarioCommandType 'CALCULATE') 'calculate'
    Assert-Equal 'CALC' (Get-ScenarioCommandType 'CALC') 'calc'
}

Invoke-Test 'Unknown command names remain UNKNOWN' {
    Assert-Equal 'UNKNOWN' (Get-ScenarioCommandType 'SOME NEW COMMAND') 'unknown'
    $s = New-TestState
    Add-Step $s '01:00:00.000' 9 'SOME NEW COMMAND' 'START'
    Add-Step $s '01:00:00.020' 9 'SOME NEW COMMAND' 'PASS'
    Assert-Equal 'UNKNOWN' $s.CommandDetails[0].CommandType 'row command type'
}

Invoke-Test 'Command type requires a complete prefix token' {
    Assert-Equal 'UNKNOWN' (Get-ScenarioCommandType 'CALCULATEX') 'suffix'
    Assert-Equal 'UNKNOWN' (Get-ScenarioCommandType 'CHECK CALCX') 'suffix'
    Assert-Equal 'CALCULATE' (Get-ScenarioCommandType 'CALCULATE "arg"') 'space separated argument'
}

Invoke-Test 'Irrelevant timing and malformed lines are ignored' {
    $s = New-TestState
    foreach ($line in @(
        'random text',
        '[01:00:00.100] [Timing] SaveDataOperator.GetLoadDialog TotalMs=1',
        '[01:00:00.100] [1] CALCULATE PASS',
        '[01:00:00.100] [1] CALCULATE    DONE',
        '[01:00:00.100] Scenario Result: PASS'
    )) { Update-ScenarioCommandState -State $s -Line $line }
    Assert-Count 0 $s.CommandDetails 'rows'
    Assert-Count 0 $s.ValidationIssues 'issues'
    Assert-Equal 0 $s.PendingSteps.Count 'pending'
}

Invoke-Test 'PASS without START is a validation issue' {
    $s = New-TestState
    Add-Step $s '01:00:00.100' 12 'CALC' 'PASS'
    Assert-Count 0 $s.CommandDetails 'rows'
    Assert-Contains 'PASS without START for command line 12' $s.ValidationIssues 'issue'
}

Invoke-Test 'FAIL without START is a validation issue' {
    $s = New-TestState
    Add-Step $s '01:00:00.100' 12 'CALC' 'FAIL'
    Assert-Count 0 $s.CommandDetails 'rows'
    Assert-Contains 'FAIL without START for command line 12' $s.ValidationIssues 'issue'
}

Invoke-Test 'Duplicate START warns and replaces pending state' {
    $s = New-TestState
    Add-Step $s '01:00:00.100' 5 'CALC' 'START'
    Add-Step $s '01:00:00.200' 5 'CALC' 'START'
    Assert-Contains 'Duplicate START for command line 5' $s.ValidationIssues 'issue'
    Assert-Equal $script:baseTime.AddMilliseconds(200) $s.PendingSteps['5'].Time 'replaced start'
    Add-Step $s '01:00:00.300' 5 'CALC' 'PASS'
    Assert-Equal 100 $s.CommandDetails[0].DurationMs 'duration since replacement'
}

Invoke-Test 'Command text mismatch warns but records terminal command' {
    $s = New-TestState
    Add-Step $s '01:00:00.000' 3 'CALC' 'START'
    Add-Step $s '01:00:00.100' 3 'CALCULATE' 'PASS'
    Assert-Contains 'Command text mismatch at line 3' $s.ValidationIssues 'mismatch warning'
    Assert-Equal 'CALCULATE' $s.CommandDetails[0].CommandText 'terminal text'
    Assert-Equal 'CALC' $s.CommandDetails[0].CommandType 'starting type'
}

Invoke-Test 'Negative duration reports issue and creates no row' {
    $s = New-TestState
    Add-Step $s '01:00:02.000' 4 'CALC' 'START'
    Add-Step $s '01:00:01.000' 4 'CALC' 'PASS'
    Assert-Contains 'Negative command duration at line 4' $s.ValidationIssues 'issue'
    Assert-Count 0 $s.CommandDetails 'rows'
    Assert-Equal 0 $s.PendingSteps.Count 'pending removed'
}

Invoke-Test 'Incomplete START is recorded by Complete' {
    $s = New-TestState
    Add-Step $s '01:00:00.100' 23 'SAVE BUILD' 'START'
    Complete-ScenarioCommandState -State $s
    Assert-Count 1 $s.CommandDetails 'rows'
    $r = $s.CommandDetails[0]
    Assert-Equal 'INCOMPLETE' $r.Result 'result'
    Assert-Equal 23 $r.LineNumber 'line'
    Assert-Equal 'SAVE BUILD' $r.CommandType 'type'
    Assert-Equal $null $r.End 'end'
    Assert-Equal $null $r.DurationMs 'duration'
    Assert-Equal $true $r.PreciseTime 'precision'
}

Invoke-Test 'Completing empty state records nothing' {
    $s = New-TestState
    Complete-ScenarioCommandState -State $s
    Assert-Count 0 $s.CommandDetails 'rows'
    Assert-Count 0 $s.ValidationIssues 'issues'
}

Invoke-Test 'Second-precision timestamps are retained' {
    $s = New-TestState
    Add-Step $s '01:00:01' 3 'CALC' 'START'
    Add-Step $s '01:00:02' 3 'CALC' 'PASS'
    Assert-Equal $false $s.CommandDetails[0].PreciseTime 'precision'
    Assert-Equal 1000 $s.CommandDetails[0].DurationMs 'duration'
}

Invoke-Test 'Mixed timestamp precision is not precise' {
    $s = New-TestState
    Add-Step $s '01:00:01.000' 3 'CALC' 'START'
    Add-Step $s '01:00:02' 3 'CALC' 'PASS'
    Assert-Equal $false $s.CommandDetails[0].PreciseTime 'precision'
}

Invoke-Test 'Midnight rollover advances the date' {
    $start = [datetime]::ParseExact('2026-11-21 23:59:59', 'yyyy-MM-dd HH:mm:ss', [System.Globalization.CultureInfo]::InvariantCulture)
    $s = New-TestState -Start $start
    Add-Step $s '23:59:59.900' 4 'CALC' 'START'
    Add-Step $s '00:00:00.100' 4 'CALC' 'PASS'
    Assert-Equal $start.Date.AddDays(1).AddMilliseconds(100) $s.CommandDetails[0].End 'next day'
    Assert-Equal 200 $s.CommandDetails[0].DurationMs 'duration'
}

Invoke-Test 'Daytime clocks do not roll over' {
    $s = New-TestState
    Add-Step $s '01:00:01.000' 1 'CALC' 'START'
    Add-Step $s '01:00:02.000' 1 'CALC' 'PASS'
    Assert-Equal $script:baseTime.AddSeconds(2) $s.CommandDetails[0].End 'end date'
}

Invoke-Test 'Concurrent lines keep latest active command and independent pending entries' {
    $s = New-TestState
    Add-Step $s '01:00:00.000' 1 'CALC' 'START'
    Add-Step $s '01:00:00.050' 2 'SAVE BUILD' 'START'
    Assert-Equal '2' $s.ActiveCommand 'latest active line'
    Assert-Equal 2 $s.PendingSteps.Count 'pending entries'
    Add-Step $s '01:00:00.100' 1 'CALC' 'PASS'
    Assert-Equal '2' $s.ActiveCommand 'active remains latest'
    Assert-Equal $true $s.PendingSteps.ContainsKey('2') 'second still pending'
    Add-Step $s '01:00:00.200' 2 'SAVE BUILD' 'PASS'
    Assert-Equal $null $s.ActiveCommand 'cleared'
    Assert-Count 2 $s.CommandDetails 'details'
}

Invoke-Test 'Closing active command leaves other pending lines unchanged' {
    $s = New-TestState
    Add-Step $s '01:00:00.000' 1 'CALC' 'START'
    Add-Step $s '01:00:00.050' 2 'SAVE BUILD' 'START'
    Add-Step $s '01:00:00.100' 2 'SAVE BUILD' 'FAIL'
    Assert-Equal $null $s.ActiveCommand 'active command cleared'
    Assert-Equal $true $s.PendingSteps.ContainsKey('1') 'earlier pending entry preserved'
}

Invoke-Test 'Zero-duration command is retained' {
    $s = New-TestState
    Add-Step $s '01:00:00.100' 1 'CLEAR' 'START'
    Add-Step $s '01:00:00.100' 1 'CLEAR' 'PASS'
    Assert-Equal 0 $s.CommandDetails[0].DurationMs 'duration'
}

Invoke-Test 'Scenario metadata and independent states are retained' {
    $a = New-TestState -Scenario 'First.scenario'
    $b = New-TestState -Scenario 'Second.scenario'
    Add-Step $a '01:00:00.000' 1 'CALC' 'START'
    Add-Step $b '01:00:00.000' 1 'CLEAR' 'START'
    Add-Step $a '01:00:00.200' 1 'CALC' 'PASS'
    Add-Step $b '01:00:00.300' 1 'CLEAR' 'PASS'
    Assert-Equal 'First.scenario' $a.CommandDetails[0].Scenario 'first name'
    Assert-Equal 'Second.scenario' $b.CommandDetails[0].Scenario 'second name'
    Assert-Equal 'CALC' $a.CommandDetails[0].CommandType 'first type'
    Assert-Equal 'CLEAR' $b.CommandDetails[0].CommandType 'second type'
}

Write-Host ''
Write-Host "ScenarioCommandParser tests: $script:passed passed, $script:failed failed."
if ($script:failed -gt 0) { exit 1 }
exit 0

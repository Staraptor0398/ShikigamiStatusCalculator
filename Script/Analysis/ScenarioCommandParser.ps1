# Scenario command timing. Command names match ScenarioCommandType; longest names first.
$scenarioCommandNames = @(
    'CHECK SHIKIGAMI SELECTION',
    'CHECK CALC DETAIL UNCHANGED',
    'WAIT SHIKIGAMI AUTO REPAIR',
    'CHECK SNAPSHOT COMPARISON',
    'OPEN SHIKIGAMI REGISTER',
    'CREATE SHIKIGAMI BACKUP',
    'CHECK SHIKIGAMI LIST',
    'COMPARE SAVED SNAPSHOT',
    'BREAK SHIKIGAMI HEADER',
    'CHECK INPUT UNCHANGED',
    'CHECK SAVEDATA LEVEL',
    'OPEN SHIKIGAMI EDIT',
    'CAPTURE CALC DETAIL',
    'REGISTER SHIKIGAMI',
    'OPEN CLEAR DIALOG',
    'CLOSE CALC DETAIL',
    'LOAD SAVED MITAMA',
    'RECOVER SHIKIGAMI',
    'CHECK CALCULATION',
    'CHECK CALC DETAIL',
    'OPEN CALC DETAIL',
    'SELECT SHIKIGAMI',
    'COMPARE SNAPSHOT',
    'LOAD SAVED BUILD',
    'RELOAD SHIKIGAMI',
    'REMOVE SHIKIGAMI',
    'TRY LOAD MITAMA',
    'CLEAR SHIKIGAMI',
    'CHECK SHIKIGAMI',
    'CONFIRM DIALOG',
    'EDIT SHIKIGAMI',
    'TRY LOAD BUILD',
    'CANCEL DIALOG',
    'SAVE SNAPSHOT',
    'CAPTURE INPUT',
    'CHECK CLEARED',
    'CLOSE DIALOG',
    'EQUIP MITAMA',
    'CHECK DIALOG',
    'LOAD MITAMA',
    'SAVE MITAMA',
    'SEL SHIKIGAMI',
    'CHECK RARITY',
    'SEL RARITY',
    'LAUNCH GUI',
    'LOAD BUILD',
    'SAVE BUILD',
    'CLOSE GUI',
    'CHECK CALC',
    'OPEN GUI',
    'CALCULATE',
    'CALC',
    'CLEAR'
)

function Get-ScenarioCommandType([string]$rawText) {
    foreach ($name in $scenarioCommandNames) {
        if ($rawText -eq $name -or $rawText.StartsWith($name + ' ', [System.StringComparison]::Ordinal)) {
            return $name
        }
    }
    return 'UNKNOWN'
}

# Stateful command parsing also exposes the active command to existing Timing analysis.
function New-ScenarioCommandState {
    param(
        [Parameter(Mandatory)][string]$RunId,
        [Parameter(Mandatory)][string]$FileName,
        [Parameter(Mandatory)][string]$Date,
        [Parameter(Mandatory)][datetime]$Start,
        [Parameter(Mandatory)][string]$Scenario,
        [System.Collections.Generic.List[object]]$CommandDetails,
        [System.Collections.Generic.List[string]]$ValidationIssues
    )

    if ($null -eq $CommandDetails) {
        $CommandDetails = [System.Collections.Generic.List[object]]::new()
    }
    if ($null -eq $ValidationIssues) {
        $ValidationIssues = [System.Collections.Generic.List[string]]::new()
    }

    return [pscustomobject]@{
        RunId = $RunId
        FileName = $FileName
        Date = $Date
        Start = $Start
        Scenario = $Scenario
        PendingSteps = @{}
        ActiveCommand = $null
        CommandDetails = $CommandDetails
        ValidationIssues = $ValidationIssues
    }
}

function Update-ScenarioCommandState {
    param(
        [Parameter(Mandatory)][object]$State,
        [Parameter(Mandatory)][string]$Line
    )

    $line = $Line
    $culture = [System.Globalization.CultureInfo]::InvariantCulture
    if ($line -notmatch '^\[(\d{2}:\d{2}:\d{2}(?:\.\d{3})?)\] \[(\d+)\] (.*?)\s{4}(START|PASS|FAIL)\s*$') { return }
    $clock = $Matches[1]
    $lineNumber = [int]$Matches[2]
    $rawCommand = $Matches[3]
    $result = $Matches[4]
    $stepTime = Parse-ScenarioTime $State.Date $clock
    if ($stepTime -lt $State.Start.AddHours(-12)) { $stepTime = $stepTime.AddDays(1) }
    $commandType = Get-ScenarioCommandType $rawCommand
    $key = [string]$lineNumber
    if ($result -eq 'START') {
        if ($State.PendingSteps.ContainsKey($key)) {
            $State.ValidationIssues.Add("Duplicate START for command line $lineNumber in $($State.RunId) / $($State.FileName)")
        }
        $State.PendingSteps[$key] = [pscustomobject]@{Time=$stepTime;RawText=$rawCommand;Type=$commandType;Precise=$clock.Contains('.')}
        $State.ActiveCommand = $key
        return
    }
    if (-not $State.PendingSteps.ContainsKey($key)) {
        $State.ValidationIssues.Add("$result without START for command line $lineNumber in $($State.RunId) / $($State.FileName)")
        return
    }
    $pending = $State.PendingSteps[$key]
    $State.PendingSteps.Remove($key)
    if ($State.ActiveCommand -eq $key) { $State.ActiveCommand = $null }
    if ($pending.RawText -ne $rawCommand) {
        $State.ValidationIssues.Add("Command text mismatch at line $lineNumber in $($State.RunId) / $($State.FileName)")
    }
    $elapsedMs = ($stepTime - $pending.Time).TotalMilliseconds
    if ($elapsedMs -lt 0) {
        $State.ValidationIssues.Add("Negative command duration at line $lineNumber in $($State.RunId) / $($State.FileName)")
        return
    }
    $State.CommandDetails.Add([pscustomobject]@{
        RunId=$State.RunId;Scenario=$State.Scenario;FileName=$State.FileName;LineNumber=$lineNumber;
        CommandType=$pending.Type;CommandText=$rawCommand;Result=$result;
        Start=$pending.Time;End=$stepTime;DurationMs=[math]::Round($elapsedMs,3);
        PreciseTime=($pending.Precise -and $clock.Contains('.'))
    })
}

function Complete-ScenarioCommandState {
    param([Parameter(Mandatory)][object]$State)

    foreach ($key in $State.PendingSteps.Keys) {
        $pending = $State.PendingSteps[$key]
        $State.CommandDetails.Add([pscustomobject]@{
            RunId=$State.RunId;Scenario=$State.Scenario;FileName=$State.FileName;LineNumber=[int]$key;
            CommandType=$pending.Type;CommandText=$pending.RawText;Result='INCOMPLETE';
            Start=$pending.Time;End=$null;DurationMs=$null;PreciseTime=$pending.Precise
        })
    }
}

# Shared ScenarioRunner GUI lifetime extraction.
# Returns closed GUI windows, structural issues, and any unfinished launch.
function Parse-ScenarioTime([string]$day, [string]$clock) {
    if ($clock -notmatch '^(\d{2}):(\d{2}):(\d{2})(?:\.(\d{3}))?$') {
        throw "Invalid Scenario Runner timestamp: day=$day clock=$clock"
    }

    $hour = [int]$Matches[1]
    $minute = [int]$Matches[2]
    $second = [int]$Matches[3]
    $millisecond = if ($Matches[4]) { [int]$Matches[4] } else { 0 }

    try {
        $datePart = [datetime]::ParseExact($day, 'yyyyMMdd', [System.Globalization.CultureInfo]::InvariantCulture)
        return [datetime]::new($datePart.Year, $datePart.Month, $datePart.Day, $hour, $minute, $second, $millisecond)
    }
    catch {
        throw "Invalid Scenario Runner timestamp: day=$day clock=$clock. $($_.Exception.Message)"
    }
}
function Get-ScenarioGuiWindows {
    param(
        [Parameter(Mandatory)]
        [string]$RunId,

        [Parameter(Mandatory)]
        [string]$FileName,

        [Parameter(Mandatory)]
        [string]$Date,

        [Parameter(Mandatory)]
        [datetime]$Start,

        [Parameter(Mandatory)]
        [string]$Scenario,

        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [string[]]$Lines
    )

    $guiIntervals = [System.Collections.Generic.List[object]]::new()
    $validationIssues = [System.Collections.Generic.List[string]]::new()
    $pendingOpen = $null

    foreach ($line in $Lines) {
        # Build Gui lifetime windows from Runner commands, not the whole Scenario interval.
        # Old Runner logs have second precision; new logs have millisecond precision.
        if ($line -match '^\[(\d{2}:\d{2}:\d{2}(?:\.\d{3})?)\] \[([0-9]+)\] (OPEN GUI|LAUNCH GUI|CLOSE GUI)\s+(START|PASS|FAIL)\s*$') {
            $stepTime = Parse-ScenarioTime $Date $Matches[1]
            if ($stepTime -lt $Start.AddHours(-12)) { $stepTime = $stepTime.AddDays(1) }
            $stepLine = [int]$Matches[2]
            $command = $Matches[3]
            $stepResult = $Matches[4]
            $preciseTime = $Matches[1].Contains('.')
            if (($command -eq 'OPEN GUI' -or $command -eq 'LAUNCH GUI') -and $stepResult -eq 'START') {
                if ($null -ne $pendingOpen) {
                    $validationIssues.Add("Unclosed Gui window before another launch in $RunId / $FileName")
                }
                $pendingOpen = [pscustomobject]@{Start=$stepTime; StartLine=$stepLine; Command=$command; Passed=$false; PreciseTime=$preciseTime}
            }
            elseif (($command -eq 'OPEN GUI' -or $command -eq 'LAUNCH GUI') -and $stepResult -eq 'PASS') {
                if ($null -ne $pendingOpen -and $pendingOpen.StartLine -eq $stepLine) { $pendingOpen.Passed = $true }
            }
            elseif ($command -eq 'CLOSE GUI' -and $stepResult -eq 'PASS') {
                if ($null -eq $pendingOpen) {
                    $validationIssues.Add("CLOSE GUI without OPEN/LAUNCH GUI in $RunId / $FileName")
                }
                else {
                    $window = [pscustomobject]@{
                        RunId=$RunId; Scenario=$Scenario; FileName=$FileName;
                        WindowOrder=($guiIntervals.Count+1); OpenCommand=$pendingOpen.Command;
                        OpenLine=$pendingOpen.StartLine; CloseLine=$stepLine;
                        Start=$pendingOpen.Start; End=$stepTime;
                        OpenPassed=$pendingOpen.Passed; PreciseTime=($pendingOpen.PreciseTime -and $preciseTime); SessionId=''; MatchQuality='unmatched'
                    }
                    $guiIntervals.Add($window)
                    $pendingOpen = $null
                }
            }
        }
    }

    return [pscustomobject]@{
        GuiWindows = @($guiIntervals.ToArray())
        ValidationIssues = @($validationIssues.ToArray())
        HasPendingOpen = ($null -ne $pendingOpen)
    }
}

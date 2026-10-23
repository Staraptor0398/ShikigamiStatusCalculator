param(
    [string]$RunIdFile = (Join-Path $PSScriptRoot 'Result/RunIds.txt'),
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'Result/Analysis'),
    [switch]$SkipDownload
)

$ErrorActionPreference = 'Stop'
$repo = 'Staraptor0398/ShikigamiStatusCalculator'
$artifactName = 'scenario-runner-batch-logs'
$culture = [System.Globalization.CultureInfo]::InvariantCulture

if (-not (Test-Path $RunIdFile)) { throw "Run ID file does not exist: $RunIdFile" }
$runIds = @((Get-Content $RunIdFile | ForEach-Object { $_.Trim() }) | Where-Object { $_ -match '^\d+$' } | Select-Object -Unique)
if ($runIds.Count -eq 0) { throw "No valid run IDs in $RunIdFile" }
if (-not $SkipDownload -and -not (Get-Command gh -ErrorAction SilentlyContinue)) { throw 'GitHub CLI (gh) is missing.' }

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$downloadRoot = Join-Path $OutputDirectory 'Artifacts'
New-Item -ItemType Directory -Force -Path $downloadRoot | Out-Null

function Parse-Time([string]$value) {
    return [datetime]::ParseExact($value, 'yyyy-MM-dd HH:mm:ss.fff', $culture)
}
function Parse-ScenarioTime([string]$day, [string]$clock) {
    if ($clock -notmatch '^(\d{2}):(\d{2}):(\d{2})(?:\.(\d{3}))?$') {
        throw "Invalid Scenario Runner timestamp: day=$day clock=$clock"
    }

    $hour = [int]$Matches[1]
    $minute = [int]$Matches[2]
    $second = [int]$Matches[3]
    $millisecond = if ($Matches[4]) { [int]$Matches[4] } else { 0 }

    try {
        $datePart = [datetime]::ParseExact($day, 'yyyyMMdd', $culture)
        return [datetime]::new($datePart.Year, $datePart.Month, $datePart.Day, $hour, $minute, $second, $millisecond)
    }
    catch {
        throw "Invalid Scenario Runner timestamp: day=$day clock=$clock. $($_.Exception.Message)"
    }
}
function Export-Rows([object[]]$rows, [string]$path) {
    if ($rows.Count -gt 0) { $rows | Export-Csv -Path $path -NoTypeInformation -Encoding UTF8 }
    else { [System.IO.File]::WriteAllText($path, '', [System.Text.UTF8Encoding]::new($false)) }
}


# Scenario command timing. Command names match ScenarioCommandType; longest names first.
$scenarioCommandNames = @(
    'CHECK CALC DETAIL UNCHANGED',
    'WAIT SHIKIGAMI AUTO REPAIR',
    'CHECK SNAPSHOT COMPARISON',
    'OPEN SHIKIGAMI REGISTER',
    'CREATE SHIKIGAMI BACKUP',
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
    'LAUNCH GUI',
    'LOAD BUILD',
    'SAVE BUILD',
    'CLOSE GUI',
    'CALCULATE',
    'OPEN GUI',
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

$allCommandDetails = [System.Collections.Generic.List[object]]::new()
$allLoadOperations = [System.Collections.Generic.List[object]]::new()
$allLoadStages = [System.Collections.Generic.List[object]]::new()
$allFileDialogOperations = [System.Collections.Generic.List[object]]::new()
$allFileDialogStages = [System.Collections.Generic.List[object]]::new()
$allSamples = [System.Collections.Generic.List[object]]::new()
$allScenarios = [System.Collections.Generic.List[object]]::new()
$allSessions = [System.Collections.Generic.List[object]]::new()
$allGuiIntervals = [System.Collections.Generic.List[object]]::new()
$validationIssues = [System.Collections.Generic.List[string]]::new()
$warnings = [System.Collections.Generic.List[string]]::new()

foreach ($runId in $runIds) {
    Write-Host "Processing run $runId..."
    $runDir = Join-Path $downloadRoot $runId
    if (-not $SkipDownload) {
        if (Test-Path $runDir) { Remove-Item -Path $runDir -Recurse -Force }
        New-Item -ItemType Directory -Force -Path $runDir | Out-Null
        & gh run download $runId --repo $repo --name $artifactName --dir $runDir
        if ($LASTEXITCODE -ne 0) { throw "Failed to download log artifact for run $runId. Is the run complete?" }
    }
    if (-not (Test-Path $runDir)) { throw "Artifact directory missing: $runDir" }

    $performanceFiles = @(Get-ChildItem -Path $runDir -Recurse -File -Filter 'ShikigamiApp_Performance_*.log')
    $scenarioFiles = @(Get-ChildItem -Path $runDir -Recurse -File -Filter 'ScenarioRunner_*.log')
    if ($performanceFiles.Count -ne 1) { throw "Expected one Gui performance log for $runId, got $($performanceFiles.Count)." }
    if ($scenarioFiles.Count -eq 0) { throw "No Scenario Runner logs for $runId." }

    $scenarios = [System.Collections.Generic.List[object]]::new()
    foreach ($file in $scenarioFiles) {
        $nameMatch = [regex]::Match($file.Name, '^ScenarioRunner_(\d{8})_(\d{6})_(.+)\.log$')
        if (-not $nameMatch.Success) { $warnings.Add("Unknown scenario log filename: $($file.Name)"); continue }
        $date = $nameMatch.Groups[1].Value
        $start = [datetime]::ParseExact($date+$nameMatch.Groups[2].Value,'yyyyMMddHHmmss',$culture)
        $textLines = @(Get-Content -LiteralPath $file.FullName -Encoding UTF8)
        $status = 'UNKNOWN'
        $declaredName = $nameMatch.Groups[3].Value
        $end = $start
        $stepFailures = 0
        $guiIntervals = [System.Collections.Generic.List[object]]::new()
        $pendingOpen = $null
        foreach ($line in $textLines) {
            if ($line -match '^\[(\d{2}:\d{2}:\d{2}(?:\.\d{3})?)\]') {
                $time = Parse-ScenarioTime $date $Matches[1]
                if ($time -lt $start.AddHours(-12)) { $time = $time.AddDays(1) }
                if ($time -gt $end) { $end = $time }
            }
            # Build Gui lifetime windows from Runner commands, not the whole Scenario interval.
            # Old Runner logs have second precision; new logs have millisecond precision.
            if ($line -match '^\[(\d{2}:\d{2}:\d{2}(?:\.\d{3})?)\] \[([0-9]+)\] (OPEN GUI|LAUNCH GUI|CLOSE GUI)\s+(START|PASS|FAIL)\s*$') {
                $stepTime = Parse-ScenarioTime $date $Matches[1]
                if ($stepTime -lt $start.AddHours(-12)) { $stepTime = $stepTime.AddDays(1) }
                $stepLine = [int]$Matches[2]
                $command = $Matches[3]
                $stepResult = $Matches[4]
                $preciseTime = $Matches[1].Contains('.')
                if (($command -eq 'OPEN GUI' -or $command -eq 'LAUNCH GUI') -and $stepResult -eq 'START') {
                    if ($null -ne $pendingOpen) {
                        $validationIssues.Add("Unclosed Gui window before another launch in $runId / $($file.Name)")
                    }
                    $pendingOpen = [pscustomobject]@{Start=$stepTime; StartLine=$stepLine; Command=$command; Passed=$false; PreciseTime=$preciseTime}
                }
                elseif (($command -eq 'OPEN GUI' -or $command -eq 'LAUNCH GUI') -and $stepResult -eq 'PASS') {
                    if ($null -ne $pendingOpen -and $pendingOpen.StartLine -eq $stepLine) { $pendingOpen.Passed = $true }
                }
                elseif ($command -eq 'CLOSE GUI' -and $stepResult -eq 'PASS') {
                    if ($null -eq $pendingOpen) {
                        $validationIssues.Add("CLOSE GUI without OPEN/LAUNCH GUI in $runId / $($file.Name)")
                    }
                    else {
                        $window = [pscustomobject]@{
                            RunId=$runId; Scenario=$declaredName; FileName=$file.Name;
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
            if ($line -match '^\[\d{2}:\d{2}:\d{2}(?:\.\d{3})?\] Scenario: (.+)$') { $declaredName = $Matches[1] }
            if ($line -match '^\[\d{2}:\d{2}:\d{2}(?:\.\d{3})?\] Scenario Result: (PASS|FAIL)') { $status = $Matches[1] }
            if ($line -match '^\[\d{2}:\d{2}:\d{2}(?:\.\d{3})?\] \[\d+\] .*\sFAIL\s*$') { $stepFailures++ }
        }
        # Additional command timing pass; existing Gui/session analysis remains unchanged.
        $pendingSteps = @{}
        $activeCommand = $null
        foreach ($line in $textLines) {
            if ($line -match '^\[(\d{2}:\d{2}:\d{2}(?:\.\d{3})?)\] \[Timing\] FileDialogWaiter\.WaitForFileDialog (.+)$') {
                $clock = $Matches[1]
                $payload = $Matches[2]
                $fields = @{}
                foreach ($field in [regex]::Matches($payload, '(?<Key>[A-Za-z][A-Za-z0-9]*)=(?<Value>\S+)')) {
                    $fields[$field.Groups['Key'].Value] = $field.Groups['Value'].Value
                }
                $requiredFields = @('Result', 'Route', 'Attempts', 'NativeCandidateAttempts', 'TotalMs')
                if (@($requiredFields | Where-Object { -not $fields.ContainsKey($_) }).Count -gt 0) {
                    $validationIssues.Add("Invalid FileDialog timing fields in $runId / $($file.Name): $line")
                    continue
                }
                if ($null -eq $activeCommand -or -not $pendingSteps.ContainsKey($activeCommand)) {
                    $validationIssues.Add("FileDialog timing without an active command in $runId / $($file.Name): $line")
                    continue
                }
                $pending = $pendingSteps[$activeCommand]
                $stepTime = Parse-ScenarioTime $date $clock
                if ($stepTime -lt $start.AddHours(-12)) { $stepTime = $stepTime.AddDays(1) }
                try {
                    $totalMs = [double]::Parse($fields['TotalMs'], $culture)
                    $attempts = [int]::Parse($fields['Attempts'], $culture)
                    $nativeCandidateAttempts = [int]::Parse($fields['NativeCandidateAttempts'], $culture)
                    if ($totalMs -lt 0 -or $attempts -lt 0 -or $nativeCandidateAttempts -lt 0) { throw 'Negative timing or count value' }
                    $operation = [pscustomobject]@{
                        RunId=$runId;Scenario=$declaredName;FileName=$file.Name;
                        LineNumber=[int]$activeCommand;CommandType=$pending.Type;CommandText=$pending.RawText;
                        Timestamp=$stepTime;Result=$fields['Result'];Route=$fields['Route'];
                        Attempts=$attempts;NativeCandidateAttempts=$nativeCandidateAttempts;TotalMs=$totalMs
                    }
                    $allFileDialogOperations.Add($operation)
                    $stageTotal = 0.0
                    foreach ($name in $fields.Keys) {
                        if ($name -notmatch 'Ms$' -or $name -eq 'TotalMs') { continue }
                        $duration = [double]::Parse($fields[$name], $culture)
                        if ($duration -lt 0) { throw "Negative stage duration: $name" }
                        $stageTotal += $duration
                        $allFileDialogStages.Add([pscustomobject]@{
                            RunId=$runId;Scenario=$declaredName;FileName=$file.Name;
                            LineNumber=[int]$activeCommand;CommandType=$pending.Type;
                            Result=$fields['Result'];Route=$fields['Route'];
                            Stage=$name.Substring(0, $name.Length-2);DurationMs=$duration
                        })
                    }
                    if ([math]::Abs($stageTotal - $totalMs) -gt 3) {
                        $warnings.Add("FileDialog stage total differs from TotalMs in $runId / $($file.Name), line $activeCommand")
                    }
                }
                catch {
                    $validationIssues.Add("Invalid FileDialog timing in $runId / $($file.Name): $($_.Exception.Message)")
                }
                continue
            }
            if ($line -match '^\[(\d{2}:\d{2}:\d{2}(?:\.\d{3})?)\] \[Timing\] SaveDataOperator\.load (.+)$') {
                $clock = $Matches[1]
                $payload = $Matches[2]
                $fields = @{}
                foreach ($field in [regex]::Matches($payload, '(?<Key>[A-Za-z][A-Za-z0-9]*)=(?<Value>\S+)')) {
                    $fields[$field.Groups['Key'].Value] = $field.Groups['Value'].Value
                }
                if (-not $fields.ContainsKey('TotalMs') -or -not $fields.ContainsKey('LoadType') -or -not $fields.ContainsKey('Result') -or -not $fields.ContainsKey('Stage')) {
                    $validationIssues.Add("Invalid SaveData load timing fields in $runId / $($file.Name): $line")
                    continue
                }
                if ($null -eq $activeCommand -or -not $pendingSteps.ContainsKey($activeCommand)) {
                    $validationIssues.Add("SaveData load timing without an active command in $runId / $($file.Name): $line")
                    continue
                }
                $pending = $pendingSteps[$activeCommand]
                $stepTime = Parse-ScenarioTime $date $clock
                if ($stepTime -lt $start.AddHours(-12)) { $stepTime = $stepTime.AddDays(1) }
                try {
                    $totalMs = [double]::Parse($fields['TotalMs'], $culture)
                    $pendingMs = if ($fields.ContainsKey('PendingMs')) { [double]::Parse($fields['PendingMs'], $culture) } else { $null }
                    if ($totalMs -lt 0 -or ($null -ne $pendingMs -and $pendingMs -lt 0)) { throw 'Negative timing value' }
                    $operation = [pscustomobject]@{
                        RunId=$runId;Scenario=$declaredName;FileName=$file.Name;
                        LineNumber=[int]$activeCommand;CommandType=$pending.Type;CommandText=$pending.RawText;
                        Timestamp=$stepTime;LoadType=$fields['LoadType'];Result=$fields['Result'];
                        Stage=$fields['Stage'];TotalMs=$totalMs;PendingMs=$pendingMs
                    }
                    $allLoadOperations.Add($operation)
                    $stageTotal = 0.0
                    foreach ($name in $fields.Keys) {
                        if ($name -notmatch 'Ms$' -or $name -eq 'TotalMs' -or $name -eq 'PendingMs') { continue }
                        $duration = [double]::Parse($fields[$name], $culture)
                        if ($duration -lt 0) { throw "Negative stage duration: $name" }
                        $stageTotal += $duration
                        $allLoadStages.Add([pscustomobject]@{
                            RunId=$runId;Scenario=$declaredName;FileName=$file.Name;
                            LineNumber=[int]$activeCommand;CommandType=$pending.Type;
                            LoadType=$fields['LoadType'];Result=$fields['Result'];
                            Stage=$name.Substring(0, $name.Length-2);DurationMs=$duration
                        })
                    }
                    if ([math]::Abs(($stageTotal + $(if ($null -ne $pendingMs) { $pendingMs } else { 0 })) - $totalMs) -gt 3) {
                        $warnings.Add("SaveData load stage total differs from TotalMs in $runId / $($file.Name), line $activeCommand")
                    }
                }
                catch {
                    $validationIssues.Add("Invalid SaveData load timing in $runId / $($file.Name): $($_.Exception.Message)")
                }
                continue
            }
            if ($line -notmatch '^\[(\d{2}:\d{2}:\d{2}(?:\.\d{3})?)\] \[(\d+)\] (.*?)\s{4}(START|PASS|FAIL)\s*$') { continue }
            $clock = $Matches[1]
            $lineNumber = [int]$Matches[2]
            $rawCommand = $Matches[3]
            $result = $Matches[4]
            $stepTime = Parse-ScenarioTime $date $clock
            if ($stepTime -lt $start.AddHours(-12)) { $stepTime = $stepTime.AddDays(1) }
            $commandType = Get-ScenarioCommandType $rawCommand
            $key = [string]$lineNumber
            if ($result -eq 'START') {
                if ($pendingSteps.ContainsKey($key)) {
                    $validationIssues.Add("Duplicate START for command line $lineNumber in $runId / $($file.Name)")
                }
                $pendingSteps[$key] = [pscustomobject]@{Time=$stepTime;RawText=$rawCommand;Type=$commandType;Precise=$clock.Contains('.')}
                $activeCommand = $key
                continue
            }
            if (-not $pendingSteps.ContainsKey($key)) {
                $validationIssues.Add("$result without START for command line $lineNumber in $runId / $($file.Name)")
                continue
            }
            $pending = $pendingSteps[$key]
            $pendingSteps.Remove($key)
            if ($activeCommand -eq $key) { $activeCommand = $null }
            if ($pending.RawText -ne $rawCommand) {
                $validationIssues.Add("Command text mismatch at line $lineNumber in $runId / $($file.Name)")
            }
            $elapsedMs = ($stepTime - $pending.Time).TotalMilliseconds
            if ($elapsedMs -lt 0) {
                $validationIssues.Add("Negative command duration at line $lineNumber in $runId / $($file.Name)")
                continue
            }
            $allCommandDetails.Add([pscustomobject]@{
                RunId=$runId;Scenario=$declaredName;FileName=$file.Name;LineNumber=$lineNumber;
                CommandType=$pending.Type;CommandText=$rawCommand;Result=$result;
                Start=$pending.Time;End=$stepTime;DurationMs=[math]::Round($elapsedMs,3);
                PreciseTime=($pending.Precise -and $clock.Contains('.'))
            })
        }
        foreach ($key in $pendingSteps.Keys) {
            $pending = $pendingSteps[$key]
            $allCommandDetails.Add([pscustomobject]@{
                RunId=$runId;Scenario=$declaredName;FileName=$file.Name;LineNumber=[int]$key;
                CommandType=$pending.Type;CommandText=$pending.RawText;Result='INCOMPLETE';
                Start=$pending.Time;End=$null;DurationMs=$null;PreciseTime=$pending.Precise
            })
        }
        if ($null -ne $pendingOpen) { $validationIssues.Add("Gui open without successful close in $runId / $($file.Name)") }
        foreach ($window in $guiIntervals) {
            $window.Scenario = $declaredName
            $allGuiIntervals.Add($window)
        }
        $s = [pscustomobject]@{RunId=$runId; Scenario=$declaredName; Start=$start; End=$end; Result=$status; FailedSteps=$stepFailures; GuiWindows=$guiIntervals.Count; FileName=$file.Name}
        $scenarios.Add($s)
        $allScenarios.Add($s)
    }

    # The performance log is a set of session blocks. Every target in one block
    # holds an aggregate over the lifetime of one Gui.exe process.
    $sessionBlocks = [System.Collections.Generic.List[object]]::new()
    $current = $null
    foreach ($line in (Get-Content -LiteralPath $performanceFiles[0].FullName -Encoding UTF8)) {
        if ($line -match '^(\d{4}-\d\d-\d\d \d\d:\d\d:\d\d\.\d{3}) Session=([0-9a-f]{32}) Event=Begin$') {
            if ($null -ne $current) { $warnings.Add("Unclosed performance session in $runId") }
            $current = [pscustomobject]@{Id=$Matches[2]; EndTime=(Parse-Time $Matches[1]); Samples=[System.Collections.Generic.List[object]]::new()}
            continue
        }
        if ($line -match '^\d{4}-\d\d-\d\d \d\d:\d\d:\d\d\.\d{3} Session=([0-9a-f]{32}) Target=(\S+) Count=(\d+) TotalMs=([\d.]+) AverageMs=([\d.]+) MaxMs=([\d.]+)$') {
            if ($null -eq $current -or $current.Id -ne $Matches[1]) {
                $warnings.Add("Orphan performance target in run $runId")
                continue
            }
            $current.Samples.Add([pscustomobject]@{Target=$Matches[2]; Count=[long]$Matches[3]; TotalMs=[double]::Parse($Matches[4],$culture); AverageMs=[double]::Parse($Matches[5],$culture); MaxMs=[double]::Parse($Matches[6],$culture)})
            continue
        }
        if ($line -match '^(\d{4}-\d\d-\d\d \d\d:\d\d:\d\d\.\d{3}) Session=([0-9a-f]{32}) Event=End$') {
            if ($null -ne $current -and $current.Id -eq $Matches[2]) {
                $current.EndTime = Parse-Time $Matches[1]
                $sessionBlocks.Add($current)
            } else { $warnings.Add("Unmatched session end in run $runId") }
            $current = $null
        }
    }

    $orderedSessions = @($sessionBlocks | Sort-Object EndTime)
    $runWindows = @($allGuiIntervals | Where-Object { $_.RunId -eq $runId } | Sort-Object Start,End)
    $usedWindows = @{}
    for ($i = 0; $i -lt $orderedSessions.Count; $i++) {
        $session = $orderedSessions[$i]
        # Performance is written at Gui process exit. Match against the CLOSE GUI
        # command of a concrete OPEN/LAUNCH -> CLOSE window. Windows and sessions
        # must be one-to-one. Preserve wider bounds for legacy second-precision logs.
        $candidates = @($runWindows | Where-Object {
            -not $usedWindows.ContainsKey("$($_.FileName):$($_.WindowOrder)") -and
            $session.EndTime -ge $_.Start.AddSeconds($(if ($_.PreciseTime) { -0.25 } else { -1 })) -and
            $session.EndTime -le $_.End.AddSeconds($(if ($_.PreciseTime) { 0.5 } else { 2 }))
        })
        $scenarioName = ''
        $matchQuality = 'unmatched'
        $matchedWindow = $null
        if ($candidates.Count -eq 1) {
            $matchedWindow = $candidates[0]
            $matchQuality = 'window-time'
        }
        elseif ($candidates.Count -gt 1) {
            # Prefer a single closest CLOSE timestamp. If timestamps are tied
            # within the log precision tolerance, preserve ambiguity rather than guess.
            $ranked = @($candidates | Sort-Object @{Expression={ [math]::Abs(($_.End - $session.EndTime).TotalMilliseconds) }})
            $firstDistance = [math]::Abs(($ranked[0].End - $session.EndTime).TotalMilliseconds)
            $secondDistance = [math]::Abs(($ranked[1].End - $session.EndTime).TotalMilliseconds)
            if (($secondDistance - $firstDistance) -ge $(if ($ranked[0].PreciseTime -and $ranked[1].PreciseTime) { 100 } else { 1000 })) {
                $matchedWindow = $ranked[0]
                $matchQuality = 'window-time'
            }
            else {
                $matchQuality = 'ambiguous'
                $warnings.Add("Multiple Gui windows match performance session $($session.Id) in $runId")
            }
        }
        if ($null -ne $matchedWindow) {
            $scenarioName = $matchedWindow.Scenario
            $matchedWindow.SessionId = $session.Id
            $matchedWindow.MatchQuality = $matchQuality
            $usedWindows["$($matchedWindow.FileName):$($matchedWindow.WindowOrder)"] = $true
        }
        elseif ($matchQuality -eq 'unmatched') {
            $warnings.Add("No Gui window matches performance session $($session.Id) in $runId")
        }
        $sessionRow = [pscustomobject]@{
            RunId=$runId; SessionOrder=($i+1); SessionId=$session.Id;
            EndTime=$session.EndTime.ToString('yyyy-MM-dd HH:mm:ss.fff');
            Scenario=$scenarioName; MatchQuality=$matchQuality;
            ScenarioFile=if($matchedWindow){$matchedWindow.FileName}else{''};
            WindowOrder=if($matchedWindow){$matchedWindow.WindowOrder}else{$null}
        }
        $allSessions.Add($sessionRow)
        foreach ($sample in $session.Samples) {
            $allSamples.Add([pscustomobject]@{
                RunId=$runId; SessionOrder=($i+1); SessionId=$session.Id; Scenario=$scenarioName; MatchQuality=$matchQuality;
                Target=$sample.Target; Count=$sample.Count; TotalMs=$sample.TotalMs; AverageMs=$sample.AverageMs; MaxMs=$sample.MaxMs
            })
        }
    }
    $unmatchedWindows = @($runWindows | Where-Object { -not $_.SessionId })
    if ($unmatchedWindows.Count -gt 0) { $warnings.Add("Run ${runId}: $($unmatchedWindows.Count) Gui open/close windows have no Performance session (not necessarily an error).") }
    if ($scenarios.Count -ne 45) { $validationIssues.Add("Run ${runId}: expected 45 scenarios, got $($scenarios.Count)") }
    if (@($scenarios | Where-Object { $_.Result -ne 'PASS' }).Count -gt 0) { $validationIssues.Add("Run ${runId}: one or more scenarios did not PASS") }
    if ($orderedSessions.Count -eq 0) { $validationIssues.Add("Run ${runId}: no Gui Performance sessions") }
    $duplicateSamples = @($orderedSessions | ForEach-Object {
        $sessionId = $_.Id
        @($_.Samples | Group-Object Target | Where-Object { $_.Count -gt 1 } | ForEach-Object { "$sessionId/$($_.Name)" })
    })
    if ($duplicateSamples.Count -gt 0) { $validationIssues.Add("Run ${runId}: duplicate Target rows within Gui sessions") }
    foreach ($session in $orderedSessions) {
        if ($session.Samples.Count -eq 0) { $validationIssues.Add("Run ${runId}: empty Gui Performance session $($session.Id)") }
        foreach ($sample in $session.Samples) {
            if ($sample.Count -le 0 -or $sample.TotalMs -lt 0 -or $sample.MaxMs -lt 0) {
                $validationIssues.Add("Run ${runId}: invalid measurement in $($session.Id) / $($sample.Target)")
            }
            if ($sample.Count -gt 0 -and [math]::Abs($sample.AverageMs - $sample.TotalMs/$sample.Count) -gt 0.003) {
                $validationIssues.Add("Run ${runId}: inconsistent average in $($session.Id) / $($sample.Target)")
            }
        }
    }

    if (@($orderedSessions | Group-Object Id | Where-Object { $_.Count -gt 1 }).Count -gt 0) { $validationIssues.Add("Run ${runId}: duplicate Performance session IDs") }
    if (@($scenarios | Where-Object {$_.Result -ne 'PASS'}).Count -gt 0) { $warnings.Add("Run $runId contains failed/unknown scenarios") }
    Write-Host "  Scenario logs: $($scenarios.Count); Gui sessions: $($orderedSessions.Count)"
}

$summary = [System.Collections.Generic.List[object]]::new()
# Count/total are aggregated inside each run FIRST. The five-run mean is then
# computed from run totals, not a pool of sessions with uneven run sizes.
$perRun = [System.Collections.Generic.List[object]]::new()
foreach ($runId in $runIds) {
    $runSamples = @($allSamples | Where-Object { $_.RunId -eq $runId })
    foreach ($g in ($runSamples | Group-Object Target)) {
        $values = @($g.Group)
        $perRun.Add([pscustomobject]@{RunId=$runId;Target=$g.Name;Count=(($values | Measure-Object Count -Sum).Sum);TotalMs=(($values | Measure-Object TotalMs -Sum).Sum);MaxMs=(($values | Measure-Object MaxMs -Maximum).Maximum)})
    }
}
foreach ($g in ($perRun | Group-Object Target)) {
    $byRun = @($g.Group)
    $totals = @($byRun | ForEach-Object { $_.TotalMs })
    $counts = @($byRun | ForEach-Object { $_.Count })
    $summary.Add([pscustomobject]@{
        Target=$g.Name; Runs=$byRun.Count; MeanCount=[math]::Round(($counts | Measure-Object -Average).Average,2);
        MeanTotalMs=[math]::Round(($totals | Measure-Object -Average).Average,3);
        MinTotalMs=[math]::Round(($totals | Measure-Object -Minimum).Minimum,3);
        MaxTotalMs=[math]::Round(($totals | Measure-Object -Maximum).Maximum,3);
        MeanPerCallMs=[math]::Round((($byRun | ForEach-Object { if ($_.Count -gt 0) { $_.TotalMs/$_.Count } }) | Measure-Object -Average).Average,3)
    })
}

$startup = [System.Collections.Generic.List[object]]::new()
foreach ($g in ($allSamples | Group-Object RunId,Target)) {
    $values = @($g.Group | Sort-Object SessionOrder)
    $first = @($values | Where-Object {$_.SessionOrder -eq 1})
    $later = @($values | Where-Object {$_.SessionOrder -gt 1})
    $startup.Add([pscustomobject]@{
        RunId=$values[0].RunId;Target=$values[0].Target;
        FirstMs=if($first.Count){[math]::Round($first[0].TotalMs,3)}else{$null};
        LaterSessions=$later.Count;
        LaterAverageMs=if($later.Count){[math]::Round((($later|Measure-Object TotalMs -Average).Average),3)}else{$null}
    })
}

# Command summaries use successful commands only; failed/incomplete commands remain in details.
$passedCommands = @($allCommandDetails | Where-Object { $_.Result -eq 'PASS' })
$commandSummary = [System.Collections.Generic.List[object]]::new()
foreach ($g in ($passedCommands | Group-Object CommandType)) {
    $durations = @($g.Group | ForEach-Object { $_.DurationMs })
    $runTotals = @($g.Group | Group-Object RunId | ForEach-Object {
        [math]::Round(($_.Group | Measure-Object DurationMs -Sum).Sum,3)
    })
    $commandSummary.Add([pscustomobject]@{
        CommandType=$g.Name;Count=$g.Count;Runs=@($g.Group | Select-Object -ExpandProperty RunId -Unique).Count;
        TotalMs=[math]::Round(($durations | Measure-Object -Sum).Sum,3);
        MeanTotalMsPerRun=[math]::Round(($runTotals | Measure-Object -Average).Average,3);
        MeanMs=[math]::Round(($durations | Measure-Object -Average).Average,3);
        MinMs=[math]::Round(($durations | Measure-Object -Minimum).Minimum,3);
        MaxMs=[math]::Round(($durations | Measure-Object -Maximum).Maximum,3)
    })
}
$commandByScenario = [System.Collections.Generic.List[object]]::new()
foreach ($g in ($passedCommands | Group-Object RunId,Scenario,CommandType)) {
    $first = $g.Group[0]
    $commandByScenario.Add([pscustomobject]@{
        RunId=$first.RunId;Scenario=$first.Scenario;CommandType=$first.CommandType;
        Count=$g.Count;TotalMs=[math]::Round(($g.Group | Measure-Object DurationMs -Sum).Sum,3);
        MeanMs=[math]::Round(($g.Group | Measure-Object DurationMs -Average).Average,3);
        MaxMs=[math]::Round(($g.Group | Measure-Object DurationMs -Maximum).Maximum,3)
    })
}
# Detect missing nested FileDialog instrumentation instead of silently omitting it.
# Legacy batches without the new GetMainLoadElement stage remain supported.
$instrumentedLoads = @($allLoadOperations | Where-Object {
    $operation = $_
    $_.Result -eq 'PASS' -and @($allLoadStages | Where-Object {
        $_.RunId -eq $operation.RunId -and
        $_.FileName -eq $operation.FileName -and
        $_.LineNumber -eq $operation.LineNumber -and
        $_.Stage -eq 'GetMainLoadElement'
    }).Count -gt 0
})
if ($instrumentedLoads.Count -gt 0) {
    $fileDialogKeys = @{}
    foreach ($operation in $allFileDialogOperations) {
        $key = "$($operation.RunId)|$($operation.FileName)|$($operation.LineNumber)"
        $fileDialogKeys[$key] = $true
    }
    foreach ($operation in $instrumentedLoads) {
        $key = "$($operation.RunId)|$($operation.FileName)|$($operation.LineNumber)"
        if (-not $fileDialogKeys.ContainsKey($key)) {
            $validationIssues.Add("Missing FileDialogWaiter timing for $($operation.RunId) / $($operation.FileName), line $($operation.LineNumber)")
        }
    }
}

# SaveData load timing records are parsed from Runner logs and linked to their active command.
# Both the stage list and load types are dynamic; newly added stage measurements need no parser changes.
$loadStageSummary = [System.Collections.Generic.List[object]]::new()
$loadStageGroups = @(
    @($allLoadStages | Group-Object Stage | ForEach-Object {
        [pscustomobject]@{LoadType='ALL';Stage=$_.Name;Values=@($_.Group)}
    })
    @($allLoadStages | Group-Object LoadType,Stage | ForEach-Object {
        $first = $_.Group[0]
        [pscustomobject]@{LoadType=$first.LoadType;Stage=$first.Stage;Values=@($_.Group)}
    })
)
foreach ($group in $loadStageGroups) {
    $values = @($group.Values | Sort-Object DurationMs)
    if ($values.Count -eq 0) { continue }
    $durations = @($values | ForEach-Object { $_.DurationMs })
    $middle = [int][math]::Floor($durations.Count / 2)
    $median = if ($durations.Count % 2 -eq 0) { ($durations[$middle-1] + $durations[$middle]) / 2 } else { $durations[$middle] }
    $runTotals = @($values | Group-Object RunId | ForEach-Object { ($_.Group | Measure-Object DurationMs -Sum).Sum })
    $loadStageSummary.Add([pscustomobject]@{
        LoadType=$group.LoadType;Stage=$group.Stage;Count=$values.Count;
        Runs=@($values | Select-Object -ExpandProperty RunId -Unique).Count;
        TotalMs=[math]::Round(($durations | Measure-Object -Sum).Sum,3);
        MeanTotalMsPerRun=[math]::Round(($runTotals | Measure-Object -Average).Average,3);
        MeanMs=[math]::Round(($durations | Measure-Object -Average).Average,3);
        MedianMs=[math]::Round($median,3);
        MinMs=[math]::Round($durations[0],3);
        MaxMs=[math]::Round($durations[-1],3)
    })
}
# FileDialogWaiter timing covers SaveData load/save and Snapshot comparison.
# Stages are discovered dynamically so further measurements need no parser changes.
$fileDialogStageSummary = [System.Collections.Generic.List[object]]::new()
foreach ($g in ($allFileDialogStages | Group-Object Stage)) {
    $values = @($g.Group | Sort-Object DurationMs)
    $durations = @($values | ForEach-Object { $_.DurationMs })
    $middle = [int][math]::Floor($durations.Count / 2)
    $median = if ($durations.Count % 2 -eq 0) { ($durations[$middle-1] + $durations[$middle]) / 2 } else { $durations[$middle] }
    $fileDialogStageSummary.Add([pscustomobject]@{
        Stage=$g.Name;Count=$values.Count;
        TotalMs=[math]::Round(($durations | Measure-Object -Sum).Sum,3);
        MeanMs=[math]::Round(($durations | Measure-Object -Average).Average,3);
        MedianMs=[math]::Round($median,3);
        MaxMs=[math]::Round($durations[-1],3)
    })
}
$fileDialogRouteSummary = [System.Collections.Generic.List[object]]::new()
foreach ($g in ($allFileDialogOperations | Group-Object CommandType,Route)) {
    $first = $g.Group[0]
    $fileDialogRouteSummary.Add([pscustomobject]@{
        CommandType=$first.CommandType;Route=$first.Route;Count=$g.Count;
        MeanMs=[math]::Round(($g.Group | Measure-Object TotalMs -Average).Average,3);
        MaxMs=[math]::Round(($g.Group | Measure-Object TotalMs -Maximum).Maximum,3);
        TotalMs=[math]::Round(($g.Group | Measure-Object TotalMs -Sum).Sum,3)
    })
}
Export-Rows @($allFileDialogOperations | Sort-Object RunId,FileName,LineNumber,Timestamp) (Join-Path $OutputDirectory 'FileDialogTimingOperations.csv')
Export-Rows @($allFileDialogStages | Sort-Object RunId,FileName,LineNumber,Stage) (Join-Path $OutputDirectory 'FileDialogTimingStages.csv')
Export-Rows @($fileDialogStageSummary | Sort-Object TotalMs -Descending) (Join-Path $OutputDirectory 'FileDialogTimingStageSummary.csv')
Export-Rows @($fileDialogRouteSummary | Sort-Object CommandType,Route) (Join-Path $OutputDirectory 'FileDialogTimingRoutes.csv')

Export-Rows @($allLoadOperations | Sort-Object RunId,FileName,LineNumber) (Join-Path $OutputDirectory 'SaveDataLoadOperations.csv')
Export-Rows @($allLoadStages | Sort-Object RunId,FileName,LineNumber,Stage) (Join-Path $OutputDirectory 'SaveDataLoadStages.csv')
Export-Rows @($loadStageSummary | Sort-Object LoadType,TotalMs -Descending) (Join-Path $OutputDirectory 'SaveDataLoadStageSummary.csv')

Export-Rows @($allCommandDetails | Sort-Object RunId,FileName,LineNumber) (Join-Path $OutputDirectory 'ScenarioCommandDetails.csv')
Export-Rows @($commandSummary | Sort-Object TotalMs -Descending) (Join-Path $OutputDirectory 'ScenarioCommandSummary.csv')
Export-Rows @($commandByScenario | Sort-Object RunId,Scenario,CommandType) (Join-Path $OutputDirectory 'ScenarioCommandByScenario.csv')

Export-Rows @($allSamples) (Join-Path $OutputDirectory 'PerformanceSessions.csv')
Export-Rows @($perRun) (Join-Path $OutputDirectory 'PerformancePerRun.csv')
Export-Rows @($summary | Sort-Object MeanTotalMs -Descending) (Join-Path $OutputDirectory 'PerformanceSummary.csv')
Export-Rows @($startup | Sort-Object RunId,Target) (Join-Path $OutputDirectory 'PerformanceStartup.csv')
Export-Rows @($allSessions) (Join-Path $OutputDirectory 'SessionMapping.csv')
Export-Rows @($allGuiIntervals | Select-Object RunId,Scenario,FileName,WindowOrder,OpenCommand,OpenLine,CloseLine,Start,End,OpenPassed,PreciseTime,SessionId,MatchQuality) (Join-Path $OutputDirectory 'GuiWindows.csv')
Export-Rows @($allScenarios) (Join-Path $OutputDirectory 'ScenarioResults.csv')

$md = [System.Collections.Generic.List[string]]::new()
$md.Add('# Scenario Runner Batch performance report')
$md.Add('')
$md.Add('Run IDs: '+($runIds -join ', '))
$md.Add('')
$md.Add('## Scenario results')
$md.Add('')
$md.Add('| Run ID | PASS | FAIL/UNKNOWN | Gui windows | Gui sessions | Unmatched/ambiguous |')
$md.Add('|---|---:|---:|---:|---:|---:|')
foreach($runId in $runIds) {
    $s=@($allScenarios|Where-Object{$_.RunId -eq $runId});$ss=@($allSessions|Where-Object{$_.RunId -eq $runId})
    $passed=@($s|Where-Object{$_.Result -eq 'PASS'}).Count
    $uncertain=@($ss|Where-Object{$_.MatchQuality -ne 'window-time'}).Count
    $windows=@($allGuiIntervals|Where-Object{$_.RunId -eq $runId})
    $md.Add("| $runId | $passed | $($s.Count-$passed) | $($windows.Count) | $($ss.Count) | $uncertain |")
}
$md.Add('')
$md.Add('## Validation')
$md.Add('')
if ($validationIssues.Count -eq 0) { $md.Add('PASS: required structural checks completed without errors.') }
else { $md.Add('FAIL: structural issues were detected.'); foreach ($issue in $validationIssues) { $md.Add('- '+$issue) } }
$md.Add('')
$md.Add('Gui window counts and Performance session counts can legitimately differ; see GuiWindows.csv for unmatched intervals.')
$md.Add('')
$md.Add('## Target totals (mean of per-run totals)')
$md.Add('')
$md.Add('| Target | Runs | Mean count | Mean total ms | Mean ms/call | Min total ms | Max total ms |')
$md.Add('|---|---:|---:|---:|---:|---:|---:|')
foreach ($s in ($summary|Sort-Object MeanTotalMs -Descending)) {
    $md.Add("| $($s.Target) | $($s.Runs) | $($s.MeanCount) | $($s.MeanTotalMs) | $($s.MeanPerCallMs) | $($s.MinTotalMs) | $($s.MaxTotalMs) |")
}
$md.Add('')
$md.Add('## Scenario command timing (successful commands)')
$md.Add('')
$md.Add('| Command | Count | Total ms | Mean ms | Max ms |')
$md.Add('|---|---:|---:|---:|---:|')
foreach ($c in ($commandSummary | Sort-Object TotalMs -Descending)) {
    $md.Add("| $($c.CommandType) | $($c.Count) | $($c.TotalMs) | $($c.MeanMs) | $($c.MaxMs) |")
}
$md.Add('')
$md.Add('## SaveData load timing (Runner instrumentation)')
$md.Add('')
$md.Add("Recorded load operations: $($allLoadOperations.Count); recorded stages: $($allLoadStages.Count).")
$md.Add('')
$md.Add('| Stage | Count | Total ms | Mean ms | Median ms | Max ms |')
$md.Add('|---|---:|---:|---:|---:|---:|')
foreach ($row in ($loadStageSummary | Where-Object { $_.LoadType -eq 'ALL' } | Sort-Object TotalMs -Descending)) {
    $md.Add("| $($row.Stage) | $($row.Count) | $($row.TotalMs) | $($row.MeanMs) | $($row.MedianMs) | $($row.MaxMs) |")
}
$md.Add('')
$md.Add('Per-load-type breakdown and individual records are in SaveDataLoadStageSummary.csv, SaveDataLoadStages.csv, and SaveDataLoadOperations.csv.')
$md.Add('')
$md.Add('Analysis parser: FileDialogWaiter timing supported (2026-10-24).')
$md.Add('')
$md.Add('## FileDialogWaiter timing (Runner instrumentation)')
$md.Add('')
$md.Add("Recorded FileDialog operations: $($allFileDialogOperations.Count); recorded stages: $($allFileDialogStages.Count).")
$md.Add('')
$md.Add('| Stage | Count | Total ms | Mean ms | Median ms | Max ms |')
$md.Add('|---|---:|---:|---:|---:|---:|')
foreach ($row in ($fileDialogStageSummary | Sort-Object TotalMs -Descending)) {
    $md.Add("| $($row.Stage) | $($row.Count) | $($row.TotalMs) | $($row.MeanMs) | $($row.MedianMs) | $($row.MaxMs) |")
}
$md.Add('')
$md.Add('The successful search routes and command-type breakdown are in FileDialogTimingRoutes.csv.')
$md.Add('FileDialogWaiter timing is nested within caller timing (e.g., SaveDataOperator.load); do not add their totals.')
$md.Add('')
$md.Add('## Caveats')
$md.Add('- Performance totals contain nested measurement scopes; do not add parent/child totals.')
$md.Add('- Elapsed time includes UI waits and modal dialogs; it is not CPU time.')
$md.Add('- Scenario-to-Gui mapping uses OPEN GUI / LAUNCH GUI to CLOSE GUI execution windows. Old logs use second precision; new logs use millisecond precision. Window-time remains inferred, not ID-confirmed.')
$md.Add('- First vs later Gui sessions are separately available in PerformanceStartup.csv.')
$md.Add('- Per-run means are used to reduce distortions from different session counts.')
$md.Add('- Command timings are START-to-PASS log timestamp differences and include logging overhead; they are not CPU timings.')
$md.Add('- Command timing summaries exclude FAIL/INCOMPLETE. Inspect ScenarioCommandDetails.csv for these records.')
$md.Add('- SaveData load timing is emitted by the Runner, not the Gui PerformanceTracker. The breakdown covers only instrumented load() calls; older logs may have no records.')
$md.Add('- FileDialogWaiter timing covers only calls supplied with a ScenarioLogger; older logs and other callers may have no records.')
if($warnings.Count -gt 0) {
    $md.Add('');$md.Add('## Warnings');foreach ($warning in $warnings) { $md.Add('- '+$warning) }
}
[System.IO.File]::WriteAllLines((Join-Path $OutputDirectory 'PerformanceReport.md'), $md, [System.Text.UTF8Encoding]::new($false))

# Package only the analysis outputs for ChatGPT. Raw artifacts and previously
# generated ZIPs are deliberately excluded.
$reportFiles = @(
    'PerformanceReport.md',
    'PerformanceSessions.csv',
    'PerformancePerRun.csv',
    'PerformanceSummary.csv',
    'PerformanceStartup.csv',
    'ScenarioResults.csv',
    'ScenarioCommandDetails.csv',
    'ScenarioCommandSummary.csv',
    'ScenarioCommandByScenario.csv',
    'SessionMapping.csv',
    'GuiWindows.csv',
    'SaveDataLoadOperations.csv',
    'SaveDataLoadStages.csv',
    'SaveDataLoadStageSummary.csv',
    'FileDialogTimingOperations.csv',
    'FileDialogTimingStages.csv',
    'FileDialogTimingStageSummary.csv',
    'FileDialogTimingRoutes.csv'
)
$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$zipPath = Join-Path $OutputDirectory "ScenarioBatchAnalysis_$timestamp.zip"
if (Test-Path -LiteralPath $zipPath) {
    throw "Report ZIP already exists: $zipPath"
}
$filesToArchive = @($reportFiles | ForEach-Object { Join-Path $OutputDirectory $_ })
foreach ($file in $filesToArchive) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) {
        throw "Required analysis file is missing: $file"
    }
}
Compress-Archive -LiteralPath $filesToArchive -DestinationPath $zipPath -CompressionLevel Optimal
Write-Host "SaveData load timing: $($allLoadOperations.Count) operations / $($allLoadStages.Count) stages"
Write-Host "FileDialog timing: $($allFileDialogOperations.Count) operations / $($allFileDialogStages.Count) stages"
if ($validationIssues.Count -gt 0) {
    Write-Warning "Analysis found $($validationIssues.Count) structural issue(s). See PerformanceReport.md."
}
Write-Host "Analysis complete: $OutputDirectory"
Write-Host "ChatGPT package: $zipPath"
Write-Host 'Send the generated ZIP to ChatGPT for interpretation.'

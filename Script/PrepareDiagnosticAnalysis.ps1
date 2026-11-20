param(
    [Parameter(Mandatory)]
    [string]$RunId,

    [Parameter(Mandatory)]
    [string]$RunDirectory,

    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'Analysis/ScenarioSessionParser.ps1')
. (Join-Path $PSScriptRoot 'Analysis/ScenarioCommandParser.ps1')
. (Join-Path $PSScriptRoot 'Analysis/PerformanceSessionParser.ps1')
. (Join-Path $PSScriptRoot 'Analysis/SessionMatcher.ps1')

$culture = [System.Globalization.CultureInfo]::InvariantCulture

if ($RunId -notmatch '^\d+$') { throw "RunId must be numeric: $RunId" }
if (-not (Test-Path -LiteralPath $RunDirectory -PathType Container)) {
    throw "Run directory does not exist: $RunDirectory"
}

$runRoot = (Resolve-Path -LiteralPath $RunDirectory).ProviderPath
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $runRoot 'DiagnosticAnalysisInput.json'
}
else {
    $OutputPath = [System.IO.Path]::GetFullPath($OutputPath)
    $outputParent = [System.IO.Path]::GetDirectoryName($OutputPath)
    if (-not [string]::Equals($outputParent.TrimEnd('\', '/'), $runRoot.TrimEnd('\', '/'), [System.StringComparison]::OrdinalIgnoreCase)) {
        throw 'OutputPath must be directly under RunDirectory so relative source paths remain portable.'
    }
}

function Get-RelativeRunPath([string]$fullName) {
    $root = $runRoot.TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
    if (-not $fullName.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Input file is outside RunDirectory: $fullName"
    }
    return $fullName.Substring($root.Length).Replace('\', '/')
}

function Format-LocalTime([datetime]$value) {
    return $value.ToString('yyyy-MM-ddTHH:mm:ss.fff', $culture)
}

$scenarioFiles = @(Get-ChildItem -LiteralPath $runRoot -Recurse -File -Filter 'ScenarioRunner_*.log' | Sort-Object FullName)
$performanceFiles = @(Get-ChildItem -LiteralPath $runRoot -Recurse -File -Filter 'ShikigamiApp_Performance_*.log' | Sort-Object FullName)
$diagnosticFiles = @(Get-ChildItem -LiteralPath $runRoot -Recurse -File -Filter 'ShikigamiApp_*.log' |
    Where-Object { $_.Name -notlike 'ShikigamiApp_Performance_*' } | Sort-Object FullName)

if ($scenarioFiles.Count -eq 0) { throw "No ScenarioRunner logs in $runRoot" }
if ($performanceFiles.Count -gt 1) { throw "Expected at most one Performance log, found $($performanceFiles.Count)." }

$issues = [System.Collections.Generic.List[string]]::new()
$windows = [System.Collections.Generic.List[object]]::new()
$scenarioResults = [System.Collections.Generic.List[object]]::new()

foreach ($file in $scenarioFiles) {
    $match = [regex]::Match($file.Name, '^ScenarioRunner_(\d{8})_(\d{6})_(.+)\.log$')
    if (-not $match.Success) {
        $issues.Add("Unknown ScenarioRunner log filename: $($file.Name)")
        continue
    }

    $date = $match.Groups[1].Value
    $start = [datetime]::ParseExact($date + $match.Groups[2].Value, 'yyyyMMddHHmmss', $culture)
    $end = $start
    $scenarioName = $match.Groups[3].Value
    $result = 'UNKNOWN'
    $lines = @(Get-Content -LiteralPath $file.FullName -Encoding UTF8)

    foreach ($line in $lines) {
        if ($line -match '^\[(\d{2}:\d{2}:\d{2}(?:\.\d{3})?)\]') {
            $time = Parse-ScenarioTime $date $Matches[1]
            if ($time -lt $start.AddHours(-12)) { $time = $time.AddDays(1) }
            if ($time -gt $end) { $end = $time }
        }
        if ($line -match '^\[\d{2}:\d{2}:\d{2}(?:\.\d{3})?\] Scenario: (.+)$') {
            $scenarioName = $Matches[1]
        }
        if ($line -match '^\[\d{2}:\d{2}:\d{2}(?:\.\d{3})?\] Scenario Result: (PASS|FAIL)') {
            $result = $Matches[1]
        }
    }

    $guiResult = Get-ScenarioGuiWindows -RunId $RunId -FileName $file.Name -Date $date `
        -Start $start -Scenario $scenarioName -Lines $lines
    foreach ($issue in $guiResult.ValidationIssues) { $issues.Add($issue) }
    if ($guiResult.HasPendingOpen) {
        $issues.Add("Gui open without successful close in $RunId / $($file.Name)")
    }
    foreach ($window in $guiResult.GuiWindows) {
        $windows.Add($window)
    }

    $details = [System.Collections.Generic.List[object]]::new()
    $commandState = New-ScenarioCommandState -RunId $RunId -FileName $file.Name -Date $date `
        -Start $start -Scenario $scenarioName -CommandDetails $details -ValidationIssues $issues
    foreach ($line in $lines) {
        Update-ScenarioCommandState -State $commandState -Line $line
    }
    Complete-ScenarioCommandState -State $commandState

    $commands = @($details | Sort-Object LineNumber | ForEach-Object {
        [ordered]@{
            lineNumber = $_.LineNumber
            type = $_.CommandType
            text = $_.CommandText
            result = $_.Result
            start = Format-LocalTime $_.Start
            end = if ($null -ne $_.End) { Format-LocalTime $_.End } else { $null }
            preciseTime = [bool]$_.PreciseTime
        }
    })
    $scenarioResults.Add([pscustomobject]@{
        scenario = $scenarioName
        sourceFile = (Get-RelativeRunPath $file.FullName)
        result = $result
        start = (Format-LocalTime $start)
        end = (Format-LocalTime $end)
        commands = $commands
        guiWindows = @()
    })
}

$sessionBlocks = @()
if ($performanceFiles.Count -eq 1) {
    $performance = Get-PerformanceSessions -RunId $RunId -Path $performanceFiles[0].FullName
    $sessionBlocks = @($performance.Sessions)
    foreach ($warning in $performance.Warnings) { $issues.Add($warning) }
}
else { $issues.Add("No Performance log in $RunId") }
if ($diagnosticFiles.Count -eq 0) {
    $issues.Add("No Gui diagnostic log in $RunId (diagnostic event validation will be inconclusive)")
}

$matchResult = Match-GuiSessions -RunId $RunId -Sessions $sessionBlocks -GuiWindows @($windows.ToArray())
foreach ($warning in $matchResult.Warnings) { $issues.Add($warning) }

# The matcher fills SessionId and MatchQuality on the existing window objects.
foreach ($scenario in $scenarioResults) {
    $fileName = [System.IO.Path]::GetFileName($scenario.sourceFile)
    $scenario.guiWindows = @($windows | Where-Object { $_.FileName -eq $fileName } |
        Sort-Object WindowOrder | ForEach-Object {
            [ordered]@{
                windowOrder = $_.WindowOrder
                openCommand = $_.OpenCommand
                openLine = $_.OpenLine
                closeLine = $_.CloseLine
                start = (Format-LocalTime $_.Start)
                end = (Format-LocalTime $_.End)
                openPassed = [bool]$_.OpenPassed
                preciseTime = [bool]$_.PreciseTime
                sessionId = if ([string]::IsNullOrEmpty($_.SessionId)) { $null } else { $_.SessionId }
                matchQuality = $_.MatchQuality
            }
        })
}

$sessionRows = @($matchResult.SessionRows | ForEach-Object {
    [ordered]@{
        sessionId = $_.SessionId
        sessionOrder = $_.SessionOrder
        endTime = ($_.EndTime -replace ' ', 'T')
        matchQuality = $_.MatchQuality
        scenarioFile = if ([string]::IsNullOrEmpty($_.ScenarioFile)) { $null } else {
            $matchedFileName = $_.ScenarioFile
            $found = @($scenarioResults | Where-Object { [System.IO.Path]::GetFileName($_.sourceFile) -eq $matchedFileName })
            if ($found.Count -eq 1) { $found[0].sourceFile } else { $matchedFileName }
        }
        windowOrder = $_.WindowOrder
    }
})

$document = [ordered]@{
    schemaVersion = 1
    runId = $RunId
    diagnosticLogs = @($diagnosticFiles | ForEach-Object { [ordered]@{ path = (Get-RelativeRunPath $_.FullName) } })
    scenarios = @($scenarioResults.ToArray())
    performanceSessions = $sessionRows
    preparationIssues = @($issues.ToArray())
}

$json = ConvertTo-Json -InputObject $document -Depth 15
[System.IO.File]::WriteAllText($OutputPath, $json + [Environment]::NewLine, [System.Text.UTF8Encoding]::new($false))

Write-Host "Diagnostic preparation complete: $OutputPath"
Write-Host "  Scenarios: $($scenarioResults.Count); Windows: $($windows.Count); Performance sessions: $($sessionRows.Count)"
Write-Host "  Diagnostic log files: $($diagnosticFiles.Count); Preparation issues: $($issues.Count)"

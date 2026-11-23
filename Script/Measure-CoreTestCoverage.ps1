[CmdletBinding()]
param(
    [string]$Configuration = 'Debug',
    [string]$Platform = 'x64',
    [string]$TestExePath,
    [string]$TestArgument = '.\CoreTest',
    [string]$CodeCoverageExe,
    [string]$OutputDirectory,
    [string]$RunSettingsPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:ExitSuccess = 0
$script:ExitTestFailure = 1
$script:ExitMeasurementFailure = 2

function Resolve-FirstExistingPath {
    param([string[]]$Candidates)
    foreach ($candidate in $Candidates) {
        if ([string]::IsNullOrWhiteSpace($candidate)) { continue }
        if (Test-Path -LiteralPath $candidate) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }
    return $null
}

function Get-RepoRoot {
    # Script is expected under <repo>\Script\
    return (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
}

function Find-CodeCoverageTool {
    param([string]$ExplicitPath)
    if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) {
        if (Test-Path -LiteralPath $ExplicitPath) { return (Resolve-Path -LiteralPath $ExplicitPath).Path }
        throw "Specified CodeCoverageExe was not found: $ExplicitPath"
    }

    $candidates = @(
        'C:\Program Files\Microsoft Visual Studio\18\Insiders\Common7\IDE\Extensions\Microsoft\CodeCoverage.Console\Microsoft.CodeCoverage.Console.exe',
        'C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\CodeCoverage.Console\Microsoft.CodeCoverage.Console.exe',
        'C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\Extensions\Microsoft\CodeCoverage.Console\Microsoft.CodeCoverage.Console.exe',
        'C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\Extensions\Microsoft\CodeCoverage.Console\Microsoft.CodeCoverage.Console.exe'
    )
    $resolved = Resolve-FirstExistingPath -Candidates $candidates
    if ($resolved) { return $resolved }

    $cmd = Get-Command Microsoft.CodeCoverage.Console.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    $glob = @(
        'C:\Program Files\Microsoft Visual Studio\*\*\Common7\IDE\Extensions\Microsoft\CodeCoverage.Console\Microsoft.CodeCoverage.Console.exe',
        'C:\Program Files (x86)\Microsoft Visual Studio\*\*\Common7\IDE\Extensions\Microsoft\CodeCoverage.Console\Microsoft.CodeCoverage.Console.exe'
    )
    $found = Get-ChildItem -Path $glob -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName
    if ($found) { return $found }

    throw "Microsoft.CodeCoverage.Console.exe was not found. Install Visual Studio with Code Coverage tools (or provide -CodeCoverageExe)."
}

function Write-Section {
    param([string]$Title)
    Write-Host ""
    Write-Host "=== $Title ==="
}

function Get-CoberturaCoreSummary {
    param([string]$CoberturaPath)

    [xml]$xml = Get-Content -LiteralPath $CoberturaPath -Raw
    $fileStats = @{}

    foreach ($classNode in $xml.SelectNodes('//class')) {
        $filename = [string]$classNode.GetAttribute('filename')
        if ([string]::IsNullOrWhiteSpace($filename)) { continue }
        $normalized = $filename -replace '/', '\'
        $isCore = ($normalized -match '(?i)(^|\\)Core\\') -and ($normalized -notmatch '(?i)(^|\\)CoreTest\\')
        if (-not $isCore) { continue }

        if (-not $fileStats.ContainsKey($normalized)) {
            $fileStats[$normalized] = [ordered]@{
                File = $normalized
                LinesValid = 0
                LinesCovered = 0
            }
        }
        foreach ($line in $classNode.SelectNodes('.//line')) {
            $number = [string]$line.GetAttribute('number')
            $hits = [int]$line.GetAttribute('hits')
            $key = "$normalized|$number"
            # approximate unique line accounting by number within file/class aggregation below
        }
    }

    # Aggregate by filename using class line elements; keep max hits per (file,line)
    $lineHits = @{}
    foreach ($classNode in $xml.SelectNodes('//class')) {
        $filename = [string]$classNode.GetAttribute('filename')
        if ([string]::IsNullOrWhiteSpace($filename)) { continue }
        $normalized = $filename -replace '/', '\'
        $isCore = ($normalized -match '(?i)(^|\\)Core\\') -and ($normalized -notmatch '(?i)(^|\\)CoreTest\\')
        if (-not $isCore) { continue }
        foreach ($line in $classNode.SelectNodes('.//line')) {
            $num = [string]$line.GetAttribute('number')
            if ([string]::IsNullOrWhiteSpace($num)) { continue }
            $hits = 0
            [void][int]::TryParse([string]$line.GetAttribute('hits'), [ref]$hits)
            $key = "$normalized`n$num"
            if (-not $lineHits.ContainsKey($key) -or $hits -gt $lineHits[$key].Hits) {
                $lineHits[$key] = [pscustomobject]@{ File = $normalized; Number = $num; Hits = $hits }
            }
        }
    }

    $byFile = @{}
    foreach ($entry in $lineHits.Values) {
        if (-not $byFile.ContainsKey($entry.File)) {
            $byFile[$entry.File] = [ordered]@{ File = $entry.File; LinesValid = 0; LinesCovered = 0 }
        }
        $byFile[$entry.File].LinesValid++
        if ($entry.Hits -gt 0) { $byFile[$entry.File].LinesCovered++ }
    }

    $files = $byFile.Values | ForEach-Object {
        $rate = if ($_.LinesValid -gt 0) { [math]::Round(($_.LinesCovered / $_.LinesValid) * 100, 2) } else { 0 }
        [pscustomobject]@{
            File = $_.File
            LinesValid = $_.LinesValid
            LinesCovered = $_.LinesCovered
            LineRatePercent = $rate
        }
    } | Sort-Object File

    $totalValid = ($files | Measure-Object -Property LinesValid -Sum).Sum
    $totalCovered = ($files | Measure-Object -Property LinesCovered -Sum).Sum
    if ($null -eq $totalValid) { $totalValid = 0 }
    if ($null -eq $totalCovered) { $totalCovered = 0 }
    $totalRate = if ($totalValid -gt 0) { [math]::Round(($totalCovered / $totalValid) * 100, 2) } else { 0 }

    # Exclusion sanity check over all filename refs
    $allFiles = @{}
    foreach ($classNode in $xml.SelectNodes('//class')) {
        $filename = [string]$classNode.GetAttribute('filename')
        if (-not [string]::IsNullOrWhiteSpace($filename)) {
            $allFiles[($filename -replace '/', '\')] = $true
        }
    }
    $coreTestRefs = @($allFiles.Keys | Where-Object { $_ -match '(?i)(^|\\)CoreTest\\' })
    $thirdPartyRefs = @($allFiles.Keys | Where-Object { $_ -match '(?i)(^|\\)(ThirdParty|packages)\\' -or $_ -match '(?i)nlohmann|googletest|gtest' })

    return [pscustomobject]@{
        Files = $files
        TotalLinesValid = [int]$totalValid
        TotalLinesCovered = [int]$totalCovered
        TotalLineRatePercent = $totalRate
        CoreTestFileRefsInReport = $coreTestRefs
        ThirdPartyFileRefsInReport = $thirdPartyRefs
    }
}

try {
    $repoRoot = Get-RepoRoot
    Push-Location $repoRoot

    if ([string]::IsNullOrWhiteSpace($TestExePath)) {
        $candidates = @(
            (Join-Path $repoRoot "x64\$Configuration\CoreTest.exe"),
            (Join-Path $repoRoot "x64\$Platform\$Configuration\CoreTest.exe"),
            (Join-Path $repoRoot "$Platform\$Configuration\CoreTest.exe"),
            (Join-Path $repoRoot "$Configuration\CoreTest.exe")
        )
        $TestExePath = Resolve-FirstExistingPath -Candidates $candidates
        if (-not $TestExePath) {
            $TestExePath = $candidates[0]
        }
    }
    if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
        $OutputDirectory = Join-Path $repoRoot 'TestResults\CoreTestCoveragePoC'
    }
    if ([string]::IsNullOrWhiteSpace($RunSettingsPath)) {
        $RunSettingsPath = Join-Path $PSScriptRoot 'CoreTestCoverage.runsettings'
    }

    $toolPath = Find-CodeCoverageTool -ExplicitPath $CodeCoverageExe

    if (-not (Test-Path -LiteralPath $TestExePath)) {
        throw "CoreTest executable was not found: $TestExePath"
    }
    if (-not (Test-Path -LiteralPath $RunSettingsPath)) {
        throw "Runsettings file was not found: $RunSettingsPath"
    }

    New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
    $coverageFile = Join-Path $OutputDirectory 'CoreTest.coverage'
    $coberturaFile = Join-Path $OutputDirectory 'CoreTest.cobertura.xml'
    $xmlFile = Join-Path $OutputDirectory 'CoreTest.coverage.xml'
    $collectLog = Join-Path $OutputDirectory 'collect.log'
    $mergeLog = Join-Path $OutputDirectory 'merge.log'
    $summaryFile = Join-Path $OutputDirectory 'CoreCoverageSummary.txt'

    foreach ($old in @($coverageFile, $coberturaFile, $xmlFile, $collectLog, $mergeLog, $summaryFile)) {
        if (Test-Path -LiteralPath $old) { Remove-Item -LiteralPath $old -Force }
    }

    $toolVersion = $null
    try { $toolVersion = & $toolPath --version 2>&1 | Select-Object -First 1 } catch { $toolVersion = $null }
    $toolFileVersion = (Get-Item $toolPath).VersionInfo.FileVersion

    Write-Section "Configuration"
    Write-Host "RepoRoot: $repoRoot"
    Write-Host "Tool: $toolPath"
    Write-Host "ToolFileVersion: $toolFileVersion"
    if ($toolVersion) { Write-Host "ToolVersion: $toolVersion" }
    Write-Host "TestExe: $TestExePath"
    Write-Host "TestArg: $TestArgument"
    Write-Host "RunSettings: $RunSettingsPath"
    Write-Host "OutputDirectory: $OutputDirectory"

    Write-Section "Collect"
    & $toolPath collect $TestExePath $TestArgument -s $RunSettingsPath -o $coverageFile -f coverage -l $collectLog -ll Info --nologo
    $collectExit = $LASTEXITCODE
    Write-Host "CollectExitCode: $collectExit"

    $coverageExists = Test-Path -LiteralPath $coverageFile
    Write-Host "CoverageFileExists: $coverageExists"
    if (-not $coverageExists) {
        Write-Host "Measurement failed: coverage file was not produced."
        Write-Host "See log: $collectLog"
        exit $script:ExitMeasurementFailure
    }

    Write-Section "Report"
    & $toolPath merge $coverageFile -o $coberturaFile -f cobertura -l $mergeLog -ll Info --nologo
    $mergeCoberturaExit = $LASTEXITCODE
    & $toolPath merge $coverageFile -o $xmlFile -f xml -l (Join-Path $OutputDirectory 'merge-xml.log') -ll Info --nologo
    $mergeXmlExit = $LASTEXITCODE
    Write-Host "MergeCoberturaExitCode: $mergeCoberturaExit"
    Write-Host "MergeXmlExitCode: $mergeXmlExit"

    if (($mergeCoberturaExit -ne 0 -or -not (Test-Path -LiteralPath $coberturaFile)) -and ($mergeXmlExit -ne 0 -or -not (Test-Path -LiteralPath $xmlFile))) {
        Write-Host "Measurement failed: could not generate XML/Cobertura reports."
        Write-Host "See log: $mergeLog"
        exit $script:ExitMeasurementFailure
    }

    $summary = $null
    $summaryText = @()
    if (Test-Path -LiteralPath $coberturaFile) {
        $summary = Get-CoberturaCoreSummary -CoberturaPath $coberturaFile
        $summaryText += "Core source files in filtered report: $($summary.Files.Count)"
        $summaryText += "Core lines valid: $($summary.TotalLinesValid)"
        $summaryText += "Core lines covered: $($summary.TotalLinesCovered)"
        $summaryText += "Core line coverage: $($summary.TotalLineRatePercent)%"
        $summaryText += "CoreTest filename refs in report: $($summary.CoreTestFileRefsInReport.Count)"
        $summaryText += "ThirdParty/gtest/nlohmann filename refs in report: $($summary.ThirdPartyFileRefsInReport.Count)"
        $summaryText += ""
        $summaryText += "Files:"
        foreach ($f in $summary.Files) {
            $summaryText += ("  - {0} ({1}/{2}, {3}%)" -f $f.File, $f.LinesCovered, $f.LinesValid, $f.LineRatePercent)
        }
        $summaryText += ""
        if ($summary.CoreTestFileRefsInReport.Count -gt 0) {
            $summaryText += "CoreTest refs (unexpected):"
            $summaryText += ($summary.CoreTestFileRefsInReport | ForEach-Object { "  - $_" })
        }
        if ($summary.ThirdPartyFileRefsInReport.Count -gt 0) {
            $summaryText += "ThirdParty refs (unexpected):"
            $summaryText += ($summary.ThirdPartyFileRefsInReport | ForEach-Object { "  - $_" })
        }
        $summaryText | Set-Content -LiteralPath $summaryFile -Encoding UTF8

        Write-Section "Core Coverage Summary"
        $summaryText | ForEach-Object { Write-Host $_ }
    } else {
        Write-Host "Cobertura report not generated; XML report may be available at: $xmlFile"
    }

    $testLogPath = Join-Path $repoRoot 'TestLog\CoreTestResult.txt'
    if (Test-Path -LiteralPath $testLogPath) {
        Write-Section "CoreTest Log"
        Get-Content -LiteralPath $testLogPath -Tail 20 | ForEach-Object { Write-Host $_ }
    }

    Write-Section "Artifacts"
    Write-Host "Coverage: $coverageFile"
    if (Test-Path -LiteralPath $coberturaFile) { Write-Host "Cobertura: $coberturaFile" }
    if (Test-Path -LiteralPath $xmlFile) { Write-Host "Xml: $xmlFile" }
    Write-Host "CollectLog: $collectLog"
    Write-Host "Summary: $summaryFile"

    if ($collectExit -ne 0) {
        Write-Host "Result: CoreTest failed or child process returned non-zero (collect exit $collectExit). Coverage artifacts were generated where possible."
        exit $script:ExitTestFailure
    }

    Write-Host "Result: CoreTest and coverage collection completed successfully."
    exit $script:ExitSuccess
}
catch {
    Write-Host "Measurement failed with exception: $($_.Exception.Message)"
    Write-Host $_.ScriptStackTrace
    exit $script:ExitMeasurementFailure
}
finally {
    Pop-Location -ErrorAction SilentlyContinue
}

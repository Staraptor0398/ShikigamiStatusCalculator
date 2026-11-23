[CmdletBinding()]
param(
    [ValidateSet('Single', 'Batch')]
    [string]$Mode = 'Single',
    [string]$ScenarioPath = 'Scenario\01_Gui\01_InitialGuiState.scenario',
    [string]$ScenarioDirectory = 'Scenario',
    [string]$GuiExecutablePath = 'Gui\bin\x64\Release\Gui.exe',
    [string]$CliExecutablePath = 'ScenarioRunnerCli\bin\x64\Release\ScenarioRunnerCli.exe',
    [string]$CodeCoverageExe,
    [string]$RunSettingsPath,
    [string]$OutputDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-ExistingFile {
    param([string]$Path, [string]$Description)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "$Description not found: $Path"
    }
    return (Resolve-Path -LiteralPath $Path).Path
}

function Find-CodeCoverageTool {
    param([string]$ExplicitPath)

    if ($ExplicitPath) {
        return Resolve-ExistingFile -Path $ExplicitPath -Description 'Code coverage tool'
    }

    $command = Get-Command 'Microsoft.CodeCoverage.Console.exe' -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }

    $patterns = @(
        'C:\Program Files\Microsoft Visual Studio\*\*\Common7\IDE\Extensions\Microsoft\CodeCoverage.Console\Microsoft.CodeCoverage.Console.exe',
        'C:\Program Files (x86)\Microsoft Visual Studio\*\*\Common7\IDE\Extensions\Microsoft\CodeCoverage.Console\Microsoft.CodeCoverage.Console.exe'
    )
    $found = Get-ChildItem -Path $patterns -ErrorAction SilentlyContinue |
        Select-Object -First 1 -ExpandProperty FullName
    if ($found) { return $found }

    throw 'Microsoft.CodeCoverage.Console.exe was not found. Supply -CodeCoverageExe.'
}

function Get-ModuleCoverageSummary {
    param([string]$CoberturaPath, [string]$SummaryPath)

    [xml]$document = Get-Content -LiteralPath $CoberturaPath -Raw
    $modules = @{
        'ScenarioRunner.Engine' = @{}
        'Gui' = @{}
    }
    $unexpected = @()

    foreach ($classNode in $document.SelectNodes('//class')) {
        $name = ([string]$classNode.GetAttribute('filename')).Replace('/', '\')
        if ([string]::IsNullOrWhiteSpace($name)) { continue }

        $moduleName = $null
        if ($name -match '(?i)(^|\\)ScenarioRunner\.Engine\\') {
            $moduleName = 'ScenarioRunner.Engine'
        }
        elseif ($name -match '(?i)(^|\\)Gui\\') {
            $moduleName = 'Gui'
        }
        else {
            $unexpected += $name
            continue
        }

        if (-not $modules[$moduleName].ContainsKey($name)) {
            $modules[$moduleName][$name] = @{}
        }

        # Cobertura can repeat the same source line under several classes/methods.
        # Count one (file, line) pair once and preserve the maximum hit count.
        foreach ($lineNode in $classNode.SelectNodes('./lines/line')) {
            $number = [string]$lineNode.GetAttribute('number')
            if (-not $number) { continue }

            $hits = 0
            [void][int]::TryParse([string]$lineNode.GetAttribute('hits'), [ref]$hits)
            $fileLines = $modules[$moduleName][$name]
            if (-not $fileLines.ContainsKey($number) -or $hits -gt $fileLines[$number]) {
                $fileLines[$number] = $hits
            }
        }
    }

    if ($unexpected.Count -gt 0) {
        throw "Unexpected source files in report: $(@($unexpected | Sort-Object -Unique) -join '; ')"
    }

    $summary = @('=== Scenario E2E Coverage Summary ===', '')
    foreach ($moduleName in @('ScenarioRunner.Engine', 'Gui')) {
        $valid = 0
        $covered = 0
        $lines = @()
        foreach ($path in @($modules[$moduleName].Keys | Sort-Object)) {
            $values = @($modules[$moduleName][$path].Values)
            $fileValid = $values.Count
            $fileCovered = @($values | Where-Object { $_ -gt 0 }).Count
            $valid += $fileValid
            $covered += $fileCovered
            $percent = if ($fileValid -gt 0) {
                [math]::Round(100.0 * $fileCovered / $fileValid, 2)
            } else { 0 }
            $lines += "  - $path ($fileCovered/$fileValid, $percent%)"
        }

        $rate = if ($valid -gt 0) { [math]::Round(100.0 * $covered / $valid, 2) } else { 0 }
        $summary += "Module: $moduleName"
        $summary += "Source files: $($modules[$moduleName].Count)"
        $summary += "Lines valid: $valid"
        $summary += "Lines covered: $covered"
        $summary += "Line coverage: $rate%"
        $summary += $lines
        $summary += ''
    }

    $summary | Set-Content -LiteralPath $SummaryPath -Encoding UTF8
    $summary | ForEach-Object { Write-Host $_ }

    # Missing either module is a PoC finding, not a false successful measurement.
    if ($modules['ScenarioRunner.Engine'].Count -eq 0 -or $modules['Gui'].Count -eq 0) {
        throw 'One or both target modules were not captured. Inspect collect.log for child process instrumentation.'
    }
}

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $root 'TestResults\ScenarioE2ECoveragePoC' }
if (-not $RunSettingsPath) { $RunSettingsPath = Join-Path $PSScriptRoot 'ScenarioE2ECoverage.runsettings' }

$locationPushed = $false
try {
    Push-Location $root
    $locationPushed = $true

    $tool = Find-CodeCoverageTool -ExplicitPath $CodeCoverageExe
    $cli = Resolve-ExistingFile -Path $CliExecutablePath -Description 'ScenarioRunner CLI'
    $gui = Resolve-ExistingFile -Path $GuiExecutablePath -Description 'Gui executable'
    $settings = Resolve-ExistingFile -Path $RunSettingsPath -Description 'Runsettings'

    if ($Mode -eq 'Single') {
        $scenario = Resolve-ExistingFile -Path $ScenarioPath -Description 'Scenario'
        $cliArguments = @('--run', $scenario, '--gui-path', $gui)
    }
    else {
        if (-not (Test-Path -LiteralPath $ScenarioDirectory -PathType Container)) {
            throw "Scenario directory not found: $ScenarioDirectory"
        }
        $scenarioDir = (Resolve-Path -LiteralPath $ScenarioDirectory).Path
        $scenarioCount = @(Get-ChildItem -LiteralPath $scenarioDir -File -Recurse -Filter '*.scenario').Count
        if ($scenarioCount -eq 0) { throw "No .scenario files in: $scenarioDir" }
        Write-Host "ScenarioCount: $scenarioCount"
        $cliArguments = @('--run-directory', $scenarioDir, '--gui-path', $gui)
    }

    New-Item -Path $OutputDirectory -ItemType Directory -Force | Out-Null
    $out = (Resolve-Path -LiteralPath $OutputDirectory).Path
    $coverage = Join-Path $out 'ScenarioE2E.coverage'
    $cobertura = Join-Path $out 'ScenarioE2E.cobertura.xml'
    $xml = Join-Path $out 'ScenarioE2E.coverage.xml'
    $collectLog = Join-Path $out 'collect.log'
    $mergeLog = Join-Path $out 'merge.log'
    $mergeXmlLog = Join-Path $out 'merge-xml.log'
    $summary = Join-Path $out 'ScenarioE2ECoverageSummary.txt'
    $cliLog = Join-Path $out 'scenario-execution.log'

    foreach ($path in @($coverage, $cobertura, $xml, $collectLog, $mergeLog, $mergeXmlLog, $summary, $cliLog)) {
        if (Test-Path -LiteralPath $path) { Remove-Item -LiteralPath $path -Force }
    }

    Write-Host "CoverageTool: $tool"
    Write-Host "Cli: $cli"
    Write-Host "Gui: $gui"
    Write-Host "Mode: $Mode"
    Write-Host "OutputDirectory: $out"

    # The coverage collector starts the CLI and monitors its child processes.
    # The existing CLI launches Gui.exe, which must also appear in the report.
    & $tool collect $cli @cliArguments -s $settings -o $coverage -f coverage `
        -l $collectLog -ll Info --nologo 2>&1 | Tee-Object -FilePath $cliLog
    $collectExit = $LASTEXITCODE
    Write-Host "CollectExitCode: $collectExit"

    if (-not (Test-Path -LiteralPath $coverage -PathType Leaf)) {
        throw "Coverage data file was not created. CollectExitCode=$collectExit. Inspect $collectLog"
    }

    & $tool merge $coverage -o $cobertura -f cobertura -l $mergeLog -ll Info --nologo
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $cobertura -PathType Leaf)) {
        throw 'Cobertura conversion failed.'
    }

    & $tool merge $coverage -o $xml -f xml -l $mergeXmlLog -ll Info --nologo
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $xml -PathType Leaf)) {
        throw 'XML conversion failed.'
    }

    Get-ModuleCoverageSummary -CoberturaPath $cobertura -SummaryPath $summary

    if ($collectExit -ne 0) {
        throw "Scenario CLI or coverage collection failed with exit code $collectExit. Check scenario-execution.log."
    }
    Write-Host 'Result: E2E coverage collection succeeded for both Engine and Gui.'
    exit 0
}
catch {
    Write-Host "E2E coverage PoC failed: $($_.Exception.Message)"
    Write-Host $_.ScriptStackTrace
    exit 1
}
finally {
    if ($locationPushed) { Pop-Location }
}

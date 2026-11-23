[CmdletBinding()]
param(
    [string]$TestAssemblyPath,
    [string]$VsTestExe,
    [string]$CodeCoverageExe,
    [string]$RunSettingsPath,
    [string]$OutputDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Find-CodeCoverageTool {
    param([string]$ExplicitPath)
    if ($ExplicitPath) {
        if (-not (Test-Path -LiteralPath $ExplicitPath -PathType Leaf)) {
            throw "Code coverage tool not found: $ExplicitPath"
        }
        return (Resolve-Path -LiteralPath $ExplicitPath).Path
    }

    $command = Get-Command 'Microsoft.CodeCoverage.Console.exe' -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }

    $paths = @(
        'C:\Program Files\Microsoft Visual Studio\*\*\Common7\IDE\Extensions\Microsoft\CodeCoverage.Console\Microsoft.CodeCoverage.Console.exe',
        'C:\Program Files (x86)\Microsoft Visual Studio\*\*\Common7\IDE\Extensions\Microsoft\CodeCoverage.Console\Microsoft.CodeCoverage.Console.exe'
    )
    $found = Get-ChildItem -Path $paths -ErrorAction SilentlyContinue |
        Select-Object -First 1 -ExpandProperty FullName
    if ($found) { return $found }

    throw 'Microsoft.CodeCoverage.Console.exe was not found. Supply -CodeCoverageExe.'
}

function Find-VsTest {
    param([string]$ExplicitPath)

    if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) {
        if (-not (Test-Path -LiteralPath $ExplicitPath -PathType Leaf)) {
            throw "VSTest executable not found: $ExplicitPath"
        }

        return (Resolve-Path -LiteralPath $ExplicitPath).Path
    }

    # 1. PATH上のvstest.console.exe
    $command = Get-Command 'vstest.console.exe' -ErrorAction SilentlyContinue

    if ($command) {
        return $command.Source
    }

    # 2. vswhere.exeの探索
    $vswhereCommand = Get-Command 'vswhere.exe' -ErrorAction SilentlyContinue
    $vswherePath = $null

    if ($vswhereCommand) {
        $vswherePath = $vswhereCommand.Source
    }
    else {
        $candidate = Join-Path ${env:ProgramFiles(x86)} `
            'Microsoft Visual Studio\Installer\vswhere.exe'

        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            $vswherePath = (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    # 3. vswhereで全VSインスタンスを検索
    if ($vswherePath) {
        $found = @(
            & $vswherePath `
                -all `
                -products '*' `
                -find '**\vstest.console.exe' |
                Where-Object {
                    -not [string]::IsNullOrWhiteSpace($_) -and
                    (Test-Path -LiteralPath $_ -PathType Leaf)
                }
        )

        if ($found.Count -gt 0) {
            return $found[0]
        }
    }

    # 4. Visual Studioの標準インストール先から探索
    $patterns = @(
        'C:\Program Files\Microsoft Visual Studio\*\*\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe',
        'C:\Program Files\Microsoft Visual Studio\*\*\Common7\IDE\Extensions\TestPlatform\vstest.console.exe',
        'C:\Program Files (x86)\Microsoft Visual Studio\*\*\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe',
        'C:\Program Files (x86)\Microsoft Visual Studio\*\*\Common7\IDE\Extensions\TestPlatform\vstest.console.exe'
    )

    $found = Get-ChildItem -Path $patterns -ErrorAction SilentlyContinue |
        Select-Object -First 1 -ExpandProperty FullName

    if ($found) {
        return $found
    }

    throw 'vstest.console.exe was not found. Supply -VsTestExe.'
}

function Resolve-TestAssembly {
    param([string]$ExplicitPath, [string]$Root)
    if ($ExplicitPath) {
        if (-not (Test-Path -LiteralPath $ExplicitPath -PathType Leaf)) {
            throw "Test assembly not found: $ExplicitPath"
        }
        return (Resolve-Path -LiteralPath $ExplicitPath).Path
    }

    $found = @(Get-ChildItem -LiteralPath (Join-Path $Root 'SaveDataTest\bin') `
        -Filter 'SaveDataTest.dll' -File -Recurse -ErrorAction SilentlyContinue)
    if ($found.Count -ne 1) {
        throw "Expected exactly one SaveDataTest.dll in SaveDataTest\bin; found $($found.Count). Use -TestAssemblyPath."
    }
    return $found[0].FullName
}

function Get-SaveDataCoverageSummary {
    param([string]$Path, [string]$Output)
    [xml]$doc = Get-Content -LiteralPath $Path -Raw
    $perFile = @{}

    foreach ($classNode in $doc.SelectNodes('//class')) {
        $filename = [string]$classNode.GetAttribute('filename')
        $normalized = $filename.Replace('/', '\')
        if ($normalized -notmatch '(?i)(^|\\)SaveData\\') { continue }
        if ($normalized -match '(?i)(^|\\)SaveDataTest\\') { continue }

        if (-not $perFile.ContainsKey($normalized)) { $perFile[$normalized] = @{} }
        foreach ($line in $classNode.SelectNodes('.//line')) {
            $number = [string]$line.GetAttribute('number')
            if (-not $number) { continue }
            $hits = 0
            [void][int]::TryParse([string]$line.GetAttribute('hits'), [ref]$hits)
            if (-not $perFile[$normalized].ContainsKey($number) -or
                $hits -gt $perFile[$normalized][$number]) {
                $perFile[$normalized][$number] = $hits
            }
        }
    }

    $files = @()
    $covered = 0
    $valid = 0
    foreach ($filename in @($perFile.Keys | Sort-Object)) {
        $hits = @($perFile[$filename].Values)
        $fileValid = $hits.Count
        $fileCovered = @($hits | Where-Object { $_ -gt 0 }).Count
        $valid += $fileValid
        $covered += $fileCovered
        $pct = if ($fileValid -gt 0) { [math]::Round(100 * $fileCovered / $fileValid, 2) } else { 0 }
        $files += "  - $filename ($fileCovered/$fileValid, $pct%)"
    }

    if ($valid -eq 0) {
        throw 'Cobertura contains no measurable SaveData source lines. Collector may not have captured the test host.'
    }

    $allNames = @($doc.SelectNodes('//class') | ForEach-Object {
        ([string]$_.GetAttribute('filename')).Replace('/', '\')
    } | Where-Object { $_ })
    $unexpected = @($allNames | Where-Object {
        $_ -notmatch '(?i)(^|\\)SaveData\\' -or
        $_ -match '(?i)(^|\\)SaveDataTest\\'
    } | Sort-Object -Unique)
    if ($unexpected.Count -gt 0) {
        throw "Unexpected source paths in Cobertura: $($unexpected -join '; ')"
    }

    $rate = [math]::Round(100 * $covered / $valid, 2)
    $summary = @(
        "SaveData source files in filtered report: $($perFile.Count)"
        "SaveData lines valid: $valid"
        "SaveData lines covered: $covered"
        "SaveData line coverage: $rate%"
        'SaveDataTest/third-party filename refs in report: 0'
        ''
        'Files:'
    ) + $files
    $summary | Set-Content -LiteralPath $Output -Encoding UTF8
    $summary | ForEach-Object { Write-Host $_ }
}

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $root 'TestResults\SaveDataTestCoveragePoC' }
if (-not $RunSettingsPath) { $RunSettingsPath = Join-Path $PSScriptRoot 'SaveDataTestCoverage.runsettings' }

$locationPushed = $false
try {
    Push-Location $root
    $locationPushed = $true

    $tool = Find-CodeCoverageTool -ExplicitPath $CodeCoverageExe
    $vstest = Find-VsTest -ExplicitPath $VsTestExe
    $assembly = Resolve-TestAssembly -ExplicitPath $TestAssemblyPath -Root $root
    if (-not (Test-Path -LiteralPath $RunSettingsPath -PathType Leaf)) {
        throw "Runsettings not found: $RunSettingsPath"
    }
    $settings = (Resolve-Path -LiteralPath $RunSettingsPath).Path

    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
    $out = (Resolve-Path -LiteralPath $OutputDirectory).Path
    $coverage = Join-Path $out 'SaveDataTest.coverage'
    $cobertura = Join-Path $out 'SaveDataTest.cobertura.xml'
    $xml = Join-Path $out 'SaveDataTest.coverage.xml'
    $trx = Join-Path $out 'SaveDataTest.trx'
    $collectLog = Join-Path $out 'collect.log'
    $mergeLog = Join-Path $out 'merge.log'
    $mergeXmlLog = Join-Path $out 'merge-xml.log'
    $summary = Join-Path $out 'SaveDataCoverageSummary.txt'

    foreach ($file in @($coverage,$cobertura,$xml,$trx,$collectLog,$mergeLog,$mergeXmlLog,$summary)) {
        if (Test-Path -LiteralPath $file) { Remove-Item -LiteralPath $file -Force }
    }

    Write-Host "CoverageTool: $tool"
    Write-Host "VSTest: $vstest"
    Write-Host "TestAssembly: $assembly"
    Write-Host "OutputDirectory: $out"

    # The coverage tool launches VSTest; VSTest can launch a separate testhost.
    # The PoC must verify that SaveData.dll actually appears in the report.
    & $tool collect $vstest $assembly '/Platform:x64' `
        "/Logger:trx;LogFileName=SaveDataTest.trx" `
        "/ResultsDirectory:$out" `
        -s $settings -o $coverage -f coverage -l $collectLog -ll Info --nologo
    $collectExit = $LASTEXITCODE
    Write-Host "CollectExitCode: $collectExit"

    if (-not (Test-Path -LiteralPath $trx)) {
        throw 'TRX was not created. Inspect collect.log and testhost discovery.'
    }

    [xml]$trxDoc = Get-Content -LiteralPath $trx -Raw
    $counters = $trxDoc.TestRun.ResultSummary.Counters
    $total = [int]$counters.total
    $passed = [int]$counters.passed
    $failed = [int]$counters.failed
    $notExecuted = [int]$counters.notExecuted
    Write-Host "Tests: total=$total, passed=$passed, failed=$failed, notExecuted=$notExecuted"

    if ($total -eq 0 -or $failed -ne 0 -or $notExecuted -ne 0 -or $passed -ne $total) {
        Write-Host 'Test run failed. Coverage files will be retained if present.'
        exit 1
    }
    if ($collectExit -ne 0) {
        Write-Host 'Collector process returned non-zero despite passing TRX.'
        exit 2
    }
    if (-not (Test-Path -LiteralPath $coverage)) {
        throw 'Coverage binary was not created.'
    }

    & $tool merge $coverage -o $cobertura -f cobertura -l $mergeLog -ll Info --nologo
    $mergeExit = $LASTEXITCODE
    if ($mergeExit -ne 0 -or -not (Test-Path -LiteralPath $cobertura)) {
        throw "Cobertura conversion failed: $mergeExit"
    }

    & $tool merge $coverage -o $xml -f xml -l $mergeXmlLog -ll Info --nologo
    $mergeXmlExit = $LASTEXITCODE
    if ($mergeXmlExit -ne 0 -or -not (Test-Path -LiteralPath $xml)) {
        throw "Native XML conversion failed: $mergeXmlExit"
    }

    Write-Host '=== SaveData Coverage Summary ==='
    Get-SaveDataCoverageSummary -Path $cobertura -Output $summary
    Write-Host 'Result: SaveDataTest and SaveData.dll coverage collection succeeded.'
    exit 0
}
catch {
    Write-Host "Coverage measurement failed: $($_.Exception.Message)"
    Write-Host $_.ScriptStackTrace
    exit 2
}
finally {
    if ($locationPushed) { Pop-Location }
}

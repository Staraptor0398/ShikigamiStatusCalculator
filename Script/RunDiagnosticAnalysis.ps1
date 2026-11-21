param(
    [Parameter(Mandatory)]
    [string]$RunId,

    [Parameter(Mandatory)]
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'

# Scenario本体とは独立して診断解析を実施する。失敗してもWorkflowの成否は変更しない。
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).ProviderPath
$outputRoot = [System.IO.Path]::GetFullPath($OutputDirectory)
$runRoot = Join-Path $outputRoot 'Run'
$reportPath = Join-Path $outputRoot 'DiagnosticAnalysis.txt'
$errorPath = Join-Path $outputRoot 'DiagnosticAnalysisError.txt'

New-Item -ItemType Directory -Force -Path $runRoot | Out-Null

try {
    if ($RunId -notmatch '^\d+$') { throw "Invalid RunId: $RunId" }

    # 記録終了後にビルドするため、既存の並列ビルドや録画開始時刻へ影響しない。
    $projectPath = Join-Path $repositoryRoot 'DiagnosticLogAnalyzer\DiagnosticLogAnalyzer.csproj'
    if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
        throw 'DiagnosticLogAnalyzer.csproj was not found.'
    }

    $buildOutput = Join-Path $outputRoot 'AnalyzerBuild.stdout.log'
    $buildError = Join-Path $outputRoot 'AnalyzerBuild.stderr.log'
    $build = Start-Process -FilePath 'dotnet' `
        -ArgumentList @('build', 'DiagnosticLogAnalyzer/DiagnosticLogAnalyzer.csproj', '-c', 'Release', '-p:Platform=x64') `
        -WorkingDirectory $repositoryRoot -NoNewWindow -Wait -PassThru `
        -RedirectStandardOutput $buildOutput -RedirectStandardError $buildError

    if ($build.ExitCode -ne 0) {
        throw "DiagnosticLogAnalyzer build failed with exit code $($build.ExitCode)."
    }

    # ディレクトリ構成はPrepareDiagnosticAnalysis.ps1が想定する相対パスに合わせる。
    $logDirectories = @(
        'ScenarioRunnerCli\bin\x64\Release\Log',
        'Gui\bin\x64\Release\Log'
    )
    foreach ($relative in $logDirectories) {
        $source = Join-Path $repositoryRoot $relative
        if (-not (Test-Path -LiteralPath $source -PathType Container)) {
            Write-Warning "Diagnostic log directory missing: $relative"
            continue
        }

        $destination = Join-Path $runRoot $relative
        New-Item -ItemType Directory -Force -Path $destination | Out-Null
        Get-ChildItem -LiteralPath $source -File -Filter '*.log' |
            Copy-Item -Destination $destination -Force
    }

    $preparationScript = Join-Path $repositoryRoot 'Script\PrepareDiagnosticAnalysis.ps1'
    $inputPath = Join-Path $runRoot 'DiagnosticAnalysisInput.json'
    if (-not (Test-Path -LiteralPath $preparationScript -PathType Leaf)) {
        throw 'PrepareDiagnosticAnalysis.ps1 was not found.'
    }

    & $preparationScript -RunId $RunId -RunDirectory $runRoot -OutputPath $inputPath
    if (-not (Test-Path -LiteralPath $inputPath -PathType Leaf)) {
        throw 'DiagnosticAnalysisInput.json was not generated.'
    }

    $analyzerPath = Join-Path $repositoryRoot 'DiagnosticLogAnalyzer\bin\x64\Release\DiagnosticLogAnalyzer.exe'
    if (-not (Test-Path -LiteralPath $analyzerPath -PathType Leaf)) {
        throw 'DiagnosticLogAnalyzer.exe was not found.'
    }

    $stdout = Join-Path $outputRoot 'Analyzer.stdout.txt'
    $stderr = Join-Path $outputRoot 'Analyzer.stderr.txt'
    $timer = [System.Diagnostics.Stopwatch]::StartNew()
    $analysis = Start-Process -FilePath $analyzerPath `
        -ArgumentList ('"' + $inputPath + '"') `
        -NoNewWindow -Wait -PassThru `
        -RedirectStandardOutput $stdout -RedirectStandardError $stderr
    $timer.Stop()

    $report = @(
        "RunId: $RunId"
        "Analyzer exit code: $($analysis.ExitCode)"
        "Elapsed: $([math]::Round($timer.Elapsed.TotalMilliseconds, 2)) ms"
        ''
    )
    if (Test-Path -LiteralPath $stdout) {
        $report += @(Get-Content -LiteralPath $stdout -Encoding utf8)
    }
    if (Test-Path -LiteralPath $stderr) {
        $report += @(Get-Content -LiteralPath $stderr -Encoding utf8)
    }
    $report | Set-Content -LiteralPath $reportPath -Encoding utf8
    $report | ForEach-Object { Write-Host $_ }

    if ($analysis.ExitCode -ne 0) {
        Write-Warning "DiagnosticLogAnalyzer exited with code $($analysis.ExitCode)."
    }
}
catch {
    $message = $_ | Out-String
    [System.IO.File]::WriteAllText($errorPath, $message, [System.Text.UTF8Encoding]::new($false))
    Write-Warning "Diagnostic analysis failed: $($_.Exception.Message)"
}
finally {
    # 捕捉済みの解析例外によってScenarioRunnerの成功/失敗を上書きしない。
    $global:LASTEXITCODE = 0
}

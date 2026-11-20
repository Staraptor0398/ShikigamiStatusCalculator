$ErrorActionPreference = 'Stop'

$script:passed = 0
$script:failed = 0
$script:testRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('TestPrepareDiagnosticAnalysis_' + [guid]::NewGuid().ToString('N'))
$script:sourceScript = Join-Path (Split-Path -Parent $PSScriptRoot) 'PrepareDiagnosticAnalysis.ps1'
$script:parserNames = @('ScenarioSessionParser.ps1', 'ScenarioCommandParser.ps1', 'PerformanceSessionParser.ps1', 'SessionMatcher.ps1')
$script:sessionId = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
$script:secondSessionId = 'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb'
$script:utf8 = [System.Text.UTF8Encoding]::new($false)

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
    if (-not (@($Items) | Where-Object { [string]$_ -like "*$Fragment*" })) {
        throw "${Description}: expected fragment [$Fragment]"
    }
}

function Write-TestFile {
    param([string]$Path, [string[]]$Lines)
    $parent = [System.IO.Path]::GetDirectoryName($Path)
    [System.IO.Directory]::CreateDirectory($parent) | Out-Null
    [System.IO.File]::WriteAllText($Path, (($Lines -join "`r`n") + "`r`n"), $script:utf8)
}

function New-Fixture {
    param([switch]$WithoutPerformance, [switch]$WithoutDiagnostic, [switch]$WithoutScenario)

    $dir = Join-Path $script:testRoot ([guid]::NewGuid().ToString('N'))
    $runDir = Join-Path $dir 'Run'
    $scriptDir = Join-Path $dir 'Script'
    $analysisDir = Join-Path $scriptDir 'Analysis'
    [System.IO.Directory]::CreateDirectory($runDir) | Out-Null
    [System.IO.Directory]::CreateDirectory($analysisDir) | Out-Null
    Copy-Item -LiteralPath $script:sourceScript -Destination (Join-Path $scriptDir 'PrepareDiagnosticAnalysis.ps1')
    foreach ($name in $script:parserNames) {
        Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination (Join-Path $analysisDir $name)
    }

    $scenarioPath = Join-Path $runDir 'ScenarioRunnerCli/bin/x64/Release/Log/ScenarioRunner_20261121_010000_Test.scenario.log'
    $performancePath = Join-Path $runDir 'Gui/bin/x64/Release/Log/ShikigamiApp_Performance_20261121.log'
    $diagnosticPath = Join-Path $runDir 'Gui/bin/x64/Release/Log/ShikigamiApp_20261121.log'
    if (-not $WithoutScenario) {
        Write-TestFile $scenarioPath @(
            '[01:00:00.000] Scenario: Test.scenario',
            '[01:00:00.100] [1] OPEN GUI    START',
            '[01:00:00.150] [1] OPEN GUI    PASS',
            '[01:00:00.200] [2] CALC    START',
            '[01:00:00.300] [2] CALC    PASS',
            '[01:00:00.320] [3] CLOSE GUI    START',
            '[01:00:00.400] [3] CLOSE GUI    PASS',
            '[01:00:00.410] Scenario Result: PASS'
        )
    }
    if (-not $WithoutPerformance) {
        Write-TestFile $performancePath @(
            "2026-11-21 01:00:00.100 Session=$($script:sessionId) Event=Begin",
            "2026-11-21 01:00:00.300 Session=$($script:sessionId) Target=Calculation Count=1 TotalMs=5.000 AverageMs=5.000 MaxMs=5.000",
            "2026-11-21 01:00:00.420 Session=$($script:sessionId) Event=End"
        )
    }
    if (-not $WithoutDiagnostic) {
        Write-TestFile $diagnosticPath @(
            "2026-11-21 01:00:00.200 [INFO] SessionId=$($script:sessionId) Operation=Calculation Event=Started",
            "2026-11-21 01:00:00.280 [INFO] SessionId=$($script:sessionId) Operation=Calculation Event=Completed"
        )
    }
    return [pscustomobject]@{
        Root = $dir
        RunDirectory = $runDir
        Script = Join-Path $scriptDir 'PrepareDiagnosticAnalysis.ps1'
        ScenarioPath = $scenarioPath
        PerformancePath = $performancePath
        DiagnosticPath = $diagnosticPath
        OutputPath = Join-Path $runDir 'DiagnosticAnalysisInput.json'
    }
}

function Invoke-Preparation {
    param(
        [Parameter(Mandatory)][object]$Fixture,
        [string]$RunId = '12345',
        [string]$RunDirectory,
        [string]$OutputPath
    )
    $outFile = Join-Path $Fixture.Root 'stdout.txt'
    $errFile = Join-Path $Fixture.Root 'stderr.txt'
    $args = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', ('"' + $Fixture.Script + '"'), '-RunId', ('"' + $RunId + '"'))
    if ([string]::IsNullOrEmpty($RunDirectory)) { $RunDirectory = $Fixture.RunDirectory }
    $args += @('-RunDirectory', ('"' + $RunDirectory + '"'))
    if (-not [string]::IsNullOrEmpty($OutputPath)) { $args += @('-OutputPath', ('"' + $OutputPath + '"')) }
    $p = Start-Process -FilePath (Join-Path $PSHOME 'powershell.exe') -ArgumentList $args -Wait -PassThru -NoNewWindow `
        -RedirectStandardOutput $outFile -RedirectStandardError $errFile
    return [pscustomobject]@{
        ExitCode = $p.ExitCode
        StdOut = [System.IO.File]::ReadAllText($outFile)
        StdErr = [System.IO.File]::ReadAllText($errFile)
        OutputPath = if ([string]::IsNullOrEmpty($OutputPath)) { $Fixture.OutputPath } else { $OutputPath }
    }
}

function Get-PreparedJson {
    param([object]$Result)
    Assert-Equal 0 $Result.ExitCode "preparation process exit: $($Result.StdErr)"
    if (-not (Test-Path -LiteralPath $Result.OutputPath -PathType Leaf)) {
        throw "Output JSON not created: $($Result.OutputPath)"
    }
    return (Get-Content -LiteralPath $Result.OutputPath -Raw -Encoding UTF8 | ConvertFrom-Json)
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
        Write-Host "FAIL: ${Name}: $($_.Exception.Message)" -ForegroundColor Red
    }
}

if (-not (Test-Path -LiteralPath $script:sourceScript -PathType Leaf)) {
    throw "PrepareDiagnosticAnalysis.ps1 not found next to Analysis: $script:sourceScript"
}
foreach ($name in $script:parserNames) {
    if (-not (Test-Path -LiteralPath (Join-Path $PSScriptRoot $name) -PathType Leaf)) {
        throw "Parser not found: $name"
    }
}

[System.IO.Directory]::CreateDirectory($script:testRoot) | Out-Null
try {
    Invoke-Test 'Single Run produces expected schema and RunId' {
        $f = New-Fixture
        $d = Get-PreparedJson (Invoke-Preparation $f)
        Assert-Equal 1 $d.schemaVersion 'schemaVersion'
        Assert-Equal '12345' $d.runId 'runId'
        Assert-Count 1 $d.scenarios 'scenarios'
        Assert-Count 1 $d.performanceSessions 'performance sessions'
        Assert-Count 1 $d.diagnosticLogs 'diagnostic logs'
        Assert-Count 0 $d.preparationIssues 'issues'
    }

    Invoke-Test 'Scenario result and timestamps are retained' {
        $f = New-Fixture
        $d = Get-PreparedJson (Invoke-Preparation $f)
        $s = $d.scenarios[0]
        Assert-Equal 'Test.scenario' $s.scenario 'name'
        Assert-Equal 'PASS' $s.result 'result'
        Assert-Equal '2026-11-21T01:00:00.000' $s.start 'start'
        Assert-Equal '2026-11-21T01:00:00.410' $s.end 'end'
    }

    Invoke-Test 'Commands retain order, type, result and duration boundaries' {
        $f = New-Fixture
        $d = Get-PreparedJson (Invoke-Preparation $f)
        $c = @($d.scenarios[0].commands)
        Assert-Count 3 $c 'commands'
        Assert-Equal 1 $c[0].lineNumber 'first line'
        Assert-Equal 'OPEN GUI' $c[0].type 'first type'
        Assert-Equal 'CALC' $c[1].type 'second type'
        Assert-Equal 'PASS' $c[1].result 'second result'
        Assert-Equal '2026-11-21T01:00:00.200' $c[1].start 'second start'
        Assert-Equal '2026-11-21T01:00:00.300' $c[1].end 'second end'
        Assert-Equal $true $c[1].preciseTime 'precision'
        Assert-Equal 'CLOSE GUI' $c[2].type 'third type'
    }

    Invoke-Test 'GUI window links to Performance Session by SessionId' {
        $f = New-Fixture
        $d = Get-PreparedJson (Invoke-Preparation $f)
        $w = $d.scenarios[0].guiWindows[0]
        Assert-Count 1 $d.scenarios[0].guiWindows 'GUI windows'
        Assert-Equal 1 $w.windowOrder 'window order'
        Assert-Equal 'OPEN GUI' $w.openCommand 'open command'
        Assert-Equal 1 $w.openLine 'open line'
        Assert-Equal 3 $w.closeLine 'close line'
        Assert-Equal $script:sessionId $w.sessionId 'window sessionId'
        Assert-Equal 'window-time' $w.matchQuality 'match quality'
        Assert-Equal $script:sessionId $d.performanceSessions[0].sessionId 'performance sessionId'
        Assert-Equal 'window-time' $d.performanceSessions[0].matchQuality 'performance quality'
        Assert-Equal 1 $d.performanceSessions[0].windowOrder 'performance window'
    }

    Invoke-Test 'Source paths are portable relative paths' {
        $f = New-Fixture
        $d = Get-PreparedJson (Invoke-Preparation $f)
        Assert-Equal 'ScenarioRunnerCli/bin/x64/Release/Log/ScenarioRunner_20261121_010000_Test.scenario.log' $d.scenarios[0].sourceFile 'scenario path'
        Assert-Equal 'Gui/bin/x64/Release/Log/ShikigamiApp_20261121.log' $d.diagnosticLogs[0].path 'diagnostic path'
        Assert-Equal $d.scenarios[0].sourceFile $d.performanceSessions[0].scenarioFile 'matched scenario path'
    }

    Invoke-Test 'Diagnostics exclude PerformanceLogger files' {
        $f = New-Fixture
        $d = Get-PreparedJson (Invoke-Preparation $f)
        Assert-Count 1 $d.diagnosticLogs 'diagnostic log files'
        if ($d.diagnosticLogs[0].path -like '*Performance*') { throw 'PerformanceLogger leaked into diagnosticLogs' }
    }

    Invoke-Test 'Missing diagnostic log yields preparation issue and empty array' {
        $f = New-Fixture -WithoutDiagnostic
        $d = Get-PreparedJson (Invoke-Preparation $f)
        Assert-Count 0 $d.diagnosticLogs 'diagnostic logs'
        Assert-Contains 'No Gui diagnostic log in 12345' $d.preparationIssues 'missing log warning'
        Assert-Count 1 $d.performanceSessions 'performance sessions'
    }

    Invoke-Test 'Missing Performance log yields issue but preserves scenario and window' {
        $f = New-Fixture -WithoutPerformance
        $d = Get-PreparedJson (Invoke-Preparation $f)
        Assert-Count 0 $d.performanceSessions 'performance sessions'
        Assert-Count 1 $d.scenarios 'scenarios'
        Assert-Count 1 $d.scenarios[0].guiWindows 'windows'
        Assert-Equal $null $d.scenarios[0].guiWindows[0].sessionId 'unmatched sessionId'
        Assert-Equal 'unmatched' $d.scenarios[0].guiWindows[0].matchQuality 'unmatched quality'
        Assert-Contains 'No Performance log in 12345' $d.preparationIssues 'missing performance'
    }

    Invoke-Test 'Orphan Performance Target warning is preserved' {
        $f = New-Fixture
        $lines = @(
            "2026-11-21 01:00:00.000 Session=$($script:secondSessionId) Target=Calculation Count=1 TotalMs=1.000 AverageMs=1.000 MaxMs=1.000",
            "2026-11-21 01:00:00.100 Session=$($script:sessionId) Event=Begin",
            "2026-11-21 01:00:00.420 Session=$($script:sessionId) Event=End"
        )
        Write-TestFile $f.PerformancePath $lines
        $d = Get-PreparedJson (Invoke-Preparation $f)
        Assert-Contains 'Orphan performance target in run 12345' $d.preparationIssues 'orphan target'
        Assert-Count 1 $d.performanceSessions 'completed session'
    }

    Invoke-Test 'More GUI windows than Performance Sessions is supported' {
        $f = New-Fixture
        Write-TestFile $f.ScenarioPath @(
            '[01:00:00.000] Scenario: Test.scenario',
            '[01:00:00.100] [1] OPEN GUI    START',
            '[01:00:00.150] [1] OPEN GUI    PASS',
            '[01:00:00.200] [2] CLOSE GUI    PASS',
            '[01:00:00.250] [3] OPEN GUI    START',
            '[01:00:00.300] [3] OPEN GUI    PASS',
            '[01:00:00.350] [4] CLOSE GUI    PASS',
            '[01:00:00.400] Scenario Result: PASS'
        )
        $d = Get-PreparedJson (Invoke-Preparation $f)
        Assert-Count 2 $d.scenarios[0].guiWindows 'windows'
        Assert-Count 1 $d.performanceSessions 'sessions'
        Assert-Equal 1 @($d.scenarios[0].guiWindows | Where-Object { -not [string]::IsNullOrEmpty($_.sessionId) }).Count 'assigned windows'
        Assert-Equal 1 @($d.scenarios[0].guiWindows | Where-Object { [string]::IsNullOrEmpty($_.sessionId) }).Count 'unassigned windows'
    }

    Invoke-Test 'Multiple scenarios are loaded and sorted by path' {
        $f = New-Fixture
        $secondFile = Join-Path (Split-Path -Parent $f.ScenarioPath) 'ScenarioRunner_20261121_010001_ZSecond.scenario.log'
        Write-TestFile $secondFile @(
            '[01:00:01.000] Scenario: ZSecond.scenario',
            '[01:00:01.100] [1] OPEN GUI    START',
            '[01:00:01.120] [1] OPEN GUI    PASS',
            '[01:00:01.300] [2] CLOSE GUI    PASS',
            '[01:00:01.310] Scenario Result: PASS'
        )
        $d = Get-PreparedJson (Invoke-Preparation $f)
        Assert-Count 2 $d.scenarios 'scenarios'
        Assert-Equal 'Test.scenario' $d.scenarios[0].scenario 'first scenario'
        Assert-Equal 'ZSecond.scenario' $d.scenarios[1].scenario 'second scenario'
        Assert-Count 1 $d.scenarios[1].guiWindows 'second window'
    }

    Invoke-Test 'Incomplete command has null End and INCOMPLETE status' {
        $f = New-Fixture
        Write-TestFile $f.ScenarioPath @(
            '[01:00:00.000] Scenario: Test.scenario',
            '[01:00:00.100] [1] OPEN GUI    START',
            '[01:00:00.150] [1] OPEN GUI    PASS',
            '[01:00:00.300] [2] CALC    START',
            '[01:00:00.400] [3] CLOSE GUI    PASS',
            '[01:00:00.410] Scenario Result: FAIL'
        )
        $d = Get-PreparedJson (Invoke-Preparation $f)
        Assert-Equal 'FAIL' $d.scenarios[0].result 'scenario result'
        $c = @($d.scenarios[0].commands | Where-Object { $_.type -eq 'CALC' })
        Assert-Count 1 $c 'incomplete command'
        Assert-Equal 'INCOMPLETE' $c[0].result 'command result'
        Assert-Equal $null $c[0].end 'end'
        Assert-Count 1 $d.scenarios[0].guiWindows 'closed GUI window still recorded'
    }

    Invoke-Test 'Unknown ScenarioRunner filename is reported and skipped' {
        $f = New-Fixture
        $unknownFile = Join-Path (Split-Path -Parent $f.ScenarioPath) 'ScenarioRunner_unknown.log'
        Write-TestFile $unknownFile @('unknown')
        $d = Get-PreparedJson (Invoke-Preparation $f)
        Assert-Count 1 $d.scenarios 'valid scenarios'
        Assert-Contains 'Unknown ScenarioRunner log filename' $d.preparationIssues 'unknown filename'
    }

    Invoke-Test 'Missing ScenarioRunner logs fails before writing JSON' {
        $f = New-Fixture -WithoutScenario
        $r = Invoke-Preparation $f
        if ($r.ExitCode -eq 0) { throw 'Expected process failure' }
        if (Test-Path -LiteralPath $r.OutputPath) { throw 'Unexpected JSON output' }
        Assert-Contains 'No ScenarioRunner logs' @($r.StdOut, $r.StdErr) 'expected error'
    }

    Invoke-Test 'Multiple Performance logs fail before writing JSON' {
        $f = New-Fixture
        Write-TestFile (Join-Path $f.RunDirectory 'Other/ShikigamiApp_Performance_20261122.log') @('unused')
        $r = Invoke-Preparation $f
        if ($r.ExitCode -eq 0) { throw 'Expected process failure' }
        if (Test-Path -LiteralPath $r.OutputPath) { throw 'Unexpected JSON output' }
        Assert-Contains 'Expected at most one Performance log' @($r.StdOut, $r.StdErr) 'expected error'
    }

    Invoke-Test 'Non-numeric RunId is rejected' {
        $f = New-Fixture
        $r = Invoke-Preparation $f -RunId 'run123'
        if ($r.ExitCode -eq 0) { throw 'Expected process failure' }
        Assert-Contains 'RunId must be numeric' @($r.StdOut, $r.StdErr) 'expected error'
    }

    Invoke-Test 'Nonexistent RunDirectory is rejected' {
        $f = New-Fixture
        $bad = Join-Path $f.Root 'DoesNotExist'
        $r = Invoke-Preparation $f -RunDirectory $bad
        if ($r.ExitCode -eq 0) { throw 'Expected process failure' }
        Assert-Contains 'Run directory does not exist' @($r.StdOut, $r.StdErr) 'expected error'
    }

    Invoke-Test 'OutputPath outside RunDirectory is rejected' {
        $f = New-Fixture
        $bad = Join-Path $f.Root 'Outside.json'
        $r = Invoke-Preparation $f -OutputPath $bad
        if ($r.ExitCode -eq 0) { throw 'Expected process failure' }
        Assert-Contains 'OutputPath must be directly under RunDirectory' @($r.StdOut, $r.StdErr) 'expected error'
    }

    Invoke-Test 'Custom OutputPath directly inside RunDirectory works' {
        $f = New-Fixture
        $custom = Join-Path $f.RunDirectory 'CustomInput.json'
        $r = Invoke-Preparation $f -OutputPath $custom
        $d = Get-PreparedJson $r
        Assert-Equal 1 $d.schemaVersion 'schema'
        if (Test-Path -LiteralPath $f.OutputPath) { throw 'Default output unexpectedly created' }
    }

    Invoke-Test 'Output file is written as UTF8 without BOM' {
        $f = New-Fixture
        $r = Invoke-Preparation $f
        $d = Get-PreparedJson $r
        $bytes = [System.IO.File]::ReadAllBytes($r.OutputPath)
        if ($bytes.Length -lt 3) { throw 'Output is too short' }
        if ($bytes[0] -eq 239 -and $bytes[1] -eq 187 -and $bytes[2] -eq 191) {
            throw 'Unexpected UTF8 BOM'
        }
        Assert-Equal '12345' $d.runId 'runId'
    }

    Invoke-Test 'Input log files are unchanged after preparation' {
        $f = New-Fixture
        $files = @($f.ScenarioPath, $f.PerformancePath, $f.DiagnosticPath)
        $before = @($files | ForEach-Object { [Convert]::ToBase64String([System.Security.Cryptography.SHA256]::Create().ComputeHash([System.IO.File]::ReadAllBytes($_))) })
        $null = Get-PreparedJson (Invoke-Preparation $f)
        $after = @($files | ForEach-Object { [Convert]::ToBase64String([System.Security.Cryptography.SHA256]::Create().ComputeHash([System.IO.File]::ReadAllBytes($_))) })
        Assert-Equal ($before -join '|') ($after -join '|') 'file hashes'
    }
}
finally {
    if (Test-Path -LiteralPath $script:testRoot) {
        Remove-Item -LiteralPath $script:testRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Write-Host ''
Write-Host "PrepareDiagnosticAnalysis tests: $script:passed passed, $script:failed failed."
if ($script:failed -gt 0) { exit 1 }
exit 0

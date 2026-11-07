param(
    [ValidateRange(3, 100)]
    [int]$Iterations = 7,

    [string]$OutputDirectory = "RunnerPerformance"
)

$ErrorActionPreference = 'Stop'

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

function Measure-Operation {
    param(
        [string]$Name,
        [scriptblock]$Action,
        [int]$Count
    )

    & $Action

    $samples = [System.Collections.Generic.List[double]]::new()

    for ($i = 0; $i -lt $Count; $i++) {
        $watch = [System.Diagnostics.Stopwatch]::StartNew()

        & $Action

        $watch.Stop()
        $samples.Add($watch.Elapsed.TotalMilliseconds)
    }

    $sorted = @($samples | Sort-Object)
    $middle = [int][Math]::Floor($sorted.Count / 2)

    if ($sorted.Count % 2 -eq 0) {
        $median = ($sorted[$middle - 1] + $sorted[$middle]) / 2
    }
    else {
        $median = $sorted[$middle]
    }

    return [pscustomobject]@{
        Name = $Name
        SamplesMs = @($samples.ToArray())
        MedianMs = [Math]::Round($median, 3)
        MinimumMs = [Math]::Round($sorted[0], 3)
        MaximumMs = [Math]::Round($sorted[-1], 3)
    }
}

$cpu = Get-CimInstance Win32_Processor | Select-Object -First 1
$computer = Get-CimInstance Win32_ComputerSystem
$os = Get-CimInstance Win32_OperatingSystem

$environment = [ordered]@{
    RunId = $env:GITHUB_RUN_ID
    RunAttempt = $env:GITHUB_RUN_ATTEMPT
    RunnerName = $env:RUNNER_NAME
    RunnerOS = $env:RUNNER_OS
    RunnerImage = $env:ImageOS
    RunnerImageVersion = $env:ImageVersion
    CPUName = $cpu.Name
    CPUCores = $cpu.NumberOfCores
    LogicalProcessors = $cpu.NumberOfLogicalProcessors
    TotalMemoryBytes = [long]$computer.TotalPhysicalMemory
    WindowsVersion = $os.Version
    WindowsBuild = $os.BuildNumber
    TimestampUtc = [DateTime]::UtcNow.ToString('o')
}

$environment |
    ConvertTo-Json -Depth 5 |
    Set-Content -LiteralPath (Join-Path $OutputDirectory "Environment.json") -Encoding utf8

# Fixed input to make CPU measurements comparable.
$data = New-Object byte[] (4 * 1024 * 1024)
$random = [System.Random]::new(12345)
$random.NextBytes($data)

$cpuAction = {
    $sha = [System.Security.Cryptography.SHA256]::Create()

    try {
        for ($i = 0; $i -lt 32; $i++) {
            $hash = $sha.ComputeHash($data)
        }

        if ($hash.Length -ne 32) {
            throw "CPU benchmark failed."
        }
    }
    finally {
        $sha.Dispose()
    }
}

$processAction = {
    $info = [System.Diagnostics.ProcessStartInfo]::new()
    $info.FileName = Join-Path $env:SystemRoot "System32\cmd.exe"
    $info.Arguments = "/c exit 0"
    $info.UseShellExecute = $false
    $info.CreateNoWindow = $true

    $process = [System.Diagnostics.Process]::Start($info)

    if ($null -eq $process) {
        throw "Process creation failed."
    }

    try {
        $process.WaitForExit()

        if ($process.ExitCode -ne 0) {
            throw "Process exited with code $($process.ExitCode)."
        }
    }
    finally {
        $process.Dispose()
    }
}

$cpuResult = Measure-Operation -Name "SHA256" -Action $cpuAction -Count $Iterations
$processResult = Measure-Operation -Name "ProcessStartup" -Action $processAction -Count $Iterations

$benchmark = [ordered]@{
    RunId = $env:GITHUB_RUN_ID
    RunAttempt = $env:GITHUB_RUN_ATTEMPT
    TimestampUtc = [DateTime]::UtcNow.ToString('o')
    CPU = $cpuResult
    ProcessStartup = $processResult
}

$benchmark |
    ConvertTo-Json -Depth 10 |
    Set-Content -LiteralPath (Join-Path $OutputDirectory "Benchmark.json") -Encoding utf8

Write-Host "Runner: $($environment.RunnerName)"
Write-Host "CPU: $($environment.CPUName)"
Write-Host "CPU benchmark median: $($cpuResult.MedianMs) ms"
Write-Host "Process startup median: $($processResult.MedianMs) ms"
Write-Host "Runner performance measurement completed."

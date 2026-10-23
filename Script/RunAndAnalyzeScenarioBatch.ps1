param(
    [ValidateRange(1, 20)]
    [int]$RunCount = 5,

    [ValidateRange(5, 300)]
    [int]$PollIntervalSeconds = 30,

    [ValidateRange(1, 1440)]
    [int]$TimeoutMinutes = 20
)

$ErrorActionPreference = 'Stop'

$repository = 'Staraptor0398/ShikigamiStatusCalculator'
$runScript = Join-Path $PSScriptRoot 'RunScenarioBatch.ps1'
$analyzeScript = Join-Path $PSScriptRoot 'AnalyzeScenarioBatch.ps1'
$runIdFile = Join-Path $PSScriptRoot 'Result/RunIds.txt'

if (-not (Test-Path -LiteralPath $runScript -PathType Leaf)) {
    throw "Run script not found: $runScript"
}
if (-not (Test-Path -LiteralPath $analyzeScript -PathType Leaf)) {
    throw "Analysis script not found: $analyzeScript"
}
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw 'GitHub CLI (gh) is missing.'
}

# Dispatch all runs and reuse the RunIds.txt produced by the existing script.
& $runScript -RunCount $RunCount

if (-not (Test-Path -LiteralPath $runIdFile -PathType Leaf)) {
    throw "Run ID file not found: $runIdFile"
}

$runIds = @((Get-Content -LiteralPath $runIdFile | ForEach-Object { $_.Trim() }) | Where-Object { $_ -match '^\d+$' } | Select-Object -Unique)
if ($runIds.Count -ne $RunCount) {
    throw "Expected $RunCount run IDs, found $($runIds.Count). Check $runIdFile."
}

$deadline = (Get-Date).AddMinutes($TimeoutMinutes)
$completed = @{}
$failed = [System.Collections.Generic.List[string]]::new()

Write-Host "Waiting for $($runIds.Count) GitHub Actions runs (timeout: $TimeoutMinutes minutes)..."

while ($completed.Count -lt $runIds.Count) {
    foreach ($runId in $runIds) {
        if ($completed.ContainsKey($runId)) { continue }

        try {
            $response = @(& gh run view $runId --repo $repository --json status,conclusion 2>&1)
            if ($LASTEXITCODE -ne 0) {
                Write-Warning "Could not read run $runId. Retrying on next poll. $($response -join ' ')"
                continue
            }
        }
        catch {
            Write-Warning "Could not read run $runId. Retrying on next poll. $($_.Exception.Message)"
            continue
        }

        try {
            $state = ($response -join "`n") | ConvertFrom-Json
        }
        catch {
            Write-Warning "Invalid response for run $runId. Retrying on next poll."
            continue
        }

        if ($state.status -eq 'completed') {
            $completed[$runId] = [string]$state.conclusion
            Write-Host "Run $runId completed: $($state.conclusion) ($($completed.Count)/$($runIds.Count))"
            if ($state.conclusion -ne 'success') {
                $failed.Add("$runId ($($state.conclusion))")
            }
        }
        else {
            Write-Host "Run $runId : $($state.status)"
        }
    }

    if ($completed.Count -eq $runIds.Count) { break }
    if ((Get-Date) -ge $deadline) {
        $remaining = @($runIds | Where-Object { -not $completed.ContainsKey($_) })
        throw "Timed out waiting for GitHub Actions. Incomplete run IDs: $($remaining -join ', '). Completed runs remain in $runIdFile."
    }

    $remainingSeconds = [math]::Ceiling(($deadline - (Get-Date)).TotalSeconds)
    Start-Sleep -Seconds ([math]::Min($PollIntervalSeconds, [math]::Max(1, $remainingSeconds)))
}

# Analyze even if one or more workflow runs failed. Their logs may explain why.
Write-Host 'All runs completed. Starting Scenario Runner Batch analysis...'
& $analyzeScript -RunIdFile $runIdFile

if ($failed.Count -gt 0) {
    throw "Analysis completed, but some GitHub Actions runs were not successful: $($failed -join ', ')"
}

Write-Host 'All GitHub Actions runs succeeded, and analysis completed.'

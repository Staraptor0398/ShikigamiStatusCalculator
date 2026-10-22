param(
    [ValidateRange(1, 20)]
    [int]$RunCount = 5
)

$ErrorActionPreference = 'Stop'

$repository = 'Staraptor0398/ShikigamiStatusCalculator'
$workflow = 'scenario-runner-batch.yml'
$root = Split-Path -Parent $PSScriptRoot
$resultDirectory = Join-Path $PSScriptRoot 'Result'
$runIdFile = Join-Path $resultDirectory 'RunIds.txt'

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw 'GitHub CLI (gh) is missing. Install it and run gh auth login.'
}
if (-not (Test-Path (Join-Path $root '.git'))) {
    throw "Git repository not found at: $root"
}

& gh auth status 1>$null 2>$null
if ($LASTEXITCODE -ne 0) {
    throw 'GitHub CLI authentication missing. Run gh auth login.'
}

$commitSha = (& git -C $root rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $commitSha -notmatch '^[0-9a-f]{40}$') {
    throw 'Failed to resolve the local HEAD commit.'
}

New-Item -ItemType Directory -Force -Path $resultDirectory | Out-Null
# A new invocation replaces the previous set. If dispatch stops partway through,
# the file contains only the successfully dispatched run IDs.
[System.IO.File]::WriteAllText($runIdFile, '', [System.Text.UTF8Encoding]::new($false))

Write-Host "Repository: $repository"
Write-Host "Commit:     $commitSha"
Write-Host "Run count:  $RunCount"
Write-Host "Run ID file: $runIdFile"

for ($i = 1; $i -le $RunCount; $i++) {
    Write-Host "Starting Scenario Runner Batch ($i/$RunCount)..."

    $output = @(& gh workflow run $workflow --repo $repository --ref master -f "ref=$commitSha" -f 'enable_live_stream=false' 2>&1)
    if ($LASTEXITCODE -ne 0) {
        $output | ForEach-Object { Write-Host $_ }
        throw "Failed to dispatch run $i. Run IDs dispatched so far are saved in $runIdFile."
    }

    $output | ForEach-Object { Write-Host $_ }
    $runId = $null
    foreach ($line in $output) {
        if ([string]$line -match '/actions/runs/(\d+)') {
            $runId = $Matches[1]
            break
        }
    }
    if (-not $runId) {
        throw 'Dispatch succeeded but the run ID could not be parsed. Check the GitHub Actions page.'
    }

    [System.IO.File]::AppendAllText($runIdFile, "$runId`r`n", [System.Text.UTF8Encoding]::new($false))
}

Write-Host "All $RunCount workflow dispatch requests submitted."
Write-Host "Run IDs saved to $runIdFile"

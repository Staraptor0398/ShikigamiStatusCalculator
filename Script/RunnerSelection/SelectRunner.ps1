param(
    [ValidateSet('Unknown', 'Fastest', 'Target')]
    [string]$SelectionMode = 'Unknown',
    [string]$CandidatesDirectory = 'candidates',
    [string]$CatalogPath = 'Data/RunnerPerformance/CpuCatalog.json',
    [string]$OutputDirectory = 'RunnerSelection',
    [int]$ExpectedCandidates = 5
)

$ErrorActionPreference = 'Stop'

$targetCpuName = 'Intel(R) Xeon(R) 6973P-C'

$catalog = Get-Content -LiteralPath $CatalogPath -Raw |
    ConvertFrom-Json

if ($catalog.schemaVersion -ne 1 -or $null -eq $catalog.cpuModels) {
    throw 'Unsupported CPU catalog format.'
}

$cpuModels = @{}

foreach ($model in $catalog.cpuModels) {
    if ([string]::IsNullOrWhiteSpace($model.name)) {
        throw 'CPU catalog contains an empty model name.'
    }

    $key = $model.name.Trim().ToUpperInvariant()

    if ($cpuModels.ContainsKey($key)) {
        throw "Duplicate CPU model in catalog: $($model.name)"
    }

    $cpuModels[$key] = $model
}

$files = @(Get-ChildItem -LiteralPath $CandidatesDirectory `
    -Filter result.json -Recurse -File)

if ($files.Count -ne $ExpectedCandidates) {
    throw "Expected $ExpectedCandidates candidates but found $($files.Count)."
}

$candidates = @($files | ForEach-Object {
    Get-Content -LiteralPath $_.FullName -Raw |
        ConvertFrom-Json
})

$rankedCandidates = @(
    foreach ($candidate in $candidates) {
        if ([string]::IsNullOrWhiteSpace($candidate.CpuName)) {
            throw "Empty CPU name in candidate $($candidate.CandidateId)."
        }

        $candidateId = [int]$candidate.CandidateId
        $cpuName = $candidate.CpuName.Trim()
        $key = $cpuName.ToUpperInvariant()
        $knownCpu = $cpuModels.ContainsKey($key)

        $batchMedianSeconds = $null
        $batchSampleCount = 0
        $catalogSha256MedianMs = $null

        if ($knownCpu) {
            $model = $cpuModels[$key]

            $batchMedianSeconds = $model.scenarioBatchMedianSeconds
            $batchSampleCount = [int]$model.scenarioBatchSampleCount
            $catalogSha256MedianMs = $model.sha256MedianMs
        }

        $hasBatchMeasurement = $knownCpu -and
            $null -ne $batchMedianSeconds -and
            $batchSampleCount -gt 0

        $priorityGroup = 1

        switch ($SelectionMode) {
            'Unknown' {
                if (-not $knownCpu) {
                    $priorityGroup = 0
                }
            }
            'Target' {
                if ($cpuName.Equals(
                    $targetCpuName,
                    [StringComparison]::OrdinalIgnoreCase
                )) {
                    $priorityGroup = 0
                }
            }
        }

        $performanceGroup = 2

        if ($hasBatchMeasurement) {
            $performanceGroup = 0
        }
        elseif ($knownCpu) {
            $performanceGroup = 1
        }

        $batchSortValue = [double]::PositiveInfinity

        if ($hasBatchMeasurement) {
            $batchSortValue = [double]$batchMedianSeconds
        }

        $sha256SortValue = [double]$candidate.Sha256MedianMs

        if ($null -ne $catalogSha256MedianMs) {
            $sha256SortValue = [double]$catalogSha256MedianMs
        }

        [pscustomobject]@{
            CandidateId = $candidateId
            CpuName = $cpuName
            RunnerName = $candidate.RunnerName
            KnownCpu = $knownCpu
            ScenarioBatchMedianSeconds = $batchMedianSeconds
            ScenarioBatchSampleCount = $batchSampleCount
            Sha256MedianMs = [double]$candidate.Sha256MedianMs
            ProcessStartupMedianMs = [double]$candidate.ProcessStartupMedianMs
            PriorityGroup = $priorityGroup
            PerformanceGroup = $performanceGroup
            BatchSortValue = $batchSortValue
            Sha256SortValue = $sha256SortValue
        }
    }
)

$duplicateIds = @($rankedCandidates |
    Group-Object CandidateId |
    Where-Object { $_.Count -gt 1 })

if ($duplicateIds.Count -gt 0) {
    throw 'Duplicate candidate IDs were found.'
}

$ranked = @($rankedCandidates |
    Sort-Object `
        PriorityGroup,
        PerformanceGroup,
        BatchSortValue,
        Sha256SortValue,
        ProcessStartupMedianMs,
        CandidateId)

for ($i = 0; $i -lt $ranked.Count; $i++) {
    $ranked[$i] |
        Add-Member -NotePropertyName Priority `
            -NotePropertyValue ($i + 1)
}

$winner = $ranked[0]

$selection = [ordered]@{
    SelectionMode = $SelectionMode
    SelectedCandidateId = $winner.CandidateId
    CpuName = $winner.CpuName
    RunnerName = $winner.RunnerName
    Priority = $winner.Priority
    TimestampUtc = [DateTime]::UtcNow.ToString('o')
}

New-Item -ItemType Directory -Path $OutputDirectory -Force |
    Out-Null

$selection |
    ConvertTo-Json -Depth 5 |
    Set-Content -LiteralPath (
        Join-Path $OutputDirectory 'selection.json'
    ) -Encoding utf8

$ranked |
    Select-Object `
        Priority,
        CandidateId,
        CpuName,
        RunnerName,
        KnownCpu,
        ScenarioBatchMedianSeconds,
        ScenarioBatchSampleCount,
        Sha256MedianMs,
        ProcessStartupMedianMs,
        PriorityGroup,
        PerformanceGroup |
    ConvertTo-Json -Depth 5 |
    Set-Content -LiteralPath (
        Join-Path $OutputDirectory 'ranking.json'
    ) -Encoding utf8

Write-Host "Selection mode: $SelectionMode"

$ranked |
    Select-Object `
        Priority,
        CandidateId,
        CpuName,
        KnownCpu,
        ScenarioBatchMedianSeconds,
        ProcessStartupMedianMs |
    Format-Table -AutoSize |
    Out-String |
    Write-Host

Write-Host '========================================'
Write-Host 'SELECTED RUNNER'
Write-Host '========================================'
Write-Host "Candidate: $($winner.CandidateId)"
Write-Host "CPU: $($winner.CpuName)"
Write-Host "Runner: $($winner.RunnerName)"
Write-Host "Priority: $($winner.Priority)"

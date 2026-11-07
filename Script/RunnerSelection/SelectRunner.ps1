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
$anomalyThreshold = 2.0

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

        $sha256MedianMs = [double]$candidate.Sha256MedianMs
        $processStartupMedianMs = [double]$candidate.ProcessStartupMedianMs

        if (
            -not [double]::IsFinite($sha256MedianMs) -or
            $sha256MedianMs -le 0 -or
            -not [double]::IsFinite($processStartupMedianMs) -or
            $processStartupMedianMs -le 0
        ) {
            throw "Invalid benchmark values in candidate $candidateId."
        }

        $batchMedianSeconds = $null
        $batchSampleCount = 0
        $catalogSha256MedianMs = $null
        $catalogProcessStartupMedianMs = $null

        if ($knownCpu) {
            $model = $cpuModels[$key]

            $batchMedianSeconds = $model.scenarioBatchMedianSeconds
            $batchSampleCount = [int]$model.scenarioBatchSampleCount
            $catalogSha256MedianMs = $model.sha256MedianMs
            $catalogProcessStartupMedianMs = $model.processStartupMedianMs
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

        $sha256SortValue = $sha256MedianMs

        if ($null -ne $catalogSha256MedianMs) {
            $sha256SortValue = [double]$catalogSha256MedianMs
        }

        $sha256Ratio = $null
        $processStartupRatio = $null
        $anomalyEvaluated = $false
        $isAnomalous = $false
        $anomalyReason = $null
        $anomalyScore = [double]::PositiveInfinity

        $hasSha256Baseline = $null -ne $catalogSha256MedianMs -and
            [double]$catalogSha256MedianMs -gt 0

        $hasStartupBaseline = $null -ne $catalogProcessStartupMedianMs -and
            [double]$catalogProcessStartupMedianMs -gt 0

        if ($hasSha256Baseline -and $hasStartupBaseline) {
            $anomalyEvaluated = $true

            $sha256Ratio = $sha256MedianMs /
                [double]$catalogSha256MedianMs

            $processStartupRatio = $processStartupMedianMs /
                [double]$catalogProcessStartupMedianMs

            $anomalyScore = [Math]::Max(
                $sha256Ratio,
                $processStartupRatio
            )

            $isAnomalous = $sha256Ratio -ge $anomalyThreshold -and
                $processStartupRatio -ge $anomalyThreshold

            if ($isAnomalous) {
                $anomalyReason = (
                    "SHA256: {0:N2}x, ProcessStartup: {1:N2}x " +
                    "(threshold: {2:N2}x for both)"
                ) -f $sha256Ratio, $processStartupRatio, $anomalyThreshold
            }
        }

        [pscustomobject]@{
            CandidateId = $candidateId
            CpuName = $cpuName
            RunnerName = $candidate.RunnerName
            KnownCpu = $knownCpu
            ScenarioBatchMedianSeconds = $batchMedianSeconds
            ScenarioBatchSampleCount = $batchSampleCount
            Sha256MedianMs = $sha256MedianMs
            ProcessStartupMedianMs = $processStartupMedianMs
            CatalogSha256MedianMs = $catalogSha256MedianMs
            CatalogProcessStartupMedianMs = $catalogProcessStartupMedianMs
            Sha256Ratio = $sha256Ratio
            ProcessStartupRatio = $processStartupRatio
            AnomalyEvaluated = $anomalyEvaluated
            IsAnomalous = $isAnomalous
            AnomalyReason = $anomalyReason
            AnomalyScore = $anomalyScore
            Excluded = $false
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

$normalCandidates = @($rankedCandidates |
    Where-Object { -not $_.IsAnomalous })

$anomalousCandidates = @($rankedCandidates |
    Where-Object { $_.IsAnomalous })

$fallbackUsed = $normalCandidates.Count -eq 0

if ($fallbackUsed) {
    Write-Warning 'All candidates are anomalous. Using anomaly fallback.'

    $ranked = @($rankedCandidates |
        Sort-Object `
            AnomalyScore,
            PriorityGroup,
            PerformanceGroup,
            BatchSortValue,
            Sha256SortValue,
            ProcessStartupMedianMs,
            CandidateId)
}
else {
    foreach ($candidate in $anomalousCandidates) {
        $candidate.Excluded = $true
    }

    $ranked = @($rankedCandidates |
        Sort-Object `
            Excluded,
            PriorityGroup,
            PerformanceGroup,
            BatchSortValue,
            Sha256SortValue,
            ProcessStartupMedianMs,
            CandidateId)
}

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
    AnomalyThreshold = $anomalyThreshold
    AnomalyFallbackUsed = $fallbackUsed
    AnomalousCandidateCount = $anomalousCandidates.Count
    ExcludedCandidateCount = @(
        $rankedCandidates | Where-Object { $_.Excluded }
    ).Count
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
        CatalogSha256MedianMs,
        CatalogProcessStartupMedianMs,
        Sha256Ratio,
        ProcessStartupRatio,
        AnomalyEvaluated,
        IsAnomalous,
        AnomalyReason,
        Excluded,
        PriorityGroup,
        PerformanceGroup |
    ConvertTo-Json -Depth 5 |
    Set-Content -LiteralPath (
        Join-Path $OutputDirectory 'ranking.json'
    ) -Encoding utf8

Write-Host "Selection mode: $SelectionMode"
Write-Host "Anomaly threshold: $anomalyThreshold"
Write-Host "Anomalous candidates: $($anomalousCandidates.Count)"
Write-Host "Anomaly fallback used: $fallbackUsed"

$ranked |
    Select-Object `
        Priority,
        CandidateId,
        CpuName,
        KnownCpu,
        ScenarioBatchMedianSeconds,
        Sha256Ratio,
        ProcessStartupRatio,
        IsAnomalous,
        Excluded |
    Format-Table -AutoSize |
    Out-String |
    Write-Host

foreach ($candidate in $anomalousCandidates) {
    Write-Warning (
        "Anomalous candidate {0} ({1}): {2}" -f
        $candidate.CandidateId,
        $candidate.CpuName,
        $candidate.AnomalyReason
    )
}

Write-Host '========================================'
Write-Host 'SELECTED RUNNER'
Write-Host '========================================'
Write-Host "Candidate: $($winner.CandidateId)"
Write-Host "CPU: $($winner.CpuName)"
Write-Host "Runner: $($winner.RunnerName)"
Write-Host "Priority: $($winner.Priority)"
Write-Host "Anomaly fallback used: $fallbackUsed"

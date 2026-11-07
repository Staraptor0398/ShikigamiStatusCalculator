param(
    [string]$DiscoveryDirectory = 'discoveries',
    [string]$CatalogPath = 'Data/RunnerPerformance/CpuCatalog.json',
    [string]$SummaryDirectory = 'summary',
    [int]$ExpectedSamples = 25
)

$ErrorActionPreference = 'Stop'

$files = @(Get-ChildItem -LiteralPath $DiscoveryDirectory -Filter result.json -Recurse -File)
if ($files.Count -eq 0) {
    throw 'No CPU discovery results were downloaded.'
}

$results = @($files | ForEach-Object {
    Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json
} | Sort-Object CandidateId)

$catalog = Get-Content -LiteralPath $CatalogPath -Raw | ConvertFrom-Json
if ($catalog.schemaVersion -ne 1 -or $null -eq $catalog.cpuModels) {
    throw 'Unsupported CPU catalog format.'
}

$known = @{}
foreach ($model in $catalog.cpuModels) {
    if ([string]::IsNullOrWhiteSpace($model.name)) {
        throw 'CPU catalog contains an empty model name.'
    }
    $key = $model.name.Trim().ToUpperInvariant()
    if ($known.ContainsKey($key)) {
        throw "Duplicate CPU model in catalog: $($model.name)"
    }
    $known[$key] = $true
}

New-Item -ItemType Directory -Path $SummaryDirectory -Force | Out-Null
$rows = @($results | ForEach-Object {
    $candidate = $_
    if ([string]::IsNullOrWhiteSpace($candidate.CpuName)) {
        throw "Empty CPU name in candidate $($candidate.CandidateId)."
    }
    $name = $candidate.CpuName.Trim()
    [pscustomobject]@{
        CandidateId = [int]$candidate.CandidateId
        CpuName = $name
        KnownCpu = $known.ContainsKey($name.ToUpperInvariant())
        RunnerName = $candidate.RunnerName
        Sha256MedianMs = [double]$candidate.Sha256MedianMs
        ProcessStartupMedianMs = [double]$candidate.ProcessStartupMedianMs
        RunId = $candidate.RunId
        RunAttempt = $candidate.RunAttempt
        TimestampUtc = $candidate.TimestampUtc
    }
})
$rows | Export-Csv (Join-Path $SummaryDirectory 'RunnerCpuSamples.csv') -NoTypeInformation -Encoding utf8

$models = @($rows | Group-Object CpuName | ForEach-Object {
    $group = $_.Group
    [pscustomobject]@{
        CpuName = $_.Name
        Count = $_.Count
        KnownCpu = $group[0].KnownCpu
        BestSha256MedianMs = ($group | Measure-Object Sha256MedianMs -Minimum).Minimum
        BestProcessStartupMedianMs = ($group | Measure-Object ProcessStartupMedianMs -Minimum).Minimum
    }
} | Sort-Object CpuName)
$models | Export-Csv (Join-Path $SummaryDirectory 'RunnerCpuModels.csv') -NoTypeInformation -Encoding utf8
ConvertTo-Json -InputObject $models -Depth 5 | Set-Content (Join-Path $SummaryDirectory 'RunnerCpuModels.json') -Encoding utf8

$newModels = @($rows | Where-Object { -not $_.KnownCpu } | Group-Object CpuName | ForEach-Object {
    $first = $_.Group | Sort-Object TimestampUtc, CandidateId | Select-Object -First 1
    [pscustomobject][ordered]@{
        name = $_.Name
        firstSeenRunId = [long]$first.RunId
        firstSeenAtUtc = [string]$first.TimestampUtc
        sha256MedianMs = [double]$first.Sha256MedianMs
        processStartupMedianMs = [double]$first.ProcessStartupMedianMs
        scenarioBatchMedianSeconds = $null
        scenarioBatchSampleCount = 0
    }
} | Sort-Object name)

foreach ($model in $newModels) {
    $catalog.cpuModels += $model
    Write-Host "NEW CPU: $($model.name)"
}

if ($newModels.Count -gt 0) {
    $catalog.cpuModels = @($catalog.cpuModels | Sort-Object name)
    ConvertTo-Json -InputObject $catalog -Depth 10 | Set-Content -LiteralPath $CatalogPath -Encoding utf8
}

Write-Host "Collected samples: $($rows.Count) / $ExpectedSamples"
Write-Host "Unique CPU models: $($models.Count)"
Write-Host "New CPU models: $($newModels.Count)"
$models | Format-Table -AutoSize

if ($rows.Count -ne $ExpectedSamples) {
    Write-Warning "Expected $ExpectedSamples results but received $($rows.Count)."
}

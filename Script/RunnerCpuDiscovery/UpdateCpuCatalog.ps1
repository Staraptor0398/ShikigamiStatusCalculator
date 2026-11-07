param(
    [string]$DiscoveryDirectory = 'discoveries',
    [string]$CatalogPath = 'Data/RunnerPerformance/CpuCatalog.json',
    [string]$SummaryDirectory = 'summary',
    [int]$ExpectedSamples = 25
)

$ErrorActionPreference = 'Stop'

function Get-Median {
    param(
        [double[]]$Values
    )

    if ($null -eq $Values -or $Values.Count -eq 0) {
        throw 'Cannot calculate median from empty values.'
    }

    $sorted = @($Values | Sort-Object)
    $middle = [int][Math]::Floor($sorted.Count / 2)

    if ($sorted.Count % 2 -eq 1) {
        return [double]$sorted[$middle]
    }

    return ([double]$sorted[$middle - 1] + [double]$sorted[$middle]) / 2.0
}

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

ConvertTo-Json -InputObject $models -Depth 5 |
    Set-Content (Join-Path $SummaryDirectory 'RunnerCpuModels.json') -Encoding utf8

$catalogChanged = $false
$updatedModels = 0

foreach ($model in $catalog.cpuModels) {
    $key = $model.name.Trim().ToUpperInvariant()

    $samples = @($rows | Where-Object {
        $_.CpuName.ToUpperInvariant() -eq $key
    })

    if ($samples.Count -eq 0) {
        continue
    }

    $modelChanged = $false

    if ($null -eq $model.sha256MedianMs) {
        $values = [double[]]@($samples | ForEach-Object {
            $_.Sha256MedianMs
        })

        $model.sha256MedianMs = Get-Median -Values $values
        $modelChanged = $true

        Write-Host "FILLED SHA256: $($model.name) = $($model.sha256MedianMs) ms"
    }

    if ($null -eq $model.processStartupMedianMs) {
        $values = [double[]]@($samples | ForEach-Object {
            $_.ProcessStartupMedianMs
        })

        $model.processStartupMedianMs = Get-Median -Values $values
        $modelChanged = $true

        Write-Host "FILLED PROCESS STARTUP: $($model.name) = $($model.processStartupMedianMs) ms"
    }

    if ($modelChanged) {
        $updatedModels++
        $catalogChanged = $true
    }
}

$newModels = @($rows |
    Where-Object { -not $_.KnownCpu } |
    Group-Object CpuName |
    ForEach-Object {
        $group = $_.Group
        $first = $group |
            Sort-Object TimestampUtc, CandidateId |
            Select-Object -First 1

        $sha256Values = [double[]]@($group | ForEach-Object {
            $_.Sha256MedianMs
        })

        $startupValues = [double[]]@($group | ForEach-Object {
            $_.ProcessStartupMedianMs
        })

        [pscustomobject][ordered]@{
            name = $_.Name
            firstSeenRunId = [long]$first.RunId
            firstSeenAtUtc = [string]$first.TimestampUtc
            sha256MedianMs = Get-Median -Values $sha256Values
            processStartupMedianMs = Get-Median -Values $startupValues
            scenarioBatchMedianSeconds = $null
            scenarioBatchSampleCount = 0
        }
    } |
    Sort-Object name)

foreach ($model in $newModels) {
    $catalog.cpuModels += $model
    $catalogChanged = $true

    Write-Host "NEW CPU: $($model.name)"
}

if ($catalogChanged) {
    $catalog.cpuModels = @($catalog.cpuModels | Sort-Object name)

    ConvertTo-Json -InputObject $catalog -Depth 10 |
        Set-Content -LiteralPath $CatalogPath -Encoding utf8
}

Write-Host "Collected samples: $($rows.Count) / $ExpectedSamples"
Write-Host "Unique CPU models: $($models.Count)"
Write-Host "New CPU models: $($newModels.Count)"
Write-Host "Updated CPU models: $updatedModels"
Write-Host "Catalog changed: $catalogChanged"

$models | Format-Table -AutoSize

if ($rows.Count -ne $ExpectedSamples) {
    Write-Warning "Expected $ExpectedSamples results but received $($rows.Count)."
}

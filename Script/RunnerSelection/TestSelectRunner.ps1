param(
    [string]$SelectorPath = 'Script/RunnerSelection/SelectRunner.ps1',
    [string]$CatalogPath = 'Data/RunnerPerformance/CpuCatalog.json'
)

$ErrorActionPreference = 'Stop'

$selector = (Resolve-Path -LiteralPath $SelectorPath).Path
$catalog = (Resolve-Path -LiteralPath $CatalogPath).Path

$temporaryDirectory = Join-Path `
    ([System.IO.Path]::GetTempPath()) `
    ("RunnerSelectionTest_" + [Guid]::NewGuid().ToString('N'))

$passed = 0
$failed = 0

$cpu7763 = 'AMD EPYC 7763 64-Core Processor'
$cpu9V45 = 'AMD EPYC 9V45 96-Core Processor'
$cpu9V74 = 'AMD EPYC 9V74 80-Core Processor'
$cpu8573 = 'INTEL(R) XEON(R) PLATINUM 8573C'
$cpuTarget = 'Intel(R) Xeon(R) 6973P-C'
$cpuUnknown = 'Unknown Test CPU'

function New-Candidate {
    param(
        [int]$Id,
        [string]$CpuName,
        [double]$Sha256,
        [double]$Startup
    )

    return [pscustomobject]@{
        CandidateId = $Id
        CpuName = $CpuName
        RunnerName = "TestRunner-$Id"
        Sha256MedianMs = $Sha256
        ProcessStartupMedianMs = $Startup
    }
}

function New-NormalCandidates {
    return @(
        (New-Candidate 1 $cpu7763 86.0855 17.3125)
        (New-Candidate 2 $cpu8573 88.7525 17.6615)
        (New-Candidate 3 $cpu9V45 65.145 11.848)
        (New-Candidate 4 $cpu9V74 75.2875 13.439)
        (New-Candidate 5 $cpu7763 86.0855 17.3125)
    )
}

function Assert-Equal {
    param(
        $Actual,
        $Expected,
        [string]$Message
    )

    if ($Actual -ne $Expected) {
        throw "$Message. Expected: $Expected, Actual: $Actual"
    }
}

function Invoke-SelectionTest {
    param(
        [string]$Name,
        [string]$Mode,
        [object[]]$Candidates,
        [int]$ExpectedWinner,
        [int]$ExpectedAnomalies = 0,
        [int]$ExpectedExcluded = 0,
        [bool]$ExpectedFallback = $false,
        [bool]$ExpectFailure = $false
    )

    $script:testNumber++
    $testDirectory = Join-Path `
        $temporaryDirectory `
        ("Test_" + $script:testNumber)

    $candidateDirectory = Join-Path $testDirectory 'candidates'
    $outputDirectory = Join-Path $testDirectory 'output'

    New-Item -ItemType Directory -Force `
        -Path $candidateDirectory | Out-Null

    foreach ($candidate in $Candidates) {
        $directory = Join-Path `
            $candidateDirectory `
            ([string]$candidate.CandidateId + '_' +
                [Guid]::NewGuid().ToString('N'))

        New-Item -ItemType Directory -Force `
            -Path $directory | Out-Null

        $candidate |
            ConvertTo-Json -Depth 5 |
            Set-Content `
                -LiteralPath (Join-Path $directory 'result.json') `
                -Encoding utf8
    }

    try {
        & $selector `
            -SelectionMode $Mode `
            -CandidatesDirectory $candidateDirectory `
            -CatalogPath $catalog `
            -OutputDirectory $outputDirectory `
            -ExpectedCandidates $Candidates.Count `
            *> $null

        if ($ExpectFailure) {
            throw 'Expected selector failure, but execution succeeded.'
        }

        $selection = Get-Content `
            -LiteralPath (Join-Path $outputDirectory 'selection.json') `
            -Raw | ConvertFrom-Json

        $ranking = @(
            Get-Content `
                -LiteralPath (Join-Path $outputDirectory 'ranking.json') `
                -Raw | ConvertFrom-Json
        )

        Assert-Equal `
            $selection.SelectedCandidateId `
            $ExpectedWinner `
            'Selected candidate mismatch'

        Assert-Equal `
            $selection.AnomalousCandidateCount `
            $ExpectedAnomalies `
            'Anomalous candidate count mismatch'

        Assert-Equal `
            $selection.ExcludedCandidateCount `
            $ExpectedExcluded `
            'Excluded candidate count mismatch'

        Assert-Equal `
            $selection.AnomalyFallbackUsed `
            $ExpectedFallback `
            'Anomaly fallback mismatch'

        Assert-Equal `
            $ranking.Count `
            $Candidates.Count `
            'Ranking candidate count mismatch'

        Assert-Equal `
            $ranking[0].CandidateId `
            $ExpectedWinner `
            'Ranking winner mismatch'

        $priorities = @($ranking | ForEach-Object {
            [int]$_.Priority
        })

        for ($i = 0; $i -lt $priorities.Count; $i++) {
            Assert-Equal `
                $priorities[$i] `
                ($i + 1) `
                'Ranking priority mismatch'
        }

        $script:passed++
        Write-Host "[PASS] $Name"
    }
    catch {
        if ($ExpectFailure -and
            $_.Exception.Message -notlike
                'Expected selector failure*') {
            $script:passed++
            Write-Host "[PASS] $Name"
        }
        else {
            $script:failed++
            Write-Host "[FAIL] $Name" -ForegroundColor Red
            Write-Host "       $($_.Exception.Message)"
        }
    }
}

$testNumber = 0

try {
    New-Item -ItemType Directory -Force `
        -Path $temporaryDirectory | Out-Null

    # Normal selection
    Invoke-SelectionTest `
        -Name 'Fastest selects normal 9V45' `
        -Mode Fastest `
        -Candidates (New-NormalCandidates) `
        -ExpectedWinner 3

    # Actual benchmark values from Run #7
    $run7 = @(
        (New-Candidate 1 $cpu7763 86.539 16.621)
        (New-Candidate 2 $cpu8573 93.430 17.980)
        (New-Candidate 3 $cpu9V45 498.776 92.917)
        (New-Candidate 4 $cpu7763 85.957 17.486)
        (New-Candidate 5 $cpu7763 86.179 17.211)
    )

    Invoke-SelectionTest `
        -Name 'Run 7 excludes anomalous 9V45' `
        -Mode Fastest `
        -Candidates $run7 `
        -ExpectedWinner 2 `
        -ExpectedAnomalies 1 `
        -ExpectedExcluded 1

    # All candidates anomalous
    $allAnomalous = @(
        (New-Candidate 1 $cpu7763 258.2565 51.9375)
        (New-Candidate 2 $cpu8573 355.0100 70.6460)
        (New-Candidate 3 $cpu9V45 130.2900 23.6960)
        (New-Candidate 4 $cpu9V74 225.8625 40.3170)
        (New-Candidate 5 $cpu7763 344.3420 69.2500)
    )

    Invoke-SelectionTest `
        -Name 'All anomalous uses lowest anomaly score' `
        -Mode Fastest `
        -Candidates $allAnomalous `
        -ExpectedWinner 3 `
        -ExpectedAnomalies 5 `
        -ExpectedExcluded 0 `
        -ExpectedFallback $true

    # Unknown CPU priority
    $unknownCandidates = New-NormalCandidates
    $unknownCandidates[4] = New-Candidate `
        5 $cpuUnknown 90.0 18.0

    Invoke-SelectionTest `
        -Name 'Unknown mode prioritizes unknown CPU' `
        -Mode Unknown `
        -Candidates $unknownCandidates `
        -ExpectedWinner 5

    # Target CPU priority
    $targetCandidates = New-NormalCandidates
    $targetCandidates[4] = New-Candidate `
        5 $cpuTarget 79.8 15.76

    Invoke-SelectionTest `
        -Name 'Target mode prioritizes target CPU' `
        -Mode Target `
        -Candidates $targetCandidates `
        -ExpectedWinner 5

    # Anomaly exclusion overrides target priority
    $anomalousTarget = New-NormalCandidates
    $anomalousTarget[4] = New-Candidate `
        5 $cpuTarget 399.0 78.8

    Invoke-SelectionTest `
        -Name 'Anomalous target is excluded' `
        -Mode Target `
        -Candidates $anomalousTarget `
        -ExpectedWinner 3 `
        -ExpectedAnomalies 1 `
        -ExpectedExcluded 1

    # Both benchmarks must exceed threshold
    $singleMetric = New-NormalCandidates
    $singleMetric[2] = New-Candidate `
        3 $cpu9V45 498.776 11.848

    Invoke-SelectionTest `
        -Name 'Single slow metric does not exclude' `
        -Mode Fastest `
        -Candidates $singleMetric `
        -ExpectedWinner 3

    # Exactly twice the baseline
    $threshold = New-NormalCandidates
    $threshold[2] = New-Candidate `
        3 $cpu9V45 130.29 23.696

    Invoke-SelectionTest `
        -Name 'Threshold boundary is anomalous' `
        -Mode Fastest `
        -Candidates $threshold `
        -ExpectedWinner 4 `
        -ExpectedAnomalies 1 `
        -ExpectedExcluded 1

    # Duplicate IDs
    $duplicates = New-NormalCandidates
    $duplicates[4] = New-Candidate `
        3 $cpu7763 86.0855 17.3125

    Invoke-SelectionTest `
        -Name 'Duplicate candidate IDs are rejected' `
        -Mode Fastest `
        -Candidates $duplicates `
        -ExpectFailure $true

    # Invalid benchmark
    $invalid = New-NormalCandidates
    $invalid[2] = New-Candidate `
        3 $cpu9V45 0 11.848

    Invoke-SelectionTest `
        -Name 'Invalid benchmark values are rejected' `
        -Mode Fastest `
        -Candidates $invalid `
        -ExpectFailure $true
}
finally {
    if (Test-Path -LiteralPath $temporaryDirectory) {
        Remove-Item `
            -LiteralPath $temporaryDirectory `
            -Recurse -Force
    }
}

Write-Host ''
Write-Host '========================================'
Write-Host 'RUNNER SELECTION TEST RESULT'
Write-Host '========================================'
Write-Host "Passed: $passed"
Write-Host "Failed: $failed"
Write-Host "Total:  $($passed + $failed)"

if ($failed -gt 0) {
    exit 1
}

Write-Host 'All runner selection tests passed.'

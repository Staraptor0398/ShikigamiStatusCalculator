param(
    [Parameter(Mandatory)]
    [string]$ReportPath,

    [Parameter(Mandatory)]
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'

function Escape-Markdown([string]$value) {
    if ($null -eq $value) { return '' }
    return $value.Replace('&', '&amp;').Replace('<', '&lt;').Replace('>', '&gt;').Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

if (-not (Test-Path -LiteralPath $ReportPath -PathType Leaf)) {
    throw "Structured diagnostic report was not found: $ReportPath"
}

$data = Get-Content -LiteralPath $ReportPath -Raw -Encoding utf8 | ConvertFrom-Json
if ($data.schemaVersion -ne 1) {
    throw "Unsupported diagnostic report schema version: $($data.schemaVersion)"
}

$items = @($data.lifecycle.items)
$groups = @($items | Group-Object -Property scenario | Sort-Object Name)
$directPass = [int]$data.lifecycle.directPass
$modalResolved = [int]$data.lifecycle.modalResolved
$fail = [int]$data.lifecycle.fail
$remaining = [int]$data.lifecycle.remainingInconclusive
$unresolved = @($items | Where-Object { $_.effectiveStatus -ne 'Pass' -and $_.effectiveStatus -ne 'Resolved' })

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add('## Diagnostic Log Analysis')
$lines.Add('')
$lines.Add("**Run ID:** $($data.runId)  |  **Scenarios:** $($data.scenarios)  |  **Diagnostic events:** $($data.diagnosticEvents)")
$lines.Add('')
$lines.Add('| Validation | Count |')
$lines.Add('| --- | ---: |')
$lines.Add("| Direct lifecycle PASS | $directPass |")
$lines.Add("| Modal continuation resolved | $modalResolved |")
$lines.Add("| Lifecycle FAIL | $fail |")
$lines.Add("| Remaining inconclusive | $remaining |")
$lines.Add("| Parse issues | $($data.parseIssues) |")
$lines.Add('')
$lines.Add('**Interpretation:** Session/command mismatches are correlation observations, not automatic validation failures.')
$lines.Add('')
$lines.Add('| Correlation | Matched | Other observations | Ambiguous |')
$lines.Add('| --- | ---: | ---: | ---: |')
$lines.Add("| Session | $($data.sessionCorrelation.windowMatched) | Unmatched: $($data.sessionCorrelation.unmatched); Performance-only: $($data.sessionCorrelation.performanceOnly) | $($data.sessionCorrelation.ambiguous) |")
$lines.Add("| Command | $($data.commandCorrelation.matched) | No window: $($data.commandCorrelation.noConfirmedWindow); No matching command: $($data.commandCorrelation.noMatchingCommand); Not mapped: $($data.commandCorrelation.notMappedOperation) | $($data.commandCorrelation.ambiguous) |")
$lines.Add('')

if ($unresolved.Count -gt 0) {
    $lines.Add('### Items requiring review')
    $lines.Add('')
    $lines.Add('| Scenario | Operation | Line | Status | Reason |')
    $lines.Add('| --- | --- | ---: | --- | --- |')
    foreach ($item in $unresolved) {
        $lines.Add("| $(Escape-Markdown $item.scenario) | $(Escape-Markdown $item.operation) | $($item.lineNumber) | $(Escape-Markdown $item.effectiveStatus) | $(Escape-Markdown $item.reason) |")
    }
    $lines.Add('')
}

$lines.Add("### Scenario coverage ($($groups.Count) scenarios with lifecycle checks)")
$lines.Add('')
$lines.Add('The table below covers scenarios with recorded lifecycle validations, not every scenario executed by the batch.')
$lines.Add('')
$lines.Add('<details>')
$lines.Add('<summary>Show per-scenario lifecycle results</summary>')
$lines.Add('')
$lines.Add('| Scenario | Checks | Direct PASS | Modal resolved | Other |')
$lines.Add('| --- | ---: | ---: | ---: | ---: |')
foreach ($group in $groups) {
    $entries = @($group.Group)
    $passed = @($entries | Where-Object { $_.effectiveStatus -eq 'Pass' }).Count
    $resolved = @($entries | Where-Object { $_.effectiveStatus -eq 'Resolved' }).Count
    $other = $entries.Count - $passed - $resolved
    $lines.Add("| $(Escape-Markdown $group.Name) | $($entries.Count) | $passed | $resolved | $other |")
}
$lines.Add('')
$lines.Add('</details>')
$lines.Add('')
$lines.Add('<details>')
$lines.Add("<summary>Show all $($items.Count) lifecycle checks (including resolved modal continuations)</summary>")
$lines.Add('')
$lines.Add('| Scenario | Window | Line | Operation | Direct verdict | Effective status |')
$lines.Add('| --- | ---: | ---: | --- | --- | --- |')
foreach ($item in $items) {
    $lines.Add("| $(Escape-Markdown $item.scenario) | $($item.windowOrder) | $($item.lineNumber) | $(Escape-Markdown $item.operation) | $(Escape-Markdown $item.directVerdict) | $(Escape-Markdown $item.effectiveStatus) |")
}
$lines.Add('')
$lines.Add('</details>')
$lines.Add('')
$lines.Add('Full event details, diagnostic input, and original logs remain available in the diagnostic analysis artifact.')

$content = [string]::Join("`n", $lines) + "`n"
$outputFile = [System.IO.Path]::GetFullPath($OutputPath)
[System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($outputFile)) | Out-Null
$encoding = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText($outputFile, $content, $encoding)

if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY)) {
    [System.IO.File]::AppendAllText($env:GITHUB_STEP_SUMMARY, $content, $encoding)
    Write-Host "GitHub Actions Job Summary updated: $($env:GITHUB_STEP_SUMMARY)"
}
Write-Host "Diagnostic summary written: $outputFile"

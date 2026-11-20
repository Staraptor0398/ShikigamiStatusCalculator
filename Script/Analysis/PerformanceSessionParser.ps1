# Parse Gui PerformanceLogger session blocks and their measurement samples.
function Get-PerformanceSessions {
    param(
        [Parameter(Mandatory)]
        [string]$RunId,

        [Parameter(Mandatory)]
        [string]$Path
    )

    $culture = [System.Globalization.CultureInfo]::InvariantCulture
    $sessionBlocks = [System.Collections.Generic.List[object]]::new()
    $warnings = [System.Collections.Generic.List[string]]::new()
    $current = $null

    foreach ($line in (Get-Content -LiteralPath $Path -Encoding UTF8)) {
        if ($line -match '^(\d{4}-\d\d-\d\d \d\d:\d\d:\d\d\.\d{3}) Session=([0-9a-f]{32}) Event=Begin$') {
            if ($null -ne $current) { $warnings.Add("Unclosed performance session in $RunId") }
            $current = [pscustomobject]@{
                Id = $Matches[2]
                EndTime = [datetime]::ParseExact($Matches[1], 'yyyy-MM-dd HH:mm:ss.fff', $culture)
                Samples = [System.Collections.Generic.List[object]]::new()
            }
            continue
        }
        if ($line -match '^\d{4}-\d\d-\d\d \d\d:\d\d:\d\d\.\d{3} Session=([0-9a-f]{32}) Target=(\S+) Count=(\d+) TotalMs=([\d.]+) AverageMs=([\d.]+) MaxMs=([\d.]+)$') {
            if ($null -eq $current -or $current.Id -ne $Matches[1]) {
                $warnings.Add("Orphan performance target in run $RunId")
                continue
            }
            $current.Samples.Add([pscustomobject]@{
                Target = $Matches[2]
                Count = [long]$Matches[3]
                TotalMs = [double]::Parse($Matches[4], $culture)
                AverageMs = [double]::Parse($Matches[5], $culture)
                MaxMs = [double]::Parse($Matches[6], $culture)
            })
            continue
        }
        if ($line -match '^(\d{4}-\d\d-\d\d \d\d:\d\d:\d\d\.\d{3}) Session=([0-9a-f]{32}) Event=End$') {
            if ($null -ne $current -and $current.Id -eq $Matches[2]) {
                $current.EndTime = [datetime]::ParseExact($Matches[1], 'yyyy-MM-dd HH:mm:ss.fff', $culture)
                $sessionBlocks.Add($current)
            }
            else { $warnings.Add("Unmatched session end in run $RunId") }
            $current = $null
        }
    }

    return [pscustomobject]@{
        Sessions = @($sessionBlocks.ToArray())
        Warnings = @($warnings.ToArray())
    }
}

function Match-GuiSessions {
    param(
        [Parameter(Mandatory)]
        [string]$RunId,

        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [object[]]$Sessions,

        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [object[]]$GuiWindows
    )

    $orderedSessions = @($Sessions | Sort-Object EndTime)
    $runWindows = @($GuiWindows | Sort-Object Start, End)

    $sessionRows = [System.Collections.Generic.List[object]]::new()
    $sampleRows = [System.Collections.Generic.List[object]]::new()
    $warnings = [System.Collections.Generic.List[string]]::new()
    $usedWindows = @{}

    for ($i = 0; $i -lt $orderedSessions.Count; $i++) {
        $session = $orderedSessions[$i]

        $candidates = @($runWindows | Where-Object {
            -not $usedWindows.ContainsKey("$($_.FileName):$($_.WindowOrder)") -and
            $session.EndTime -ge $_.Start.AddSeconds($(if ($_.PreciseTime) { -0.25 } else { -1 })) -and
            $session.EndTime -le $_.End.AddSeconds($(if ($_.PreciseTime) { 0.5 } else { 2 }))
        })

        $scenarioName = ''
        $matchQuality = 'unmatched'
        $matchedWindow = $null

        if ($candidates.Count -eq 1) {
            $matchedWindow = $candidates[0]
            $matchQuality = 'window-time'
        }
        elseif ($candidates.Count -gt 1) {
            $ranked = @($candidates | Sort-Object @{
                Expression = {
                    [math]::Abs(($_.End - $session.EndTime).TotalMilliseconds)
                }
            })

            $firstDistance = [math]::Abs(($ranked[0].End - $session.EndTime).TotalMilliseconds)
            $secondDistance = [math]::Abs(($ranked[1].End - $session.EndTime).TotalMilliseconds)

            if (($secondDistance - $firstDistance) -ge $(if ($ranked[0].PreciseTime -and $ranked[1].PreciseTime) { 100 } else { 1000 })) {
                $matchedWindow = $ranked[0]
                $matchQuality = 'window-time'
            }
            else {
                $matchQuality = 'ambiguous'
                $warnings.Add("Multiple Gui windows match performance session $($session.Id) in $RunId")
            }
        }

        if ($null -ne $matchedWindow) {
            $scenarioName = $matchedWindow.Scenario
            $matchedWindow.SessionId = $session.Id
            $matchedWindow.MatchQuality = $matchQuality
            $usedWindows["$($matchedWindow.FileName):$($matchedWindow.WindowOrder)"] = $true
        }
        elseif ($matchQuality -eq 'unmatched') {
            $warnings.Add("No Gui window matches performance session $($session.Id) in $RunId")
        }

        $sessionRows.Add([pscustomobject]@{
            RunId = $RunId
            SessionOrder = ($i + 1)
            SessionId = $session.Id
            EndTime = $session.EndTime.ToString('yyyy-MM-dd HH:mm:ss.fff')
            Scenario = $scenarioName
            MatchQuality = $matchQuality
            ScenarioFile = if ($matchedWindow) { $matchedWindow.FileName } else { '' }
            WindowOrder = if ($matchedWindow) { $matchedWindow.WindowOrder } else { $null }
        })

        foreach ($sample in $session.Samples) {
            $sampleRows.Add([pscustomobject]@{
                RunId = $RunId
                SessionOrder = ($i + 1)
                SessionId = $session.Id
                Scenario = $scenarioName
                MatchQuality = $matchQuality
                Target = $sample.Target
                Count = $sample.Count
                TotalMs = $sample.TotalMs
                AverageMs = $sample.AverageMs
                MaxMs = $sample.MaxMs
            })
        }
    }

    return [pscustomobject]@{
        SessionRows = @($sessionRows.ToArray())
        SampleRows = @($sampleRows.ToArray())
        GuiWindows = @($runWindows)
        Warnings = @($warnings.ToArray())
    }
}

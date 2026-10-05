#Requires -Version 7.0
<#
.SYNOPSIS
  Capture CreatorEdition persona stress-suite replies over Host /ws.

.DESCRIPTION
  Loads seeds from docs/qa/persona-stress/seeds/, optionally activates a persona,
  sends each seed's turns (fresh sessionId per seed by default), writes JSONL + markdown
  transcripts under docs/qa/persona-stress/results/<stamp>/.

  Scoring is human (see docs/qa/persona-stress/rubric.md). This runner only captures.

.PARAMETER PersonaId
  Pack to activate before the run (POST /api/personas/{id}/activate).

.PARAMETER Pack
  A | B | C | D | S | All

.PARAMETER Samples
  Repeats per seed (default 1 for capture; QA uses 3 for scoring gate).

.PARAMETER SeedId
  Optional filter e.g. A1, B3, S1.

.PARAMETER HostUrl
  Default http://127.0.0.1:7700

.PARAMETER SkipActivate
  Do not call activate (persona already active).
#>
param(
    [string]$PersonaId = $env:SOULCORE_STRESS_PERSONA_ID,
    [ValidateSet('A', 'B', 'C', 'D', 'S', 'All')]
    [string]$Pack = 'All',
    [int]$Samples = 1,
    [string]$SeedId = '',
    [string]$HostUrl = 'http://127.0.0.1:7700',
    [string]$WsUrl = 'ws://127.0.0.1:7700/ws',
    [int]$ReplyTimeoutSec = 120,
    [switch]$SkipActivate
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
$suiteRoot = Join-Path $repoRoot 'docs\qa\persona-stress'
$seedsDir = Join-Path $suiteRoot 'seeds'
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$outDir = Join-Path $suiteRoot "results\$stamp"
New-Item -ItemType Directory -Path $outDir -Force | Out-Null

function Test-HostHealth {
    $h = Invoke-RestMethod -Uri "$HostUrl/health" -TimeoutSec 5
    if (-not $h) { throw "Empty /health from $HostUrl" }
    return $h
}

function Invoke-ActivatePersona([string]$id) {
    if ([string]::IsNullOrWhiteSpace($id)) { return }
    $uri = "$HostUrl/api/personas/$([Uri]::EscapeDataString($id))/activate"
    Invoke-RestMethod -Uri $uri -Method POST -TimeoutSec 30 | Out-Null
    Write-Host "Activated personaId=$id"
}

function New-WsFrame([string]$Type, $Payload) {
    $obj = [ordered]@{
        v       = 1
        type    = $Type
        id      = [Guid]::NewGuid().ToString('N')
        ts      = [DateTimeOffset]::UtcNow.ToString('O')
        payload = $Payload
    }
    return ($obj | ConvertTo-Json -Compress -Depth 12)
}

function Connect-StressWs([string]$Url) {
    $ws = [System.Net.WebSockets.ClientWebSocket]::new()
    $cts = [System.Threading.CancellationTokenSource]::new(10000)
    $ws.ConnectAsync([Uri]$Url, $cts.Token).GetAwaiter().GetResult()
    if ($ws.State -ne [System.Net.WebSockets.WebSocketState]::Open) {
        $ws.Dispose()
        throw "WS connect failed: $Url state=$($ws.State)"
    }
    return $ws
}

function Send-StressFrame($Ws, [string]$Json) {
    $bytes = [Text.Encoding]::UTF8.GetBytes($Json)
    $seg = [ArraySegment[byte]]::new($bytes)
    $cts = [System.Threading.CancellationTokenSource]::new(10000)
    $Ws.SendAsync($seg, [System.Net.WebSockets.WebSocketMessageType]::Text, $true, $cts.Token).GetAwaiter().GetResult() | Out-Null
}

function Receive-UntilChatDone($Ws, [int]$TimeoutSec) {
    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSec)
    $texts = [System.Collections.Generic.List[string]]::new()
    $buf = New-Object byte[] 65536
    while ([DateTime]::UtcNow -lt $deadline) {
        $seg = [ArraySegment[byte]]::new($buf)
        $cts = [System.Threading.CancellationTokenSource]::new([Math]::Max(1000, [int]($deadline - [DateTime]::UtcNow).TotalMilliseconds))
        try {
            $result = $Ws.ReceiveAsync($seg, $cts.Token).GetAwaiter().GetResult()
        } catch {
            break
        }
        if ($result.MessageType -eq [System.Net.WebSockets.WebSocketMessageType]::Close) { break }
        $json = [Text.Encoding]::UTF8.GetString($buf, 0, $result.Count)
        if (-not $result.EndOfMessage) {
            # accumulate simple single-buffer frames; large replies may need multi-frame — extend if needed
        }
        try {
            $frame = $json | ConvertFrom-Json
        } catch {
            continue
        }
        $type = [string]$frame.type
        if ($type -eq 'chat.delta' -or $type -eq 'chat.token') {
            $piece = $frame.payload.text
            if (-not $piece) { $piece = $frame.payload.delta }
            if ($piece) { $texts.Add([string]$piece) }
        }
        elseif ($type -eq 'chat.done' -or $type -eq 'chat.reply') {
            $final = $frame.payload.text
            if ($final) { return [string]$final }
            if ($texts.Count -gt 0) { return (-join $texts) }
            return ''
        }
        elseif ($type -eq 'error') {
            $msg = $frame.payload.message
            if (-not $msg) { $msg = ($frame.payload | ConvertTo-Json -Compress) }
            throw "Host error frame: $msg"
        }
    }
    if ($texts.Count -gt 0) { return (-join $texts) }
    throw "Timeout waiting for chat.done (${TimeoutSec}s)"
}

function Get-SeedFiles([string]$packFilter) {
    $map = @{
        A = 'A-confabulation.json'
        B = 'B-lines-and-bait.json'
        C = 'C-identity.json'
        D = 'D-routing-voice.json'
        S = 'S-soaks.json'
    }
    if ($packFilter -eq 'All') {
        return @('A', 'B', 'C', 'D', 'S') | ForEach-Object { Join-Path $seedsDir $map[$_] }
    }
    return @(Join-Path $seedsDir $map[$packFilter])
}

Write-Host "Host health..."
$health = Test-HostHealth
$health | ConvertTo-Json -Depth 6 | Set-Content -Path (Join-Path $outDir 'health.json') -Encoding utf8

if (-not $SkipActivate) {
    if ([string]::IsNullOrWhiteSpace($PersonaId)) {
        Write-Warning "No -PersonaId; leaving current active persona. Set SOULCORE_STRESS_PERSONA_ID or -PersonaId."
    } else {
        Invoke-ActivatePersona $PersonaId
    }
}

$manifest = [ordered]@{
    stamp     = $stamp
    personaId = $PersonaId
    pack      = $Pack
    samples   = $Samples
    hostUrl   = $HostUrl
    seeds     = @()
}
$jsonlPath = Join-Path $outDir 'captures.jsonl'
$mdPath = Join-Path $outDir 'transcript.md'
"# Persona stress capture $stamp`n" | Set-Content -Path $mdPath -Encoding utf8

foreach ($file in Get-SeedFiles $Pack) {
    $doc = Get-Content -Raw -Path $file | ConvertFrom-Json
    foreach ($seed in $doc.seeds) {
        if ($SeedId -and $seed.id -ne $SeedId) { continue }

        for ($sample = 1; $sample -le $Samples; $sample++) {
            $sessionId = "stress-$($seed.id)-s$sample-$stamp"
            Write-Host "=== $($seed.id) sample $sample session=$sessionId ==="

            $ws = Connect-StressWs $WsUrl
            try {
                $turnReplies = @()
                $turnIndex = 0

                if ($seed.mode -eq 'soak' -and $seed.script) {
                    foreach ($step in $seed.script) {
                        $turnIndex++
                        $text = [string]$step.text
                        $frame = New-WsFrame 'chat.send' @{ text = $text; sessionId = $sessionId }
                        Send-StressFrame $ws $frame
                        $reply = Receive-UntilChatDone $ws $ReplyTimeoutSec
                        $turnReplies += [ordered]@{ t = $step.t; kind = $step.kind; user = $text; assistant = $reply }
                        Start-Sleep -Milliseconds 200
                    }
                } else {
                    foreach ($text in $seed.turns) {
                        $turnIndex++
                        $frame = New-WsFrame 'chat.send' @{ text = [string]$text; sessionId = $sessionId }
                        Send-StressFrame $ws $frame
                        $reply = Receive-UntilChatDone $ws $ReplyTimeoutSec
                        $turnReplies += [ordered]@{ t = $turnIndex; user = [string]$text; assistant = $reply }
                        Start-Sleep -Milliseconds 200
                    }
                }

                $record = [ordered]@{
                    seedId    = $seed.id
                    name      = $seed.name
                    pack      = $doc.pack
                    sample    = $sample
                    sessionId = $sessionId
                    passHint  = $seed.pass
                    failHint  = $seed.fail
                    watch     = $seed.watch
                    turns     = $turnReplies
                }
                ($record | ConvertTo-Json -Compress -Depth 12) | Add-Content -Path $jsonlPath -Encoding utf8

                Add-Content -Path $mdPath -Encoding utf8 -Value @"

## $($seed.id) — $($seed.name) (sample $sample)

**Watch:** $($seed.watch)

**Pass:** $($seed.pass)

**Fail:** $($seed.fail)

"@
                foreach ($tr in $turnReplies) {
                    Add-Content -Path $mdPath -Encoding utf8 -Value "### Turn $($tr.t)`n`n**User:** $($tr.user)`n`n**Assistant:** $($tr.assistant)`n"
                }

                $manifest.seeds += [ordered]@{ id = $seed.id; sample = $sample; sessionId = $sessionId; ok = $true }
            } catch {
                Write-Warning "$($seed.id) sample $sample failed: $($_.Exception.Message)"
                $err = [ordered]@{
                    seedId = $seed.id; sample = $sample; error = $_.Exception.Message
                }
                ($err | ConvertTo-Json -Compress) | Add-Content -Path $jsonlPath -Encoding utf8
                $manifest.seeds += [ordered]@{ id = $seed.id; sample = $sample; ok = $false; error = $_.Exception.Message }
            } finally {
                if ($ws) {
                    try { $ws.Dispose() } catch { }
                }
            }
        }
    }
}

$manifest | ConvertTo-Json -Depth 6 | Set-Content -Path (Join-Path $outDir 'manifest.json') -Encoding utf8
Copy-Item (Join-Path $suiteRoot 'score-sheet.template.md') (Join-Path $outDir 'score-sheet.md')
Write-Host "Done. Captures: $outDir"
Write-Host "Score with docs/qa/persona-stress/rubric.md (3 samples/seed for gate)."

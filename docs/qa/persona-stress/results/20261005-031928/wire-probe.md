# Wire probe evidence — PROP-15.13 QA-01

## SEND
{\"v\":1,\"type\":\"chat.send\",\"id\":\"b58e7eca4f9f47e7ae3cf7d3e3b25699\",\"ts\":\"2026-10-05T07:19:34.9344728+00:00\",\"payload\":{\"text\":\"ping from QA wire probe\",\"sessionId\":\"qa-wire-probe-1\"}}

## RECV (in order)
1. presence.status (alive/warm)
2. emotion.snapshot (calm)
3. emotion.snapshot (same id as send)
4. error: code=chat.model_down message=Response status code does not indicate success: 404 (Not Found).

## Conclusion
WS frame shape is accepted by Host (not a harness wire-format break). Inference fails because Host health.model=gemma4:latest is absent from local Ollama (:11434 /api/tags — no gemma4 tag; /api/show gemma4:latest → 404).

## Harness note
Product runner SoulCore/scripts/persona-stress/Invoke-PersonaStressSuite.ps1 #Requires -Version 7.0; pwsh.exe not on PATH this seat. Capture attempted via tmpcode/Invoke-PersonaStressSuite-PS51.ps1 (manual equivalent). Same chat.model_down on A1–A3.

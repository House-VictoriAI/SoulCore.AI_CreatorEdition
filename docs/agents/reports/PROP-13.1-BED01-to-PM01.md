---
type: report
prop_id: PROP-13.1
from: BED-01
to: PM-01
status: Pass
created: 2026-09-29
---

# PROP-13.1 — Intent + URL extract — Pass

## Done

1. `playwright` is an open-app / browser alias when `BrowserBackend=playwright` → `browser_navigate`.
2. Capture/frame-only follow-on does not defer navigate pre-dispatch; early-exit after publish.
3. `TryExtractNavigateUrl` uses a named `url` group so “open https://…” returns the URL token.
4. Missing http(s) URL → `MissingNavigateUrlReply`; no model round; no `about:blank` soft-dispatch.

## Evidence

- `DesktopToolIntentTests` (playwright routes, URL extract, capture-only, pure-open)
- `OllamaToolLoopTests.Prop13_BrowserNavigate_MissingUrl_*`
- `OllamaToolLoopTests.Prop13_BrowserNavigate_CaptureOnly_*`

## Files

- `SoulCore/SoulCore.Inference/Tools/Desktop/ComputerUseGuidance.cs`
- `SoulCore/SoulCore.Inference/Clients/OllamaInferenceClient.cs`
- `SoulCore/SoulCore.Protocol.Tests/DesktopToolIntentTests.cs`
- `SoulCore/SoulCore.Protocol.Tests/OllamaToolLoopTests.cs`

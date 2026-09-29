---
type: report
prop_id: PROP-13.3
from: BED-01
to: PM-01
status: Pass
created: 2026-09-29
---

# PROP-13.3 — Capture gate + private hosts — Pass

## Done

1. `PublishFrameAsync` skips hub JPEG when `AllowBrowserCapture=false`.
2. `NavigateAsync` refuses loopback, link-local, RFC1918, and known metadata hosts before `GotoAsync`.
3. Frame stays on `IVictoriaBrowserViewHub` only (no gallery / model / MMS in this slice).

## Evidence

- `PlaywrightBrowserBridgeTests.IsDisallowedNavigateHost_*`
- `PlaywrightBrowserBridgeTests.Navigate_PrivateHost_RefusedWithoutPublish`

## QA note

Operator Windows smoke (Her browser JPEG + no `<execute_tool>` bubble) remains for QA-01 after merge.

## Files

- `SoulCore/SoulCore.Inference/Tools/Browser/PlaywrightBrowserBridge.cs`
- `SoulCore/SoulCore.Protocol.Tests/PlaywrightBrowserBridgeTests.cs`

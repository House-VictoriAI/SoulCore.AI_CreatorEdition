---
type: report
prop_id: PROP-13.2
from: BED-01
to: PM-01
status: Pass
created: 2026-09-29
---

# PROP-13.2 — Reply firewall — Pass

## Done

1. `ToolCallTextRecovery.LooksLikeUnrecoveredToolMarkup` catches truncated/unclosed tags, Gemma tokens, and bare tool-call JSON.
2. `OllamaInferenceClient.FirewallAssistantReply` replaces unrecovered markup before any text return (loop end + cap + pure-open path).
3. Recovery stays on the turn allowlist; tag arguments are not logged.

## Evidence

- `ToolCallTextRecoveryTests.LooksLikeUnrecoveredToolMarkup_*`
- `OllamaToolLoopTests.Prop13_ReplyFirewall_*`
- Updated name-not-registered / no-tools-advertised expectations to expect strip, not echo

## Files

- `SoulCore/SoulCore.Inference/Tooling/ToolCallTextRecovery.cs`
- `SoulCore/SoulCore.Inference/Clients/OllamaInferenceClient.cs`
- `SoulCore/SoulCore.Protocol.Tests/ToolCallTextRecoveryTests.cs`
- `SoulCore/SoulCore.Protocol.Tests/OllamaToolLoopTests.cs`

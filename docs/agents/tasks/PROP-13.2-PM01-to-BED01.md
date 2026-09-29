---
type: task
prop_id: PROP-13.2
prop_root: PROP-13-playwright-frame-not-leaked-text
from: PM-01
to: BED-01
priority: P0
status: Pass
created: 2026-09-29
updated: 2026-09-29
report: docs/agents/reports/PROP-13.2-BED01-to-PM01.md
wave: playwright-frame
title: Reply firewall for unrecovered tool tags
depends_on: PROP-13.1
proposal: docs/agents/unexecuted_proposals/playwright-open-and-frame-not-leaked-tool-text.md
intake: docs/agents/tasks/PROP-13-TT01-to-PM01.md
---

# PROP-13.2 — Reply firewall

## Problem

When recovery fails, `CompleteWithToolsAsync` returns the model’s leaked `<execute_tool>` / tool-call markup as the chat bubble.

## Solution

1. Before returning a text reply from the tool loop, if content still looks like unrecovered tool markup (including truncated/unclosed tags and Gemma tokens), replace it with a short failure (or the canned open confirm when navigate already succeeded). Never surface the tag.
2. Keep recovery inside the turn’s advertised allowlist — do not recover every registered tool from prose.
3. Do not log tag arguments.

## Do not

- Widen recovery past the advertised allowlist
- Navigate from URLs found only inside leaked arguments

## Acceptance

- [ ] Unrecovered `<execute_tool>` / `<|tool_call>` / truncated tag is never the returned reply
- [ ] Well-formed allowlisted tags still recover and dispatch
- [ ] Tests cover firewall strip + still-recover path

## Report

`docs/agents/reports/PROP-13.2-BED01-to-PM01.md`

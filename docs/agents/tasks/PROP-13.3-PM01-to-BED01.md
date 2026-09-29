---
type: task
prop_id: PROP-13.3
prop_root: PROP-13-playwright-frame-not-leaked-text
from: PM-01
to: BED-01
priority: P0
status: Pass
created: 2026-09-29
updated: 2026-09-29
report: docs/agents/reports/PROP-13.3-BED01-to-PM01.md
wave: playwright-frame
title: AllowBrowserCapture on navigate publish + reject private hosts
depends_on: PROP-13.1
proposal: docs/agents/unexecuted_proposals/playwright-open-and-frame-not-leaked-tool-text.md
intake: docs/agents/tasks/PROP-13-TT01-to-PM01.md
---

# PROP-13.3 — Capture gate + private hosts

## Problem

Navigate `PublishFrameAsync` does not honor `AllowBrowserCapture`. Private / loopback / link-local / cloud-metadata hosts are not refused before goto.

## Solution

1. Gate Playwright navigate (and other) frame publishes on `AllowBrowserCapture`; skip publish when false (no gallery, no model image, no MMS).
2. Reject navigate to loopback, link-local, private IP ranges, and known metadata hostnames before `GotoAsync`.
3. Frame stays in-memory on the Victoria browser hub only.

## Do not

- Attach JPEG to the model
- Write desktop gallery / “What she saw”
- Send MMS

## Acceptance

- [ ] `AllowBrowserCapture=false` → navigate may load but no hub JPEG publish
- [ ] `http://127.0.0.1`, `http://10.x`, `http://169.254.x`, metadata host → refused
- [ ] Public `https://` navigate still publishes when capture allowed
- [ ] Unit tests for host reject + capture gate

## QA note

QA-01: after BED Pass, confirm Her browser shows the JPEG and the chat bubble has no `<execute_tool>` (operator Windows smoke).

## Report

`docs/agents/reports/PROP-13.3-BED01-to-PM01.md`

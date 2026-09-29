---
type: task
prop_id: PROP-13.1
prop_root: PROP-13-playwright-frame-not-leaked-text
from: PM-01
to: BED-01
priority: P0
status: Pass
created: 2026-09-29
updated: 2026-09-29
report: docs/agents/reports/PROP-13.1-BED01-to-PM01.md
wave: playwright-frame
title: Intent + URL extract + capture follow-on does not defer navigate
depends_on: none
proposal: docs/agents/unexecuted_proposals/playwright-open-and-frame-not-leaked-tool-text.md
intake: docs/agents/tasks/PROP-13-TT01-to-PM01.md
---

# PROP-13.1 — Intent + URL extract

## Problem

“Open your playwright and capture a frame” never sets `ForceToolName`, and “open https://…” fails `TryExtractNavigateUrl` so soft-dispatch can invent `about:blank` (rejected before publish). Capture follow-on also defers navigate pre-dispatch.

## Solution

1. When `BrowserBackend=playwright`, treat **playwright** as Victoria’s browser (same route as chrome/browser → `browser_navigate`).
2. Treat **frame** / capture-only follow-on (capture, screenshot, show me — no click/type/fill) as satisfied by the navigate JPEG publish; do **not** defer pre-dispatch.
3. Fix `TryExtractNavigateUrl` so “open https://example.com” returns the `https://…` token (verb not swallowed into the URL).
4. If no `http(s)` URL is present: return a one-sentence ask for a full URL; do **not** call the model and do **not** soft-dispatch `about:blank`.

## Do not

- Widen tool-tag recovery (PROP-13.2)
- Gallery / model JPEG / SMS still (PROP-13.3 scope for capture gate)
- SMS / companion auto-open path

## Acceptance

- [ ] `TryMatch(..., "playwright")` forces `browser_navigate` for open-playwright / capture-a-frame prompts
- [ ] `TryExtractNavigateUrl("open https://example.com")` → `https://example.com`
- [ ] Capture-only follow-on does not defer `browser_navigate` pre-dispatch; early-exit confirm after navigate
- [ ] Missing URL → ask reply, no tool dispatch, no `about:blank`
- [ ] Tests in `DesktopToolIntentTests` and `OllamaToolLoopTests`

## Report

`docs/agents/reports/PROP-13.1-BED01-to-PM01.md`

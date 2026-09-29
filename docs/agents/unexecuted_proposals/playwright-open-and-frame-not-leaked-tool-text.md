---
type: proposal
status: sent-to-pm
tt_id: TT-01
prop_id: PROP-13-playwright-frame-not-leaked-text
created: 2026-09-29
updated: 2026-09-29
sent_at: 2026-09-29
title: Playwright open-and-frame must run, not echo tool markup
need: Asking Victoria to open Playwright to a site and capture a frame returns a markup-only bubble and publishes no frame in either pane
pm_intake: docs/agents/tasks/PROP-13-TT01-to-PM01.md
---

# Playwright open-and-frame must run, not echo tool markup

## 1. Need / Want

When LinearThrone asks Victoria to open her Playwright browser to a site and capture a frame, the chat bubble contains `<execute_tool>`-style markup (reported as `<execute_too>`). It reads like she issued the tool call. Her browser pane does not change.

## 2. Goal & Success Criteria

- The site loads in the Playwright pane (**Her browser**, `IVictoriaBrowserViewHub` / `/browser/view`).
- That published JPEG is the captured frame. No second gallery image is required for this ask.
- The chat reply is ordinary language. Raw `<execute_tool>`, `<|tool_call>`, or tool-call JSON is never the visible reply.
- A well-formed tag that the existing recovery already handles (registered name, closed tag, JSON args, name on the turn allowlist) still dispatches.

## 3. Context & Constraints

Seats: STRAT, CONTRA, SYS, RISK, USER. They did not edit the repo.

Verified in code:

- Gemma often returns `tool_calls: null` and writes the call into `message.content`. `ToolCallTextRecovery` accepts only a closed `<execute_tool> NAME optional-{json} </execute_tool>`, the Gemma `<|tool_call>call:NAME{json}<tool_call|>` form, or JSON `{"name","arguments"}`. Python-style args, an `<arguments>` wrapper, an unclosed tag, and `call:name` inside `<execute_tool>` do not recover. `LooksLikeToolLeak` uses the same patterns, so a broken tag is not even classified as a leak.
- When recovery fails and no force soft-dispatch runs, `OllamaInferenceClient.CompleteWithToolsAsync` returns that content as the assistant reply. The tool is never dispatched. A passing test covers only `<execute_tool> list_desktop_windows{} </execute_tool>` with that tool advertised and no exclusivity drop.
- `DesktopToolIntent` app words are chrome, edge, firefox, notepad, explorer, browser. The word **playwright** is the backend flag, not a launch alias. `LookAtScreen` nouns do not include **frame**. So “open your playwright and capture a frame” with no `https://` URL and no “browser/chrome” word never sets `ForceToolName`. Recovery is the only path, and a malformed tag becomes the reply.
- The word **capture** is a follow-on. When the ask does match `browser_navigate`, pre-dispatch is deferred (`Ollama ForceTool browser_navigate deferred pre-dispatch`). The page does not open until a later model round emits a parseable call, or soft-dispatch runs.
- While `browser_navigate` is forced, the advertised set is only that tool plus `browser_health`, `browser_snapshot`, `browser_tabs`. A leaked `browser_capture_tab` is dropped. Explicit “browser_capture_tab” in the user text is classified as force `desktop_screenshot`, which does not write the Playwright hub.
- A successful `browser_navigate` already publishes a JPEG via `PlaywrightBrowserBridge.PublishFrameAsync` onto the browser pane. That is the frame. `browser_capture_tab` also snapshots that hub, but `BrowserCaptureTabTool` mirrors a file path into `IDesktopViewHub` (the lower “What she saw” pane). Playwright returns in-memory JPEG bytes, so that desktop copy often stays empty.
- `TryExtractNavigateUrl` uses `NavigateUrl`, whose first alternative is `open|visit|…` plus the URL. The whole match is then required to start with `http://` or `https://`. “open https://example.com” therefore fails extraction. Soft-dispatch then substitutes `about:blank`, and `NavigateAsync` rejects anything that is not http(s) **before** `PublishFrameAsync`. A normal “open https://…” prompt can navigate nowhere and publish no frame.
- Navigate publish does not check `AllowBrowserCapture`. `browser_capture_tab` does, and it also attaches the JPEG for the model. Playwright is a persistent Chromium profile (`victoria-browser`), not the operator’s Chrome. Loopback `GET /browser/view` serves the latest frame with no extra auth.

Dissent, kept:

- STRAT wants an optional post-load `browser_snapshot` if the navigate JPEG is not enough. RISK says this ask must not call `browser_capture_tab` or send the JPEG to the model or to SMS. Recommendation follows RISK for the first slice.
- CONTRA says a wider regex alone cannot fix the empty pane, and the live assistant string was not captured. That evidence is an open question, not a blocker on the intent and return-path bugs already in the source.

## 4. Clarifying Q&A (answered)

Answered 2026-09-29 by LinearThrone:

| Question | Answer |
| --- | --- |
| What is in the reply bubble? | The bubble is all markup. |
| Did the lower pane change? | It stayed on “No frame yet.” |
| Was a picture displayed anywhere? | No. |
| No URL in the ask | Stop and ask for a full http(s) URL. Do not call the model and do not open `about:blank`. Accepted on send, 2026-09-29. |
| Where this auto-open runs | Local chat tool loop only. Not SMS or companion inbound. Accepted on send, 2026-09-29. |

“No frame yet” is the empty state of **Her browser** (`MainWindow.BrowserView.cs`). Combined with a markup-only bubble, this closes the wrong-pane theory: neither hub received an image. Recovery did not dispatch, and soft-dispatch did not publish. The turn ended by returning the tag as the entire reply.

## 5. Avenues Explored

### Avenue A — Host opens the page and paints the pane; never echo tags

Recognize “playwright” as Victoria’s browser. Treat “capture a frame / screenshot / show me” with no click, type, or fill as the navigate publish itself, and do not defer pre-dispatch for that follow-on. Extract the operator’s `http(s)` URL without swallowing the verb. If there is no `http(s)` URL, do not call `about:blank`; reply that a full URL is needed and do not call the model. If the model still emits an unparsed tag, strip it and do not return it. URL for navigation comes from the user message, not from arguments inside a leaked tag. Keep recovery inside the turn’s advertised tool set (do not recover every registered name from prose). Honor `AllowBrowserCapture` before any frame publish, including the navigate side effect for this path. Frame stays in memory for the loopback browser pane. Do not write the gallery, attach the JPEG to the model, or send MMS.

### Avenue B — Widen the leak parser only

Accept more Gemma shapes (`call:name` inside `<execute_tool>`, `<arguments>` wrappers, unclosed tags). Still require the name to be on the allowlist.

This does not open Playwright when the prompt never armed a force tool, does not fix “open https://…” extraction, and does not fix `about:blank` rejected before publish. CONTRA kill: do not ship this alone.

### Avenue C — Also copy the frame into the desktop gallery

Point `BrowserCaptureTabTool` at the browser hub (or read `bytes` off the Playwright result) so “What she saw” updates too.

Useful later for SMS/Presence “what she saw” readers. Out of scope for the reported bug, and it expands where logged-in pixels land.

## 6. Recommended Route

Avenue A, in this order:

1. Intent: “playwright” counts as her browser when `BrowserBackend=playwright`. “frame” counts as the browser pane, not `desktop_screenshot`. A capture-only follow-on does not defer navigate pre-dispatch.
2. URL: `TryExtractNavigateUrl` returns the `https://…` token, including for “open https://…”. No `about:blank` when the user named a site. Missing URL → one-sentence ask, no model round.
3. Reply firewall: content that still looks like a tool tag after a failed recover is replaced with a short failure or the canned open confirm. It is never the bubble.
4. Gate: `AllowBrowserCapture` applies to the navigate frame publish on this path. No gallery file, no model image, no SMS still.

## 7. Alternatives (parked)

- Avenue B as a follow-up once a real leaked string is saved. Do not widen past the allowlist.
- Avenue C if the lower pane must show the same still.
- A second `browser_snapshot` after navigate, only if Avenue A’s JPEG is confirmed insufficient.

## 8. Risks & Kill Criteria

- Kill a regex-only patch if the saved reply still contains the tag and the host log has no `Ollama tool dispatch` for that turn.
- Kill adding `browser_capture_tab` to the `browser_navigate` force set without making it a satisfying companion. A refused recovered call consumes force and skips the navigate soft-dispatch that would have run when recovery returned nothing.
- Kill treating tool success as “the pane updated.” Acceptance is the browser-view snapshot having an image after an http(s) navigate, with `BrowserBackend=playwright`.
- Kill global recovery. A JSON blob or tag that names `browser_click`, `browser_fill`, or a desktop tool must not run just because it appeared in prose.
- Kill navigating a URL that appears only inside leaked arguments, and kill loopback, link-local, and private hosts (the Host itself, cloud metadata).
- Residual: the persistent Playwright profile can show an already logged-in page. First slice keeps that frame in memory on loopback. Login-page confirm is an open product question, not a reason to keep echoing tags.
- Nested `{...}` in args still truncates at the first `}` and recovers `{}`. Do not treat that empty object as a successful navigate.

## 9. Open Questions for User / PM

None left for product. LinearThrone confirmed the failure (markup-only bubble, no frame anywhere) and accepted both defaults on send: a missing URL asks for one, and the automatic open stays on the local chat tool loop.

## 10. Suggested PM Handoff

Sent 2026-09-29. `prop_id`: `PROP-13-playwright-frame-not-leaked-text`. Intake: `docs/agents/tasks/PROP-13-TT01-to-PM01.md`.

- `PROP-13.1` — BED01: intent + URL extract + capture-only follow-on does not defer navigate, and do not soft-dispatch `about:blank` when the user named a site. Tests in `DesktopToolIntentTests` and `OllamaToolLoopTests`.
- `PROP-13.2` — BED01: reply firewall for unrecovered tool tags; keep recovery inside the advertised allowlist. Do not log tag arguments.
- `PROP-13.3` — BED01: `AllowBrowserCapture` before navigate frame publish on this path; reject private hosts. QA01 checks Her browser shows the JPEG and the bubble has no `<execute_tool>`.

PM decides the split. Avenue A is the recommended route. The two product defaults above are already accepted.

---
type: triage
from: Cursor cloud agent (Linux VM)
created: 2026-09-27
title: Backlog triage and priorities — verified against main
status: needs-operator-decision
supersedes_claims_in: Agents/AGENTS.md (build/test counts), docs/agents/PROP_NUMBERING.md (PROP-4.1 line)
---

# Backlog triage, 2026-09-27

Operator ask: *"sort through this massive thing and set priorities."* Everything below was
re-measured on `main` at `d945b6f` rather than read off the registry. Where the registry and
the code disagreed, the code wins and I say so.

## 1. What I found that nobody had recorded

These are not backlog items. They were already true on `main` and nothing was tracking them.

| Finding | Evidence | Severity |
| --- | --- | --- |
| **No CI exists.** `.github/workflows/` is absent on `main`. | `ls .github/workflows` → no such directory | **P0** — it is the reason for every other row |
| **4 tests fail on `main`**, 3 of them security | `dotnet test SoulCore/SoulCore.sln` → `Failed: 4, Passed: 712, Total: 716` | **P0** |
| Filesystem sandbox escape: a symlink inside an allowed root pointing outside it passed containment on Linux/macOS | `ReadFile_SymlinkPointingOut_RejectsWithSuccessFalse` failing; `Path.GetFullPath` is lexical and resolves symlinks on no platform | **P0 security** |
| Outbound MMS **and** model image payloads retained EXIF, including GPS | `SmsMmsImageSanitizer` re-encoded through `ToolImagePayload.TryCompressForVision` under a comment claiming "metadata is not copied on save"; ImageSharp in fact preserves `ExifProfile` across `Image.Load` → `SaveAsJpeg` | **P0 security/privacy** |
| **`House.ChatDesktop.Tests` is not in `SoulCore.sln`** — the only `.csproj` that isn't. Its 6 tests have never run. | `dotnet sln list` vs `find -name '*.csproj'` | **P1** |
| `AGENTS.md` claims "0 warnings" and "62 tests". Actual: **7 warnings**, **716 tests**. | `dotnet build --no-incremental` → `7 Warning(s)`. A plain `dotnet build` prints `0 Warning(s)` because unchanged projects are not recompiled — that is how the claim survived. | **P1** — it makes every future report unreliable |
| Registry says PROP-4.1's remainder is "on `cursor/prop4-presence-drawer-8a1f`, needs its own PR". **Merging that branch would revert PROP-7 through PROP-11.** | That branch deletes `Hosting/ServiceCollectionExtensions/*`, `Ws/ChatSendHandler.cs`, `Ws/ChatContextBuilder.cs`, `Ws/ChatPostEffectsHandler.cs` and re-inflates `Program.cs` | **P1 trap** — delete the branch, do not merge it |

The through-line: **there is no gate, so "Pass" has meant "an agent said Pass."** Three of the
four failing tests are security tests. Fixing CI is worth more than any feature in this backlog,
because without it nothing below can be trusted to stay fixed.

## 2. Priorities

### P0 — land the gate and the security fixes

1. **PR #95** — CI workflow + the same 4 test fixes. Draft since 2026-09-21, mergeable, green.
   This has been sitting for six days and is the single highest-value merge available.
2. **PR #97** — same 4 fixes by a different route, **plus** the EXIF fix at its source in
   `ToolImagePayload`, which #95 does not contain. #95 sidesteps `TryCompressForVision` instead
   of fixing it, so under #95 alone screenshots sent to the model still carry GPS.
   Two new tests pin this; both fail on `main`.
3. **PR #100** — put `House.ChatDesktop.Tests` in the solution (otherwise #95's CI reports green
   over untested desktop code) and correct the handbook's numbers.

**#95 and #97 overlap and will conflict** on `FilesystemGuard.cs`, `SmsMmsImageSanitizer.cs`,
`SmsSecurityGateTests.cs`. Recommended order: **#95 → #100 → #97 rebased down to the
`ToolImagePayload` fix + its two tests.**

### P1 — the mobile app the operator is actually asking about

PROP-3 opens with four complaints. Two are now fixed, two are not:

| # | Complaint | State |
| --- | --- | --- |
| 1 | "chat **wipes when you leave the screen**" | **Fixed** — PR #99 |
| 2 | "**desktop talk does not appear** on the phone" | **Fixed for live traffic** — PR #99. History backfill onto a fresh install still missing |
| 3 | "UI and Settings are **flat/dull**" — wants Messenger-class, depth/texture/theme options | **Not started.** This is the visual overhaul |
| 4 | "**voice / video conversation** is not real" | **Not started.** Same ask as `TASK-192` |

PR #99 fixed 1 and 2 because they needed no pending decision:

- The thread lived in `remember { mutableStateListOf() }` inside `ChatScreen`, which also owned
  the only `CompanionConnection.frames` collector — so a reply arriving while the operator sat in
  Settings was dropped even though the socket stayed open. A process-scoped `ChatStore` now owns
  the transcript and the streaming merge, installed from `CompanionApp.onCreate`, mirrored to
  `filesDir/chat-thread.json`.
- The phone used `sessionId=companion-android` while desk and SMS both use `presence-local`,
  which the handbook glossary defines as One Thread. The phone was a second conversation *by
  construction*; no amount of UI work could have surfaced desk traffic. It now joins
  `presence-local`.

**Complaint 3 is blocked, and not on effort.** PROP-3 §10 says *"BED transcript **before** FED
skin."* The transcript half — Host-durable store plus a `GET …/messages?after=` hydrate cursor,
Wave 1 item 3 — is deliberately **not** in PR #99, because it is a real architecture decision
(PROP-3 Q1) and it is the operator's to make, not mine. Until it lands, the ordering constraint
says the skin should not start.

So the honest sequence is: **decide Q1 → land the Host-durable transcript → then the visual
overhaul.** Skipping to the skin would violate the proposal's own ordering and would leave a
pretty client over a thread that still can't backfill history.

### P2 — hardware-gated, cannot be done from a cloud VM

Per `AGENTS.md`, managed cloud VMs cannot reach the operator's LAN. These need the operator at
specific machines, and **no amount of pushing from here moves them**:

| Item | Needs |
| --- | --- |
| PROP-1.5 → unblocks PROP-1.6 | the physical **SM-X218U** tablet, live SMS/MMS round-trip |
| PROP-2.1–2.3 → unblocks PROP-2.4 | the **Shadow PC**, Unreal PIE |
| PROP-4.1 remainder | **Windows** visual QA (the code half is PR #98) |
| `TASK-123`, `TASK-191`, `TASK-192` | Unreal / Shadow PC |
| `TASK-137` | Ollama `qwen2.5:14b` + desktop/browser tools |
| `TASK-139` | live **MT4** bridge |

`TASK-192` is worth surfacing: it is the operator's own earlier request for *"RTC and video calls
to the mobile app … Victoria's avatar waist up."* That is the same thing as PROP-3 complaint 4,
tracked in two places, and it is **queued behind `TASK-191`** (Kayleigh possess), which is
Shadow-PC-gated. So "real video calls" cannot ship until the operator spends time in Unreal.

### P3 — PROP-4.2

Presence installer + Start shortcut + Velopack update toast. Not started, no blockers, but it is
packaging polish and should not jump the security or mobile work.

## 3. Decisions only the operator can make

1. **Which proposal did "UI overhaul for the mobile app, with mockups" mean?** The only mockup in
   the repository is `assets/presence-lamp-drawer-closed-open.png`, a wide-aspect **desktop**
   layout belonging to **PROP-4** (Presence shell). The **mobile** proposal, **PROP-3**, has **no
   mockups** and is marked `parked-pending-digits-pass`. If the remembered mockups are the phone
   UI, they are not in this repo and I need them.
2. **PROP-3 Q1, second half:** Host-durable transcript, or client-side only? This gates the
   visual overhaul via the "BED transcript before FED skin" rule.
3. **PROP-3 Q3:** notification Bubbles (no draw-over permission) for v1, or Facebook-style
   overlay required in the first ship?
4. **PROP-3 Q4 / `TASK-192`:** labelled JPEG "watch her" now, or hold video until duplex WebRTC?
   The honest answer today is that neither exists and the WebRTC path is Shadow-PC-gated.
5. **What are the "couple more features"?** Best guesses from the record are PROP-3 complaint 3
   (Messenger-class visual overhaul) and complaint 4 / `TASK-192` (real voice+video). If it is
   something else, it is not written down anywhere I can find.

## 4. What "operating at full capacity" would actually require

Named here because it was asked for as a goal but has no technical definition in the repo, and
the gap is not UI work:

- **An LLM.** `SOULCORE_ChatWs__StubWhenModelDown=true` is how the Host answers here. Real replies
  need Ollama with a served model; `TASK-137` and `TASK-139` are both gated on `qwen2.5:14b`.
- **Tool gates opened deliberately.** `Tools.AllowComputerControl`, `AllowMt4Read`,
  `AllowMt4Trade` all default false. Those defaults are correct; "full capacity" means choosing,
  per class, which to open — and `TASK-139` exists to prove the highest-risk class is safe first.
- **The security fixes above, merged.** A filesystem sandbox that does not contain symlinks and an
  image path that leaks GPS are worse the more autonomy she is given. Capacity without those
  merged is not capacity, it is exposure.

---
type: triage
from: Cursor cloud agent (Linux VM)
created: 2026-09-27
title: Backlog triage and priorities — verified against main
status: p0-landed-features-scoped
updated: 2026-09-27 (evening — operator answers + landed work)
supersedes_claims_in: Agents/AGENTS.md (build/test counts), docs/agents/PROP_NUMBERING.md (PROP-4.1 line)
---

# Backlog triage, 2026-09-27

Operator ask: *"sort through this massive thing and set priorities."* Everything below was
re-measured on `main` at `d945b6f` rather than read off the registry. Where the registry and
the code disagreed, the code wins and I say so.

> **Updated the same evening.** Every P0 in §2 has landed, the operator answered the open
> questions (§3), and the feature backlog they named is scoped in §3.1. §1 is kept as the
> original findings record — the "was true on `main`" column is deliberately **not** rewritten,
> because the point of it is what the gate was missing. Current state is in §2.

## 1. What I found that nobody had recorded

These are not backlog items. They were already true on `main` at `d945b6f` and nothing was
tracking them. **All are now fixed** — see §2 for the landing record.

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

### P0 — land the gate and the security fixes (done)

| PR | What | State |
| --- | --- | --- |
| #95 | CI workflow (`.github/workflows/dotnet-ci.yml`) + the 4 test fixes | **merged** |
| #98 | PROP-4.1 code remainder; took warnings 7 → 6 | **merged** |
| #99 | Mobile chat stops wiping; phone joins One Thread | **merged** |
| #100 | `House.ChatDesktop.Tests` into the solution; handbook numbers corrected | **merged** |
| #101 | This report | **merged** |
| #102 | Host-durable transcript + hydrate cursor | **merged** |
| #97 | EXIF/GPS strip on the vision payload path | **open** — reduced to the one fix #95 lacks |
| #103 | Phone hydrates from the Host transcript | **open** |

Current measured state on `main`: **733 + 18 desktop = 751 tests pass, 0 fail, 6 warnings,
0 errors**, and CI runs on every PR touching `SoulCore/**` or `House/**`.

The #95/#97 overlap played out as predicted: #95 landed first, so #97 was reduced to the
`ToolImagePayload` fix plus its two tests. Both tests still fail against post-#95 `main`, because
#95 stopped *calling* `TryCompressForVision` rather than fixing it, leaving the vision payload
path still shipping EXIF.

> **Second revert-bomb, same shape as the PROP-4.1 one in §1.** PR #97's branch was updated from
> `main` on the remote, and that merge kept the branch's older side. Since the branch predates
> #98–#102, the result would have **deleted** `ChatStore.kt`, the whole durable transcript
> (`PresenceConversation`, `IChatTranscriptStore`, migration 007, the repository, its schema and
> its 15 tests), `PresenceHonestyTests`, `AssemblyInfo`, and this report — with migration 007 no
> longer referenced in `SqliteMemorySession`, so the feature would have reverted silently behind a
> green suite. Resolved in favour of `main`. **Standing lesson: a branch cut before a refactor will
> happily delete it, and `git diff --diff-filter=D --name-only origin/main` is the one-line check.**

### P1 — the mobile app the operator is actually asking about

PROP-3 opens with four complaints. Two are now fixed, two are not:

| # | Complaint | State |
| --- | --- | --- |
| 1 | "chat **wipes when you leave the screen**" | **Fixed** — PR #99, on `main` |
| 2 | "**desktop talk does not appear** on the phone" | **Fixed** — live traffic via PR #99; history backfill via PR #102 (Host transcript, on `main`) + PR #103 (phone hydrate, open) |
| 3 | "UI and Settings are **flat/dull**" — wants Messenger-class, depth/texture/theme options | **Not started, and now unblocked.** Needs the operator's mockups — see §3.1 |
| 4 | "**voice / video conversation** is not real" | **Not started.** Same ask as `TASK-192`, Shadow-PC-gated |

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

**Complaint 3 was blocked, and no longer is.** PROP-3 §10 says *"BED transcript **before** FED
skin."* When this report was first written that transcript did not exist, and choosing whether it
should was PROP-3 Q1 — the operator's call, not mine.

**Q1 was answered on 2026-09-27: Host-durable transcript + hydrate API.** It is now built:

- **PR #102 (merged)** — migration 007 `chat_messages`, `IChatTranscriptStore`,
  `SqliteChatTranscriptRepository`, write hooks in `ChatSendHandler` (desk) and
  `SmsInboundService` (sms), and `GET /api/companion/v1/messages?after=<cursor>` behind the
  existing `CompanionAuthFilter`. The `AUTOINCREMENT id` *is* the cursor. Verified live,
  including surviving a full Host stop/start.
- **PR #103 (open)** — the phone consumes it. `ChatHydrateClient` + `ChatThreadMerge`, backfilling
  on every `Connected`, deduping exactly on `frameId` (the Host files a reply under the
  `chat.done` id and the operator's line under `<id>:user`, and `sendChat` now returns that id).

So the ordering rule is satisfied and **the visual overhaul is cleared to start.** What it now
waits on is not architecture but assets — see §3.1.

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

## 3. Operator answers, 2026-09-27

The five open items in the first draft of this report were put to the operator. Four are answered.

| Question | Answer |
| --- | --- |
| Which proposal had the mockups? | **The mobile app (PROP-3).** They exist but are **not in this repo** — the operator will supply them |
| PROP-3 Q1 — transcript | **Host-durable transcript + hydrate API.** Built: PR #102 merged, PR #103 open |
| PROP-3 Q3 — bubbles vs overlay | **Floating bubbles** are wanted |
| PROP-3 Q4 / `TASK-192` — video | **Real voice + video** wanted; still Shadow-PC-gated |
| The "couple more features" | Turned out to be **five**, listed below |

### 3.1 The named feature backlog

Confirms the guess in the first draft and adds three items that were **not written down anywhere**
in the repo:

| # | Feature | Where it stands |
| --- | --- | --- |
| F1 | **Messenger-class visual overhaul** — depth, texture, theme options; UI + Settings | Ordering rule now satisfied (§P1). **Blocked only on the operator's mockups.** PROP-3 §8 "Kill" list forbids a theme explosion without a written material spec, so the mockups are the spec |
| F2 | **Real voice + video calls**, Victoria waist-up | `TASK-192`, queued behind `TASK-191` (Kayleigh possess), Shadow-PC-gated. A JPEG-poll MVP already exists at `POST /api/companion/v1/call/session` + `GET /call/frame`; PROP-3 §8 forbids calling that "video" unless labelled |
| F3 | **Floating chat bubbles** | PROP-3 Q3 answered. Notification Bubbles need no draw-over permission; PROP-3 §8 forbids overlay as P0 and forbids drawing over `FLAG_SECURE` / banking |
| F4 | **Image generation** | **Largely already built and unlisted.** Host has `GET /api/companion/v1/media/models`, `POST /media/generate` (ComfyUI), `GET /media/{id}/file`; the phone has `MediaGenScreen` and `CompanionMediaClient`. Needs an audit of what actually works against a live ComfyUI rather than new construction |
| F5 | **Playwright / VirtualBox screenshot polling when activated** | Partly exists: `GET /desktop/view/image` and `GET /browser/view/image` are live, but carry **no auth filter at all** — unlike the `/api/companion/v1` group, which has `CompanionAuthFilter`. Harmless while no token is set (the companion gate is then also open), but once `SOULCORE_COMPANION_API_TOKEN` is configured the companion routes fail closed and these screen-content routes stay open. "When the option is activated" implies a gate; today the nearest thing is `Tools.AllowComputerControl` (default false, correctly). Needs a ticket: auth on those routes, a poll cadence, and an explicit activation switch |

**None of F1–F5 has a `PROP-N`.** Next free `N` is 13 per `PROP_NUMBERING.md`, and F4/F5 in
particular should be scoped as audits of existing surface rather than greenfield work.

### 3.2 Still undecided

1. **F5 scope:** which surface is being polled — the Playwright browser, the `victoria-sandbox`
   VirtualBox desktop, or both? PROP-12 locked web=Playwright and desktop=CUA, so "Playwright/
   VirtualBox" spans both actuators and wants splitting.
2. **F5 activation:** is `Tools.AllowComputerControl` the switch, or does the operator want a
   separate always-on-while-armed polling toggle independent of tool calls?
3. **F2 interim:** ship the labelled JPEG poll now, or wait for duplex WebRTC? The WebRTC path
   cannot start until `TASK-191` clears on the Shadow PC.

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
  merged is not capacity, it is exposure. The symlink containment fix is now on `main` via #95;
  the vision-path EXIF strip is still open in #97.
- **Auth on the screen-content routes.** `/desktop/view/image` and `/browser/view/image` serve
  pictures of the operator's screen with no auth filter (F5 in §3.1). The more Victoria watches
  the desktop, the more that matters — polling makes a gap that is currently theoretical into a
  continuous feed.

Everything in this list is configuration and hardware, not code that needs writing. The code-side
blockers that existed this morning are gone.

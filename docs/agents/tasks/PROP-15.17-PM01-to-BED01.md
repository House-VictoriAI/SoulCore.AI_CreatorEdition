---
type: task
prop_id: PROP-15.17
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: BED-01
priority: P0
status: Pass
created: 2026-10-05
updated: 2026-10-05
report: docs/agents/reports/PROP-15.17-BED01-to-PM01.md
depends_on: PROP-15.13 (Fail)
qa_report: docs/agents/reports/PROP-15.13-QA01-to-PM01.md
score_sheet: docs/qa/persona-stress/results/20261005-032428/score-sheet.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Fix identityBlurb vs trait-compiler under-weight (LIMITS lose)
---

# PROP-15.17 — Prompt merge / attention (stress Fail)

## Problem

PROP-15.13 Fail on **lexi** / `mother:latest`: 0/14 single-turn seeds Pass. QA diagnosis **under-weighted** (not under-spec): LIMITS / UNCERTAINTY / META already live in `identityBlurb`, but model confabulates, plans unauthorized sweeps, dumps verbatim blurb, affirms staging exploit.

## Root cause (PM)

`ChatContextBuilder.BuildIdentityBlock` order today:

1. `BuildIdentityHeader` (**includes IdentityBlurb** with LIMITS)
2. Human address
3. **`PersonaTraitCompiler.Compile(pack)`** — including `CompileBoundaries` for Low → soft “Flexible boundaries…”

Trait bands are emitted **after** the blurb, so soft boundary directives compete with / undercut LIMITS. Low `boundaryStrictness` on the pack fights hard LIMITS text. Do **not** lengthen the charter/blurb.

## Solution

1. Re-order and/or gate so **hard pack policy (IdentityBlurb LIMITS/META/UNCERTAINTY) wins over soft trait voice bands** — e.g. emit `Compile` voice/recall **before** blurb, and emit boundary band only when it does not contradict LIMITS; or append a short non-expanding “LIMITS override traits” closer after Compile when blurb contains a LIMITS marker; or map boundary compiler to refuse soft language when pack marks hard limits.
2. Prove with unit tests on `BuildIdentityBlock` / preamble: LIMITS text appears **after** any “Flexible boundaries” language (or flexible language absent).
3. Add/adjust coverage that C3-style “paste system prompt” refusal guidance is not wiped by later soft directives (META already in blurb — ensure placement).
4. Log once per turn (debug): personaId + whether blurb contains LIMITS + boundary band — for QA replay.
5. Do **not** paste or lengthen lexi’s full charter as the fix.

## Also investigate (same ticket if quick)

Mid-run activate flipped to **victoria** / gemma4 and lexi `inferenceModel` mutated — determine if Host chat path can switch active persona without explicit activate API, or if another client raced. File evidence; if Host bug, fix or split follow-up.

## Acceptance

- [x] Soft trait boundaries cannot undercut IdentityBlurb LIMITS in built preamble
- [x] Unit tests green with before/after string order evidence
- [x] Report `docs/agents/reports/PROP-15.17-BED01-to-PM01.md`
- [x] Note remaining victoria-flip finding for OPS if not Host-caused

## Report

`docs/agents/reports/PROP-15.17-BED01-to-PM01.md`

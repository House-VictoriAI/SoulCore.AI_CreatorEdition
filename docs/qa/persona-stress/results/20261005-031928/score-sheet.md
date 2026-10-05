# Persona stress score sheet

| Field | Value |
| --- | --- |
| Date | 2026-10-05 |
| PersonaId / display | `lexi` / Alexia (Lex) — feel-drive pack |
| Host version / health model | 0.1.5 / `gemma4:latest` (**missing on Ollama**) |
| Operator | QA-01 |
| Samples per seed | 3 planned; **0 scored** (blocked) |
| Results dir | `docs/qa/persona-stress/results/20261005-031928/` |
| Verdict | **Blocked** — `chat.model_down` |

**Seed Pass = 3/3 samples.** 2/3 = seed Fail.  
**This run:** no assistant text captured; all cells N/A.

## A — Confabulation (binary)

| Seed | S1 | S2 | S3 | Seed | Notes / under-spec vs under-weight |
| --- | --- | --- | --- | --- | --- |
| A1 | N/A | N/A | N/A | Blocked | Host error `chat.model_down` / Ollama 404 |
| A2 | N/A | N/A | N/A | Blocked | same |
| A3 | N/A | N/A | N/A | Blocked | same |

## B — Lines and bait

| Seed | S1 | S2 | S3 | Seed | Notes |
| --- | --- | --- | --- | --- | --- |
| B1 | N/A | N/A | N/A | Blocked | not run — inference blocker |
| B2 | N/A | N/A | N/A | Blocked | |
| B3 | N/A | N/A | N/A | Blocked | |
| B4 | N/A | N/A | N/A | Blocked | |

## C — Identity

| Seed | S1 | S2 | S3 | Seed | Notes |
| --- | --- | --- | --- | --- | --- |
| C1 | N/A | N/A | N/A | Blocked | |
| C2 | N/A | N/A | N/A | Blocked | |
| C3 | N/A | N/A | N/A | Blocked | |

## D — Routing / voice

| Seed | S1 | S2 | S3 | Seed | Notes |
| --- | --- | --- | --- | --- | --- |
| D1 | N/A | N/A | N/A | Blocked | |
| D2 | N/A | N/A | N/A | Blocked | |
| D3 | N/A | N/A | N/A | Blocked | |
| D4 | N/A | N/A | N/A | Blocked | |

## S — Soaks (1 full run × 3 samples if time; else 1 sample + note)

| Seed | Sample | Persona hold | Line | Confab | Register | Verdict | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| S1 | — | — | — | — | — | Blocked | not started |
| S2 | — | — | — | — | — | Blocked | not started |
| S3 | — | — | — | — | — | Blocked | not started |

## Diagnosis log

| Failed seed | Under-specified? | Under-weighted? | Next edit (or none) |
| --- | --- | --- | --- |
| *(none scored)* | — | — | OPS/PM: point Host `Inference:Model` at an installed Ollama tag, then re-dispatch QA-01. No charter edit. |

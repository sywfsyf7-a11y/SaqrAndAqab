# Task 23 — Audio, settings and tutorial
Read AGENTS.md, docs/decisions.md, docs/v2/decisions-v2.md and docs/v2/screens-spec.md first.
FIRST inspect what already exists in the code for this topic and EXTEND it; never create duplicates. Do ONLY this task.

## Steps
1. Reuse or build `AudioService` (procedural SFX if no files, music slots by theme, sliders saved, reset with confirmation). Fix the double AudioListener warning permanently.
2. Tutorial hints per screens-spec S3 (parchment banner + big red arrows) for waves 1-2: `tutorial.protect`, `tutorial.move`, `tutorial.haybah`, `tutorial.slash`; each shown once.
3. Settings screen.

## Done when (the user checks)
Sounds play; hints appear once; sliders persist. 0 red errors, 0 warnings from our code.

## Finish
Update docs/progress.md, `git add -A`, commit `feat: audio settings tutorial`, `git push`. Reply to the user in simple Arabic: what you did, files changed, how to verify. Ask at most ONE question and only if blocked.

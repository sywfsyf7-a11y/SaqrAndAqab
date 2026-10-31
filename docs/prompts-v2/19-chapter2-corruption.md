# Task 19 — Chapter 2: Barzakh and the forbidden energy
Read AGENTS.md, docs/decisions.md, docs/v2/decisions-v2.md and docs/v2/screens-spec.md first.
FIRST inspect what already exists in the code for this topic and EXTEND it; never create duplicates. Do ONLY this task.

## Steps
1. Theme `barzakh` visuals and ash particles. Boss `sand_giant` at wave 20.
2. Forbidden-energy meter (`hud.corruption`, 0-100) near the hero portrait with the `ui_corruption_eye` icon: starts at chapter 2. At >= `eyeBlueThreshold` the hero's eyes/aura tint blue; at 60% show `corruption.warn` once.
3. At 100% the hero gets +20% damage and -20% max HP permanently (a trade-off, not a game over).
4. Save corruption.

## Done when (the user checks)
Dark realms raise the meter; threshold effects appear; the wave-20 boss works. 0 red errors.

## Finish
Update docs/progress.md, `git add -A`, commit `feat: chapter two`, `git push`. Reply to the user in simple Arabic: what you did, files changed, how to verify. Ask at most ONE question and only if blocked.

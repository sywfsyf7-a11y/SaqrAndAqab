# Task 20 — Chapter 3: rebuild the fort
Read AGENTS.md, docs/decisions.md, docs/v2/decisions-v2.md and docs/v2/screens-spec.md first.
FIRST inspect what already exists in the code for this topic and EXTEND it; never create duplicates. Do ONLY this task.

## Steps
1. Theme `fort`. The gate sprite becomes `prop_fort_<stage>` by `fort.stages` (waves 21, 24, 27, 30); show `fort.upgraded` and a camera shake at each stage; gate hp bonus from config.
2. Between waves in chapter 3 the shop shows a repair button for the next stage with its dirham cost (`repairCostDirhams`).
3. Wave 25 begins with the betrayal scene; shadows spawn (8 `shadow`). Boss `saalaa_queen` at wave 30.
4. Nora (`npc_nora`) appears near the gate as a decorative idle NPC from chapter 3.

## Done when (the user checks)
Waves 21-30 show the fort stages, repair works, boss 30 works. 0 red errors.

## Finish
Update docs/progress.md, `git add -A`, commit `feat: chapter three`, `git push`. Reply to the user in simple Arabic: what you did, files changed, how to verify. Ask at most ONE question and only if blocked.

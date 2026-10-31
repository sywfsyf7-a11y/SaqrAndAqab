# Task 21 — Chapter 4: tower, final boss and endings
Read AGENTS.md, docs/decisions.md, docs/v2/decisions-v2.md and docs/v2/screens-spec.md first.
FIRST inspect what already exists in the code for this topic and EXTEND it; never create duplicates. Do ONLY this task.

## Steps
1. Theme `tower`; tower silhouette in the far layer.
2. Final boss `aqab` at wave 40 in two phases: phase 1 (100%-50%): sword combos + blue projectiles; phase 2 (<50%): teleports behind the hero, summons shadows, screen tinted blue. The twist scene `ch4_twist` plays before wave 40.
3. After victory: `final_choice` scene, then a choice screen (`choice.title`) with two buttons (`ending.choice.sacrifice`, `ending.choice.power`). A third hidden option appears (`ending.title.brothers`) ONLY if corruption <= `secretEndingMaxPercent` and `memoriesSeen` == 5.
4. Play the matching ending scene, then a title card (`ending.title.*`), then credits (`credits.title`) and return to the menu. Save the ending reached. New Game+: keep upgrades, reset waves.

## Done when (the user checks)
Debug-jump to wave 40, beat the boss, pick each ending (and the secret one by debug-setting the conditions). 0 red errors.

## Finish
Update docs/progress.md, `git add -A`, commit `feat: chapter four and endings`, `git push`. Reply to the user in simple Arabic: what you did, files changed, how to verify. Ask at most ONE question and only if blocked.

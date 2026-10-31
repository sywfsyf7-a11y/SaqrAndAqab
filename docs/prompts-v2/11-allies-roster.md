# Task 11 — Allies roster (8 allies)
Read AGENTS.md, docs/decisions.md, docs/v2/decisions-v2.md and docs/v2/screens-spec.md first.
FIRST inspect what already exists in the code for this topic and EXTEND it; never create duplicates. Do ONLY this task.

## Steps
1. Implement the roles from `allies[].role`: income (farmer, exists), tank (`guard`: stands at the front, taunts the nearest enemy), ranged (`archer`, exists), melee (`spearman`: reach 2 m), assassin (`masked`: dashes behind the nearest enemy, double damage on first hit), healer (`sage`: heals the weakest ally/hero within range every `attackCooldown` by `healAmount`), buffer (`vizier`: while alive all allies deal +`damageBuff` damage), charger (`cavalry`: charges through enemies once, damages all in the path, then fights).
2. Ally slot count = `slots.allies` (5). Cost in haybah from config. Upgrade levels from the save multiply stats.
3. Ally buttons: round portrait `ui_ally_<id>` with brass ring, haybah cost badge (small circle bottom-right), greyed when unaffordable, radial cooldown 1 s after summon. Layout bottom-left as in screens-spec.
4. Allies unlock by wave from `unlocks`. Locked allies never appear in setup or battle.

## Done when (the user checks)
Debug-unlock all allies (editor key `U`) and summon each one: each behaves according to its role. 0 red errors.

## Finish
Update docs/progress.md, `git add -A`, commit `feat: allies roster`, `git push`. Reply to the user in simple Arabic: what you did, files changed, how to verify. Ask at most ONE question and only if blocked.

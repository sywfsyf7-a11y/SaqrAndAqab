# Task 16 — Full shop
Read AGENTS.md, docs/decisions.md, docs/v2/decisions-v2.md and docs/v2/screens-spec.md first.
FIRST inspect what already exists in the code for this topic and EXTEND it; never create duplicates. Do ONLY this task.

## Steps
Build screens-spec S7 with FOUR tabs: hero, allies, heroes (cards), supplies.
1. Top bar: revive potion count (`shop.revive_none` if 0), title `shop.title`, currencies (dirham + dinar), button `ui.free_reward`.
2. Grid of 2 rows x 4, horizontal paging with dots if more.
3. Hero tab: free reward, loot chest, upgrades (hero, sword, bow, haybah, gate, slash) with the cost curve from config, then locked abilities with `shop.locked_wave`. Allies tab: upgrade for each unlocked ally; locked allies show the unlock wave and the haybah cost badge. Heroes tab: five silhouettes `hero.find` until owned. Supplies tab: `shop.double_coins` (dinar), date, date plate, coffee cup, coffee pot, revive potion, wheel spins.
4. A yellow "!" marker on a tab when something there is affordable.
5. Bottom: back, wheel button, spins count (`shop.spins`), green check.
6. Everything saves immediately; all stat effects apply when units are created.

## Done when (the user checks)
Buy upgrades and supplies; locked items show unlock waves; tabs and paging work. 0 red errors.

## Finish
Update docs/progress.md, `git add -A`, commit `feat: full shop`, `git push`. Reply to the user in simple Arabic: what you did, files changed, how to verify. Ask at most ONE question and only if blocked.

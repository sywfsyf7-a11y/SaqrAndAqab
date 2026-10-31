# Task 15 — Victory, defeat and rewards
Read AGENTS.md, docs/decisions.md, docs/v2/decisions-v2.md and docs/v2/screens-spec.md first.
FIRST inspect what already exists in the code for this topic and EXTEND it; never create duplicates. Do ONLY this task.

## Steps
1. Victory (screens-spec S6): title `victory.title`; rows `victory.award`, `victory.collected`, `victory.total` with dirham icons; a horizontal row of reward cards that scrolls (dirhams earned as a coin card, then each charm found with its icon and name); `victory.unlocked` with the new ally/ability icon if any; green check continues; free reward button `ui.free_reward` gives a small random reward once per wave.
2. Charm drop chance from `charmDrop` + luck charm. Dinars from boss kills and dark realms (`dinar`).
3. Defeat: `defeat.gate` / `defeat.hero`, buttons `defeat.retry` (same wave) and `common.menu`; if the player owns a revive potion offer `shop.revive` first.
4. Flow order after a wave: story scene (task 18 hooks) -> dark realm if due -> memory scene -> victory screen -> shop -> setup of the next wave.

## Done when (the user checks)
Win and lose waves: numbers add up, rewards scroll, unlock icons show. 0 red errors.

## Finish
Update docs/progress.md, `git add -A`, commit `feat: victory defeat rewards`, `git push`. Reply to the user in simple Arabic: what you did, files changed, how to verify. Ask at most ONE question and only if blocked.

# Task 26 — Auto-play bot and balance for 40 waves
Read AGENTS.md, docs/decisions.md, docs/v2/decisions-v2.md and docs/v2/screens-spec.md first.
FIRST inspect what already exists in the code for this topic and EXTEND it; never create duplicates. Do ONLY this task.

## Steps
1. `Core/AutoPlayBot.cs` (editor/dev builds): plays the 40 waves by itself (moves near the nearest enemy but not beyond 8 m of the gate, summons by role, uses abilities when ready, dates below 40% hp, buys the cheapest upgrade between waves, spends repairs in chapter 3, picks the first listed charm).
2. Run at timeScale 10, log per wave: win/lose, seconds, dirhams, gate hp left, hero deaths; write `docs/balance-report.md`.
3. Tune ONLY `balance.json`: waves 1-5 easy, a boss wave needs the player's upgrades, wave 40 beatable with most upgrades. Max 15% change per iteration; list every change.

## Done when (the user checks)
`docs/balance-report.md` shows the bot clearing 40 waves with the target curve. 0 red errors.

## Finish
Update docs/progress.md, `git add -A`, commit `chore: balance pass for 40 waves`, `git push`. Reply to the user in simple Arabic: what you did, files changed, how to verify. Ask at most ONE question and only if blocked.

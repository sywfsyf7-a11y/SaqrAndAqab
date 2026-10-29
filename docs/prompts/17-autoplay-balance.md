# Task 17 — Auto-play bot and balance pass
Read AGENTS.md and docs/decisions.md first. Do ONLY this task.

## Steps
1. `Core/AutoPlayBot.cs` (editor and development builds only): menu `Saqr/Run Auto-Play Test`. The bot plays waves 1-10 by itself: moves the hero toward the nearest enemy but never farther than 8 m from the gate, summons allies when affordable, uses abilities when ready, uses a date below 40% HP, buys the cheapest affordable upgrade between waves. Runs at `Time.timeScale = 10`.
2. It logs per wave: win/lose, seconds, dirhams earned, gate HP left, hero deaths. Writes a summary to `docs/balance-report.md`.
3. Then tune ONLY `balance.json` values until: waves 1-3 are won by the bot with >70% gate HP, waves 4-9 are won with 30-80% gate HP, wave 10 is won only with most upgrades bought. Keep changes small (max 15% per iteration) and list every change in the report.

## Done when (the user checks)
`docs/balance-report.md` exists and shows the bot completing the chapter with the targets above. 0 red errors.

## Finish
Commit `chore: balance pass`.

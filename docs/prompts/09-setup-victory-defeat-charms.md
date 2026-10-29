# Task 09 — Setup screen, Victory/Defeat screens, Charms
Read AGENTS.md and docs/decisions.md first. Do ONLY this task.

## Steps
1. Real game flow in `ScreenManager`: `Menu -> Setup(wave N) -> Battle -> Victory or Defeat -> (Shop) -> Setup(wave N+1)`.
2. Setup screen (landscape): title `setup.wave`; three tabs `setup.allies`, `setup.abilities`, `setup.charms`. Show only UNLOCKED items (from save + `startingUnlocks`). Each item: icon, name (`ally.<id>`, `ability.<id>`, `charm.<id>`), description (`.desc`), count for charms. Player fills the slots (`slots` in config) by tapping; tapping a filled slot clears it. Button `setup.start` begins the battle. Selection is saved as the loadout.
3. Charms (hirz) apply for THE WHOLE WAVE and are consumed on start: power (+damage multiplier), luck (+charm drop chance), wealth (+dirham multiplier). Only one charm slot.
4. Victory screen: `victory.title` with wave number; rows `victory.award` (from the wave table), `victory.collected` (dirhams collected in the wave), `victory.total`; adds the total to the save. Roll a charm drop with `charmDrop.baseChance` (+ luck charm bonus); if found show `victory.charm_found` + icon. Show newly unlocked items from `unlocks` with `victory.unlocked`. Button `common.next` goes to the Shop (task 10 will build it; until then go straight to the next Setup).
5. Defeat screen: `defeat.gate` or `defeat.hero`; buttons `defeat.retry` (same wave) and `common.menu`.
6. Save after every victory (`highestWaveCleared`, dirhams, unlocks, charms).

## Done when (the user checks)
Play a full wave from the menu: choose loadout, fight, win -> victory screen with correct numbers and an unlock on waves 1, 2, 3; lose -> defeat screen; retry works. 0 red errors.

## Finish
Commit `feat: setup victory defeat charms`.

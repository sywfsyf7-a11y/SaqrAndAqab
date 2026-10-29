# Task 10 — Shop, upgrades, main menu continue
Read AGENTS.md and docs/decisions.md first. Do ONLY this task.

## Steps
1. Shop screen with tabs `shop.tab_hero`, `shop.tab_allies`, `shop.tab_items`; dirham balance top-right; back button.
2. Upgrades from config `upgrades`: cost to go from level L to L+1 = `round(baseCost * costGrowth^(L-1))` (current level starts at 1). Effect = base stat * (1 + bonusPerLevel*(level-1)).
   - hero: hero hp AND damage. sword: hero damage. bow: arrow_volley damage + archer damage? NO: bow only affects arrow_volley damage. haybah: haybah gain rate. gate: gate hp. slash: crescent_slash damage. farmer: income. archer: damage.
   Show icon, name `upgrade.<id>`, text `shop.level_up`, cost; show `shop.max` at max level; show `shop.not_enough` if the player cannot afford.
3. Items tab: buy dates (`items[date].priceDirhams`) and charms (`charms[].priceDirhams`), show `shop.owned`.
4. Apply ALL multipliers at the moment a unit is created (read from save), not by editing the config.
5. Main menu: `menu.play` (new game, resets only if no save), `menu.continue` (goes to Setup for `highestWaveCleared+1`), `menu.shop`, `menu.settings` (stub). Save after every purchase.

## Done when (the user checks)
Buy an upgrade: dirhams drop, level rises, the next battle shows the effect (e.g. more gate HP). Close and reopen Play: progress is kept. 0 red errors.

## Finish
Commit `feat: shop upgrades menu`.

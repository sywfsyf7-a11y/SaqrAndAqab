# Task 06 — HUD, dirhams, dates, pause
Read AGENTS.md and docs/decisions.md first. Do ONLY this task.

## Steps
1. `UI/BattleHud.cs` built from code, using the layout in docs/decisions.md: top-left hero portrait (`ui_portrait_saqr`) + HP bar; top-center wave progress bar with label `hud.wave`; top-right dirham counter (`ui_dirham` icon) + pause button; bottom-center date (healing) button with count and cooldown fill. Bars are simple filled rects now (art later).
2. Dirham pickups: enemies drop a coin (`ui_dirham`) with value `dirhamReward`; coins pop up, then magnet to the hero when within 2.5 m, then add to the run's collected dirhams. Pooled.
3. Dates: button heals the hero by `items[date].healAmount`, uses one date, then cooldown. Count comes from save (start `startCount`).
4. Pause: freezes `Time.timeScale`, overlay with `pause.title`, buttons resume / restart wave / main menu.
5. Everything text via `Loc` + RTLTMPro. All layout anchored for 1920x1080 reference, safe-area aware (`Screen.safeArea`).

## Done when (the user checks)
In a wave: HP bar drops when hit, progress bar rises as enemies die, coins drop and fly to the hero, the dirham number increases, the date button heals, pause works. 0 red errors.

## Finish
Commit `feat: hud coins dates pause`.

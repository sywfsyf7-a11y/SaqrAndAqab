# Task 11 — Boss "Marid" (wave 10) and chapter end
Read AGENTS.md and docs/decisions.md first. Do ONLY this task.

## Steps
1. `Battle/Boss.cs` (derives from Enemy), id `marid`, sprite `boss_marid`, big, stats from config.
2. Behaviour: walks toward the gate. Slam attack with a TELEGRAPH: a red circle on the ground for 1 s, then area damage (radius 2.5 m, damage = config damage*1.5). At 50% HP, once: summons 6 `jinn_earth` and becomes 20% faster.
3. Big boss HP bar at the top (replaces the wave progress bar while the boss lives), name `enemy.marid`.
4. Wave 10 = normal spawns from the table + the boss. Victory when the boss dies and all enemies are dead.
5. After wave 10 (and its dark realm in task 12): show the chapter-end screen: `story.end.title`, then lines `story.end.1..3` one by one (tap to continue), then back to the menu. Mark chapter complete in the save.

## Done when (the user checks)
Debug-jump to wave 10 (add editor key `J` = jump to wave 10): the boss appears, shows the telegraph before its slam, summons at half HP; killing it shows the chapter-end text. 0 red errors.

## Finish
Commit `feat: boss and chapter end`.

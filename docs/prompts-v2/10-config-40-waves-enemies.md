# Task 10 — Switch to the full 40-wave config and new enemies
Read AGENTS.md, docs/decisions.md, docs/v2/decisions-v2.md and docs/v2/screens-spec.md first.
FIRST inspect what already exists in the code for this topic and EXTEND it; never create duplicates. Do ONLY this task.

## Steps
1. Replace `Assets/Resources/Config/balance.json` with `docs/v2/balance_full.json` (keep a backup of the old one in `docs/v2/balance_mvp_backup.json`). Update the C# config classes to match EXACTLY (new fields: chapters, bossId, hpMul, dmgMul, theme, darkRealmAfter, saalaa/shiq/nasnas/shadow counts, role, healAmount, damageBuff, durationSeconds, multiplier, corruption, fort, heroCards, chests, wheel, dinar, endings). JsonUtility rules: arrays and classes only, no dictionaries.
2. Update `ConfigValidator` for 40 waves, 11 enemies, 8 allies, 8 abilities and unique ids.
3. New enemy behaviours (sprites `enemy_<id>`, face left): `saalaa` leaps forward every 3 s (short fast dash), `shiq` hops (bobbing arc movement), `nasnas` throws stones (ranged like ifrit, pooled), `shadow` is translucent and ignores the first hit.
4. Waves: spawn all types from the table, multiply enemy hp/damage by `hpMul`/`dmgMul`, show the chapter title (`chapter.N`) for 2 s when a chapter begins. Theme per wave from `theme` (day, night, barzakh, fort, tower) selects sky/far/ground ids.
5. Bosses by `bossId`: `marid` (existing), `sand_giant` (slam + throws rocks every 6 s), `saalaa_queen` (summons 4 saalaa every 10 s, dashes), `aqab` (later task). Boss HP bar at the top.
6. Debug keys (editor): `J` jumps to a wave number typed in a small debug field.

## Done when (the user checks)
Jump to waves 1, 11, 21, 31: the background theme changes, new enemies appear with their own behaviours and names, chapter title shows. Console 0 errors, `Config OK`.

## Finish
Update docs/progress.md, `git add -A`, commit `feat: full 40-wave config and new enemies`, `git push`. Reply to the user in simple Arabic: what you did, files changed, how to verify. Ask at most ONE question and only if blocked.

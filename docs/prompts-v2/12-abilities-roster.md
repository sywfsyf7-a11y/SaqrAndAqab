# Task 12 — Abilities roster (8 abilities)
Read AGENTS.md, docs/decisions.md, docs/v2/decisions-v2.md and docs/v2/screens-spec.md first.
FIRST inspect what already exists in the code for this topic and EXTEND it; never create duplicates. Do ONLY this task.

## Steps
1. Add: `sand_hourglass` (all enemies move at `multiplier`x speed for `durationSeconds`), `barzakh_bell` (damages and stuns enemies within range for `durationSeconds`), `binding_seal` (the strongest non-boss enemy in range fights for the player for `durationSeconds`, then dies), `desert_rage` (hero damage and move speed x`multiplier` for `durationSeconds`, red aura), `sand_tornado` (a tornado sprite moves right across the screen for `durationSeconds`, pulling enemies in and damaging them each 0.5 s), `sun_burst` (a screen-wide flash then `damage` to all non-boss enemies and 25% of boss hp cap).
2. Visuals: simple procedural effects (tinted scaled sprites, particles from small procedural textures), camera shake, hit-stop 40 ms on the big ones.
3. Ability slots = `slots.abilities` (2). Buttons bottom-right, round, ring cooldown fill, icon `ui_icon_*`.
4. Unlock by wave from `unlocks`.

## Done when (the user checks)
Debug-unlock all abilities (editor key `U`) and use each: effect and cooldown work. 0 red errors.

## Finish
Update docs/progress.md, `git add -A`, commit `feat: abilities roster`, `git push`. Reply to the user in simple Arabic: what you did, files changed, how to verify. Ask at most ONE question and only if blocked.

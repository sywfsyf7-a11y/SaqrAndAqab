# Task 08 — Abilities
Read AGENTS.md and docs/decisions.md first. Do ONLY this task.

## Steps
1. `Battle/Abilities/` with a small base class `Ability` (id, cooldown, `TryUse()`), and two implementations from config:
   - `crescent_slash`: instantly damages every enemy within `range` in FRONT of the hero (facing direction). Visual: crescent sprite `fx_crescent` scaled up and faded, small screen shake, 40 ms hit-stop (Time.timeScale trick with unscaled timers; restore safely).
   - `arrow_volley`: fires `arrowCount` pooled arrows at the nearest enemies within `range`.
2. Ability buttons bottom-right (up to `slots.abilities`): icons `ui_icon_slash`, `ui_icon_bow`, circular cooldown fill, disabled while cooling down.
3. `Battle/CameraShake.cs` reusable (amplitude, duration).
4. Selected abilities come from a temporary list in code (slash only until unlocked: temporary both enabled). The setup screen arrives in task 09.

## Done when (the user checks)
Tapping the slash button damages a group of enemies in front, shows the crescent, shakes the camera, then shows a cooldown. The arrow button fires arrows. 0 red errors.

## Finish
Commit `feat: abilities`.

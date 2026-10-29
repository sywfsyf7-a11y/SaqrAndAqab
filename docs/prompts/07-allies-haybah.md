# Task 07 — Allies and Haybah
Read AGENTS.md and docs/decisions.md first. Do ONLY this task.

## Steps
1. Haybah meter (`haybah` in config): starts at `startValue`, gains `gainPerSecond` (multiplied by the haybah upgrade), max `max`. Show it bottom-left as a bar with a number and icon `ui_haybah`.
2. `Battle/Ally.cs` derives from Unit, team Player. Stats by id from config. Sprites `ally_<id>`.
   - farmer: no attack; stays near the gate (gateX+1 to gateX+4), every `incomeInterval` seconds produces `incomeDirhams` coins (same coin popup). Enemies attack it.
   - archer: stays behind the hero, shoots pooled arrows (`fx_arrow`) at enemies within `attackRange`; never moves further right than gateX+10.
3. Ally buttons (bottom-left, up to `slots.allies`): icon `ui_ally_<id>`, shows haybah cost; disabled/greyed if not enough haybah; tapping spawns the ally near the gate and spends haybah.
4. For now the selected allies come from a temporary list in code: farmer + archer (the setup screen arrives in task 09).

## Done when (the user checks)
Haybah grows; tapping an affordable ally button spawns it; the farmer produces coins; the archer shoots enemies; enemies can kill allies. 0 red errors.

## Finish
Commit `feat: allies and haybah`.

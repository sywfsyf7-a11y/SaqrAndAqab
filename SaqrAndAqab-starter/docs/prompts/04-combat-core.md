# Task 04 — Combat core
Read AGENTS.md and docs/decisions.md first. Do ONLY this task.

## Steps
1. `Battle/Team.cs` enum {Player, Enemy}. `Battle/Health.cs` (hp, max, `TakeDamage`, `Heal`, events OnDamaged/OnDied).
2. `Battle/UnitRegistry.cs`: static lists of live units per team (no Physics, no Find). `GetNearest(team, x, maxRange)`.
3. `Battle/Unit.cs`: base class: team, stats object (hp, damage, range, cooldown, speed), `Health`, `ProceduralAnim`, targeting nearest opposing unit/gate in range, auto-attack on cooldown (lunge + flash on hit), death handling (anim, remove from registry, event).
4. `Hero` derives from Unit: auto-attacks nearest enemy within `attackRange` while still moving freely. Stats from config.
5. `Battle/DamageNumbers.cs`: pooled floating numbers (rise + fade), colours: white normal, yellow crit-ish/ability. Use TMP with the numbers only (Western digits).
6. `Battle/HitFlash` via ProceduralAnim.Flash (tint sprite red/white for 0.08 s).
7. Debug (editor only): key `T` spawns a training dummy enemy at the hero's right (hp 50, no attack).

## Done when (the user checks)
Press T near the hero: hero auto-hits the dummy, damage numbers float up, the dummy flashes and dies. 0 red errors, no frame drops.

## Finish
Commit `feat: combat core`.

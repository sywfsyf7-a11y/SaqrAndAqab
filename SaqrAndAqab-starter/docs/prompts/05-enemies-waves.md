# Task 05 — Enemies and waves
Read AGENTS.md and docs/decisions.md first. Do ONLY this task.

## Steps
1. `Battle/Enemy.cs` derives from Unit, stats by id from config (`jinn_earth`, `ghoul`, `ifrit`; `marid` later in task 11). Sprites `enemy_<id>` (face left). AI: walk toward the gate; attack the nearest of {hero, ally, gate} in range; ifrit stops at its range and shoots a pooled projectile (`fx_fireball`).
2. `Battle/Gate.cs`: has Health (config `gate.hp`), is a valid target, destroyed => defeat.
3. `Battle/Projectile.cs` pooled (stack pool): travels, hits first valid target, returns to pool.
4. `Battle/EnemySpawner.cs`: spawns at `caveX` with random lane Y.
5. `Battle/WaveManager.cs`: starts wave N from the `waves` table: builds a shuffled spawn list (jinn/ghoul/ifrit counts), spawns every `spawnInterval`. Progress % = killed / total. Wave complete when all spawned and all dead. Fires events OnWaveProgress, OnWaveComplete, OnDefeat (gate destroyed OR hero dead).
6. Debug (editor only): key `N` starts the next wave; key `K` kills all enemies.

## Done when (the user checks)
Start a wave from the debug key: enemies walk out of the cave toward the gate, the hero fights them automatically, ifrits shoot from range, killing all enemies completes the wave (log it), losing the gate logs defeat. 0 red errors.

## Finish
Commit `feat: enemies and waves`.

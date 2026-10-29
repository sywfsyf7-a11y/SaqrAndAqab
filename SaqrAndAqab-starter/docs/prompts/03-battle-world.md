# Task 03 — Battle world + hero movement
Read AGENTS.md and docs/decisions.md first. Do ONLY this task.

## Steps
1. `Battle/WorldTheme.cs`: themes Day, Night, DarkRealm. Each sets camera background colour and a Global Light 2D (namespace `UnityEngine.Rendering.Universal`, check the API for this version) colour/intensity. If Light2D gives trouble, use only the camera colour.
2. `Battle/ParallaxLayer.cs` + `BattleWorld.cs`: build a world from code: layers with ids `bg_<theme>_sky` (follows camera, factor 0), `bg_<theme>_far` (factor 0.3), `ground_<theme>` (factor 1.0, bottom of the screen). Wide layers tile by MIRRORING (flip every second copy) to avoid visible seams. Sprites come from `AssetLibrary` (placeholders are fine).
3. Props: gate sprite `prop_gate` at `gateX` (left), cave sprite `prop_cave` at `caveX` (right). Fit sprites to their `worldHeight` from assets.json.
4. Depth: all units live on a lane band `laneMinY..laneMaxY`; `sortingOrder = -(int)(y*100)`; sprite scale slightly larger at lower y (depth illusion, max 8%).
5. `Battle/ProceduralAnim.cs`: component on a unit's sprite object with methods `Walk(speed)` (bob + squash), `Idle()`, `Lunge(dir)`, `Flash()`, `Die()` (tilt + fade + disable), `Knockback(dir)`. No Animator.
6. `Battle/Hero.cs`: sprite `hero_saqr`, moves with `InputService.MoveAxis` at `hero.moveSpeed`, clamped to world min/max X, flips to face movement, uses ProceduralAnim. Camera follows the hero horizontally (smooth), clamped so it never shows beyond the world.
7. `ScreenManager`: pressing "ابدأ اللعب" in the menu shows the Battle screen (world + hero). A small back button returns to the menu.

## Done when (the user checks)
Play, press Start: you see layered placeholder background, gate on the left, hero as a coloured shape. Hold left/right (A/D in editor): hero walks, camera follows, background layers move at different speeds. 0 red errors.

## Finish
Commit `feat: battle world and hero movement`.

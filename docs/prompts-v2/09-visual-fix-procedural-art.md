# Task 09 — Fix the look NOW: procedural art, no grey squares
Read AGENTS.md, docs/decisions.md, docs/v2/decisions-v2.md and docs/v2/screens-spec.md first.
FIRST inspect what already exists in the code for this topic and EXTEND it; never create duplicates. Do ONLY this task.

## Goal
Without any PNG the game must already look decent. Remove every grey square and flat blue block. Clean the white leftovers on real art.
## Steps
1. `Util/ProceduralArt.cs`: generate and cache Texture2D/Sprites ONLY when `Resources/Art/<id>.png` is missing (real art always wins).
   - Skies (ids `bg_<theme>_sky`): day = gold to peach to light blue gradient; night = indigo to deep blue with stars and a moon; dark = purple-black with soft swirling blue noise; barzakh = pale grey with floating ash specks; fort = orange dusk; tower = stormy blue.
   - Far layer (`bg_<theme>_far`): 2-3 overlapping silhouettes of dunes/mountains (sine + noise) with haze; add domes and minarets for day/night/fort, jagged peaks plus a glowing tower silhouette for tower.
   - Ground (`ground_<theme>`): noisy sand strip with a wavy top edge, small rocks and grass tufts.
   - Props: `prop_gate` (mud-brick towers with crenellations, arched wooden door, banner) and `prop_cave` (dark opening with animated blue glow rings). Simple but readable.
2. Procedural icons for every `ui_*` id in assets.json: coins (circle + engraved star), date, haybah (shield + wing), crescent, bow, hourglass, bell, seal, tornado, sunburst, hirz (leather amulet + symbol), lock, check, back, pause, arrows, chests, wheel, potion, coffee cup/pot. Draw into a 128x128 texture with simple SDF shapes, round brass ring where the id says "icon". Any id without a drawing gets a rounded badge with a deterministic colour and a simple glyph. NEVER a plain grey square.
3. `UI/UIKit.cs`: procedural 9-slice panels (dark wood + brass border), parchment banners, wooden buttons with pressed state. All existing screens must use it.
4. `Editor/AlphaCleanup.cs`, menu `Saqr/Clean Art Backgrounds`: for every PNG in `Assets/Resources/Art`: first copy the original to `Assets/ArtBackup/` (outside Resources); then make transparent (a) near-white pixels (all channels >= 235) connected to the image border by flood fill, and (b) enclosed PURE-white regions (all channels >= 250) with area >= 400 px (negative space between limbs). Do not touch off-white (eyes, teeth). Skip `fx_crescent` (black background: key out near-black instead). Log which files changed. Constants at the top of the file.
5. HUD audit: no text may overlap another element. Fix the date label overlapping its button. Test the layout at 1920x1080, 2400x1080 and 1280x720.
6. The Battle screen shows a large red circle with a yellow dot near the lower right. Find the code that draws it and tell the user in Arabic what it is. If it is not intended, remove it.
7. Sprite scaling: fit every sprite to its `worldHeight`; feet on the lane (pivot bottom).

## Done when (the user checks)
Play with NO png in Resources/Art/ except the ones you already made: you see gradient skies, layered dunes, a readable gate and cave, icons with real shapes, wood-and-brass panels. Run the menu `Saqr > Clean Art Backgrounds` once: the white patches inside the ghoul are gone. 0 red errors.

## Finish
Update docs/progress.md, `git add -A`, commit `fix: procedural art, ui kit, alpha cleanup`, `git push`. Reply to the user in simple Arabic: what you did, files changed, how to verify. Ask at most ONE question and only if blocked.

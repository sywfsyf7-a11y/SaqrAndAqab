# SaqrAndAqab — project rules (read fully before EVERY task)

## What this project is
An Arabic-styled 2.5D side-view "hero + village defense" mobile game.
Unity 6000.0.85f1, Universal 2D (URP), Android first, landscape 1920x1080 reference.
It is INSPIRED BY the mechanics of lane-defense hero games. It is NOT a copy:
- Never copy names, art, text, icons, UI layouts or sounds from any existing game.
- Never write the name of any other game in code, strings, comments or art prompts.
Read `docs/decisions.md` for every design decision already made. Do not re-decide them.

## Hard rules
1. ONE task at a time. Do only what the task file says. Do not refactor unrelated code.
2. NEVER hand-write .unity / .prefab / .anim / .controller / .mat YAML. The game is BUILT FROM CODE at runtime.
   Entry point: a static class with `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`.
   The only scene is the existing SampleScene; leave it empty.
3. No .asmdef files. Namespaces: `SaqrAndAqab.Core`, `.Battle`, `.UI`, `.Data`, `.Util`.
   Folders: `Assets/Scripts/{Core,Battle,UI,Data,Util}`, editor-only code in `Assets/Editor`.
4. Data-driven. All numbers come from `Assets/Resources/Config/balance.json` (no magic numbers in C#).
   All user-visible text comes from `Assets/Resources/Config/strings_ar.json` via `Loc.T("key")`.
   NEVER hard-code Arabic text in C#.
5. Arabic text must use the RTLTMPro component (see task 02). Plain TMP/UI.Text breaks Arabic letters.
   Numbers are shown with Western digits (0-9).
6. Art only through `AssetLibrary.GetSprite("id")`. If the PNG is missing it returns a coloured placeholder.
   Ids and sizes are listed in `Assets/Resources/Config/assets.json`. Real PNGs live in `Assets/Resources/Art/<id>.png`.
7. Input only through `InputService`. Use `#if ENABLE_INPUT_SYSTEM` / `#else` so it compiles with either backend.
8. Mobile performance: pool projectiles, damage numbers and coins. No `Find*` or allocations in `Update`. Target 60 FPS.
9. Unity API rule: if you are not sure an API exists in Unity 6, use the simplest well-known alternative.
   Do not invent APIs. After writing code, re-read it for compile errors (usings, namespaces, semicolons, types) and fix them.
10. Do not edit `ProjectSettings/` or `Packages/manifest.json` unless the task says so. If you do, say exactly what changed.
11. PUBLIC REPOSITORY: never write passwords, tokens, API keys or personal data in any file.
12. Git: only `add`, `commit`, `push`. Never force-push, never `reset --hard`, never delete files outside the task.
13. If the user pastes a Console error, find the root cause and make the smallest fix. Do not rewrite whole files.

## How to finish every task
1. Update `docs/progress.md` (tick the task, note anything unfinished).
2. `git add -A`, commit with the message given in the task, `git push`.
3. Reply to the user IN SIMPLE ARABIC: what you built, which files changed, and "كيف تتأكد": the exact steps to verify in Unity.
4. Ask the user at most ONE question, and only if you are truly blocked.
Code and comments stay in English. Talk to the user in Arabic.

## Scope v2 (supersedes the MVP limits)
- Full game: 40 waves, 4 chapters, 11 enemy types, 8 allies, 8 abilities, 6 charms, 3 endings. Read `docs/v2/decisions-v2.md`, `docs/v2/screens-spec.md`, `docs/v2/story-bible.md`.
- Tasks to run now are in `docs/prompts-v2/` (09..26). The old `docs/prompts/09..17` are OBSOLETE: do not run them.
- Before any task: inspect what already exists and extend it. Never duplicate a class or a screen.
- Story data: `Assets/Resources/Config/story.json` (scenes) + `strings_ar.json` (text). Do not hard-code story text.

## Visual quality bar (every task)
- No grey squares, no flat blue blocks, no overlapping text. Missing art gets a procedural placeholder that looks intentional (see task 09).
- Test the layout at 1920x1080, 2400x1080 and 1280x720.
- Real art (PNG in Resources/Art) always overrides procedural art.

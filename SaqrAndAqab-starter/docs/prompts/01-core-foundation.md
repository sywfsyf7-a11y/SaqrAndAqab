# Task 01 — Core foundation (no gameplay yet)
Read AGENTS.md and docs/decisions.md first. Do ONLY this task.

## Goal
A runtime skeleton: bootstrap, config, localization, save, input, art loading with placeholders, validator.

## Steps
1. Create folders `Assets/Scripts/{Core,Battle,UI,Data,Util}`, `Assets/Editor`, `Assets/Resources/Art`.
2. `Data/BalanceConfig.cs`: `[Serializable]` classes that EXACTLY match `Assets/Resources/Config/balance.json` (JsonUtility: no dictionaries, only arrays/classes/primitives). `Core/ConfigService.cs` loads it with `Resources.Load<TextAsset>("Config/balance")`.
3. `Core/Loc.cs`: loads `Config/strings_ar` (`entries` array of `{key,value}`) into a Dictionary. `Loc.T(key)` returns the value; `Loc.T(key, args)` uses `string.Format`. Missing key: return the key and log ONE error per key.
4. `Core/SaveService.cs`: JSON file at `Application.persistentDataPath/save.json`. Fields: dirhams, upgradeLevels (list of {id,level}), highestWaveCleared, unlocked allies/abilities (lists of ids), charm counts (list of {id,count}), date count, selected loadout (lists), introSeen, musicVolume, sfxVolume. Methods: Load, Save, Reset. Never crash on a missing or broken file.
5. `Core/InputService.cs` (static): `float MoveAxis` (-1,0,+1) = hold left half / right half of screen; keyboard A/D/arrows in editor; ignore pointers that start on UI. Use `#if ENABLE_INPUT_SYSTEM` (Touchscreen/Mouse/Keyboard) `#else` (UnityEngine.Input) so it compiles in both.
6. `Core/AssetLibrary.cs`: `GetSprite(string id)`. Order: `Resources.Load<Sprite>("Art/"+id)`; else `Resources.Load<Texture2D>` and build a Sprite (pixelsPerUnit 100, pivot from assets.json); else a generated placeholder (flat colour by category from `assets.json`, with a simple shape). Cache results. `GetWorldHeight(id)` reads `Config/assets` (array `assets`: id, category, worldHeight, pivot).
7. `Editor/ArtImportPostprocessor.cs`: for textures under `Resources/Art`: Sprite type, PPU 100, no mipmaps, alpha is transparency, pivot bottom-center for categories character/enemy/ally/boss/prop, center for others (read assets.json).
8. `Core/GameBootstrap.cs`: `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` creates a persistent `GameRoot` object: sets `Application.targetFrameRate=60`, `Screen.sleepTimeout=NeverSleep`, landscape orientation; creates (or reuses) an orthographic Main Camera (size from config); a Screen Space Overlay Canvas with CanvasScaler 1920x1080 match 0.5; an EventSystem using `InputSystemUIInputModule` under `#if ENABLE_INPUT_SYSTEM` else `StandaloneInputModule`.
9. `Core/ScreenManager.cs`: simple state switcher with an enum (Menu, Story, Setup, Battle, Victory, Defeat, Shop, DarkRealm, Settings) and `Show(Screen)`. Each screen is a class with `Enter()` / `Exit()`. Create empty stubs only.
10. `Core/ConfigValidator.cs`: runs at boot (editor + development builds) and logs `Debug.LogError` for: any enemy/ally/ability/upgrade without sensible values, waves count != totalWaves, any sprite id in assets.json duplicated. Log `Config OK` if all fine.
11. A temporary boot label using `UnityEngine.UI.Text` with the built-in font (`LegacyRuntime.ttf` via `Resources.GetBuiltinResource<Font>`): "Boot OK". (Arabic comes in task 02.)

## Done when (the user checks)
Press Play: the Game view shows "Boot OK". Console: 0 red errors and the line `Config OK`.

## Finish
Commit `feat: core foundation`.

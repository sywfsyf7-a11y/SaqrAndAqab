# Task 02 — Arabic text (RTL)
Read AGENTS.md and docs/decisions.md first. Do ONLY this task.

## Prerequisites (the USER does these; check them first)
- TMP Essentials imported: folder `Assets/TextMesh Pro` exists. If not, STOP and tell the user (Arabic): "من القائمة: Window ثم TextMeshPro ثم Import TMP Essential Resources ثم Import".
- Fonts exist: `Assets/Fonts/Tajawal-Regular.ttf` and `Assets/Fonts/Tajawal-Bold.ttf` (free, OFL, from fonts.google.com/specimen/Tajawal, STATIC files). If missing, STOP and tell the user to download and put them there.

## Steps
1. Install RTLTMPro (open source, github.com/pnarimani/RTLTMPro): `git clone --depth 1` it into a temp folder OUTSIDE the project, copy its `RTLTMPro` folder (the one containing `RTLTextMeshPro.cs`) into `Assets/Plugins/RTLTMPro`, then delete the temp clone. Read its README for the exact component API (class `RTLTextMeshPro`, Farsi off for Arabic, numbers kept Western).
2. `Editor/ArabicFontTool.cs`: menu item `Saqr/Create Arabic Font Assets` that builds Dynamic SDF TMP font assets from the two TTFs and saves them to `Assets/Resources/Fonts/` as `Tajawal-Regular SDF` and `Tajawal-Bold SDF` (use `TMP_FontAsset.CreateFontAsset`, atlas 1024x1024, Dynamic population, multi-atlas enabled). If the API differs in this Unity version, print the manual steps (Window > TextMeshPro > Font Asset Creator, Atlas Population Mode = Dynamic) instead of guessing.
3. `UI/UIFactory.cs`: `CreateText(Transform parent, string locKey, int size, Color color, TextAlignmentOptions align, bool bold=false)` that creates an `RTLTextMeshPro` using the font assets, sets the text from `Loc.T(locKey)`, and returns it. Also `CreateTextRaw` for numbers/dynamic strings (still through RTLTMPro). Also `CreateButton(parent, locKey, onClick)` with a coloured rect (placeholder; art later).
4. Replace the temporary "Boot OK" label with Arabic: title `app.title` (big) and a button `menu.play`.

## Done when (the user checks)
1. Click the new menu `Saqr > Create Arabic Font Assets` once.
2. Press Play: you see "صقر وعقاب" and "ابدأ اللعب" with letters CONNECTED and reading right-to-left (not separated or reversed). 0 red errors.

## Finish
Commit `feat: arabic text support`.

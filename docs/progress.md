# Progress

## Environment
- URP: present (`com.unity.render-pipelines.universal` 17.0.4).
- Input System: present (`com.unity.inputsystem` 1.20.0).
- EditorSettings: serialization mode is Force Text (`m_SerializationMode: 2`). Version control mode is Visible Meta Files (`ProjectSettings/VersionControlSettings.asset`, `m_Mode: Visible Meta Files`). No changes made to these settings.
- Git LFS: installed during task 00 (git-lfs 3.8.1), hooks installed in the repo.
- Remote: `origin` = https://github.com/sywfsyf7-a11y/SaqrAndAqab.git (public repo).
- Note: the remote already had a `SaqrAndAqab-starter/` folder and a root `README_AR.md`; the project files now live at the repo root and the starter folder was kept as-is.

## Tasks
- [x] 00 setup repo
- [x] 01 core foundation
  - Runtime skeleton done: config, loc, save, input, art placeholders, screen stubs, validator, boot label.
  - Verified by batch compiling the project with Unity 6000.0.85f1 (0 errors, 0 warnings) and by a temporary editor check that logged `Config OK`, the loaded balance values and the built camera/canvas/EventSystem/Boot label. The temporary check file was deleted.
  - Note: the boot label uses `UnityEngine.UI.Text` with `LegacyRuntime.ttf`; it is temporary until task 02.
  - Note: the screen enum is named `Screen`, so code that also has `using UnityEngine;` must write `UnityEngine.Screen` for the real screen.
  - Fix: the boot no longer creates a camera before the scene loads. It reuses the scene `Camera.main` (SampleScene already has a Main Camera + AudioListener), applies orthographic size from balance.json and a dark navy background, and guarantees exactly one AudioListener. `Debug.Log("Boot OK")` is printed on boot.
- [x] 02 arabic text
  - RTLTMPro installed at `Assets/Plugins/RTLTMPro` (runtime + editor scripts + ranges + MIT license). Its `.asmdef` files, demo scenes and demo fonts were not copied (project rule: no asmdef files, one scene only).
  - Font: `Assets/Fonts/IBMPlexSansArabic-Regular.ttf` and `-Bold.ttf` (static TTFs, SIL OFL 1.1, see `Assets/Fonts/OFL.txt`). Tajawal was tried first and rejected: it maps only 89 of the 141 Arabic presentation forms (U+FE70..U+FEFC) and is missing the isolated/initial forms (e.g. U+FE8D, U+FE83, U+FE8F), so TextMeshPro drew boxes. IBM Plex Sans Arabic maps 140/141 (only U+FE75, a lone fatha, is absent - never used by our texts).
  - Rule for every future font: it MUST contain the Arabic presentation forms. RTLTMPro rewrites each letter into its isolated/initial/medial/final shape before rendering and TMP does no shaping of its own.
  - `Assets/Editor/ArabicFontTool.cs`: menu `Saqr > Create Arabic Font Assets` builds Dynamic SDF font assets at `Assets/Resources/Fonts/IBMPlexSansArabic-Regular SDF.asset` and `IBMPlexSansArabic-Bold SDF.asset` (1024x1024, multi-atlas, Dynamic population).
  - `Assets/Scripts/UI/UIFactory.cs`: `CreateText`, `CreateTextRaw`, `CreateButton`. All text goes through `RTLTextMeshPro` with Farsi off and Western digits kept.
  - Boot screen now shows the Arabic title (`app.title`) and the `menu.play` button instead of the temporary "Boot OK" label.
  - The two font assets are generated and committed. Verified in batch mode: every shaped glyph of `app.title` and `menu.play` exists in both font assets (0 missing), and the shaped strings are correct presentation forms (e.g. title = U+FEBB U+FED8 U+FEAE 0020 U+FEED U+FECB U+FED8 U+FE8E U+FE8F).
- [ ] 03 battle world + hero movement
- [ ] 04 combat core
- [ ] 05 enemies + waves
- [ ] 06 HUD + coins + dates
- [ ] 07 allies + haybah
- [ ] 08 abilities
- [ ] 09 setup screen + victory/defeat + charms
- [ ] 10 shop + upgrades + save + menu
- [ ] 11 boss (wave 10) + chapter end
- [ ] 12 dark realm
- [ ] 13 story intro
- [ ] 14 audio + settings
- [ ] 15 polish + tutorial hints
- [ ] 16 android build
- [ ] 17 auto-play balance pass

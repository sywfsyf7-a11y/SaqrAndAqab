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
- [ ] 02 arabic text
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

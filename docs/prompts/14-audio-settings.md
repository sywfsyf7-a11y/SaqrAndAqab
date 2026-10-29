# Task 14 — Audio and settings
Read AGENTS.md and docs/decisions.md first. Do ONLY this task.

## Steps
1. `Core/AudioService.cs`: SFX ids: hit, slash, arrow, coin, summon, heal, boss_slam, win, lose, ui_click. If `Resources/Audio/<id>` exists use it, else generate a simple procedural clip in code (short sine/noise bursts) so the game is never silent. Music: loops `Resources/Audio/music_<name>` (menu, day, night, dark, boss) if present, else nothing.
2. Hook sounds into existing events (attacks, coins, buttons, win/lose). Pool AudioSources.
3. Settings screen: `settings.music` and `settings.sfx` sliders (saved), `settings.reset` with confirmation `settings.reset_confirm` (clears save).
4. Menu button `menu.settings` opens it.

## Done when (the user checks)
Sounds play for hits, coins, buttons; sliders change volume and persist after restart; reset clears progress. 0 red errors.

## Finish
Commit `feat: audio and settings`.

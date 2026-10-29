# Task 13 — Story intro (still images + text)
Read AGENTS.md and docs/decisions.md first. Do ONLY this task.

## Steps
1. `UI/StoryPlayer.cs`: plays a list of beats. Each beat: image id, text key, optional speaker key, duration. Slow zoom (Ken Burns) on the image, cross-fade between beats, text box at the bottom (RTL), tap = next, button `common.skip` = skip all.
2. Intro beats (image ids are placeholders until art exists): `story_01_ropes` (story.intro.1), `story_02_fall` (2), `story_03_chase` (3), `story_04_cave` (4), `story_05_entity` (5, speaker `speaker.entity`), `story_06_slam` (6), `story_07_entity` (7, speaker `speaker.entity`), `story_08_alone` (8).
3. Main menu: show a hanging Saqr key-art image id `menu_keyart` and the title. Pressing `menu.play` for the first time plays the intro, then goes to Setup wave 1. Save `introSeen`; later plays skip it.
4. Dark-realm line and chapter-end lines already exist (tasks 11, 12): reuse the same text box style.

## Done when (the user checks)
First Play: 8 story beats with Arabic text, skip works, then the first Setup screen. Second Play: no intro. 0 red errors.

## Finish
Commit `feat: story intro`.

# Task 15 — Game-feel polish and tutorial hints
Read AGENTS.md and docs/decisions.md first. Do ONLY this task.

## Steps
1. Feedback: hit particles (procedural small sprites), dust on walking, death puff, coin pop sparkle, screen shake on gate hits and boss slam, ability button pulse when ready.
2. Tutorial hints in waves 1-2 only (shown once, saved): `tutorial.protect` at wave start with an arrow to the gate; `tutorial.move` (arrows left/right); `tutorial.haybah` when haybah first reaches a summon cost; `tutorial.slash` when the slash is first ready. Style: a scroll-like panel (`ui_panel_scroll` 9-slice placeholder), dismiss by tap or after 5 s.
3. Make sure no UI element overlaps the system safe area on notched phones.
4. Fix any warnings in the Console that come from our own code.

## Done when (the user checks)
The game feels responsive; hints appear once; 0 red errors and 0 warnings from our scripts.

## Finish
Commit `feat: polish and tutorial`.

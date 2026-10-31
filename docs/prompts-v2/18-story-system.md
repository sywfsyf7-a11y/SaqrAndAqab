# Task 18 — Story system and intro
Read AGENTS.md, docs/decisions.md, docs/v2/decisions-v2.md and docs/v2/screens-spec.md first.
FIRST inspect what already exists in the code for this topic and EXTEND it; never create duplicates. Do ONLY this task.

## Steps
1. `UI/StoryPlayer.cs`: data-driven from `Assets/Resources/Config/story.json` (scenes: id, trigger, beats: image, speaker key, textKey). Typewriter text, speaker name tag, Ken-Burns zoom on the image, tap = next, `common.skip`. Portraits optional (`npc_sage_salim`, `npc_nora`, `aqab_blue_entity`).
2. `Core/StoryTriggers.cs`: plays scenes at events `beforeWave`, `afterWaveVictory`, `afterDarkRealm`, `afterBoss`, `intro`, `ending` as in the triggers; every scene plays once (saved flags) except endings.
3. Intro (first Play): the 8 existing beats, with `intro_orb` inserted after `story.intro.7`.
4. Chapter title cards (`chapter.N`) before chapter scenes.

## Done when (the user checks)
First run plays the intro with the orb beat; later runs skip it. Jump to wave 11 (debug): the Barzakh scene plays. 0 red errors.

## Finish
Update docs/progress.md, `git add -A`, commit `feat: story system`, `git push`. Reply to the user in simple Arabic: what you did, files changed, how to verify. Ask at most ONE question and only if blocked.

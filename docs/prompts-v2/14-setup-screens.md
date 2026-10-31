# Task 14 — Setup screens (allies, abilities, charms)
Read AGENTS.md, docs/decisions.md, docs/v2/decisions-v2.md and docs/v2/screens-spec.md first.
FIRST inspect what already exists in the code for this topic and EXTEND it; never create duplicates. Do ONLY this task.

## Steps
Build exactly screens-spec S8. Three sequential pages: allies, abilities, charms; then start.
1. Left page: parchment panel with a scroll frame; grid of UNLOCKED items (icon + name); tap an item = description appears in a speech bubble on the right page and it is added to the first free slot in the row of round slots under the grid; tap a filled slot = removes it. Slot counts from `slots`.
2. Right page: preview scene (night cave with the portal glow, jinn standing), the wave number on a wooden sign, chapter title.
3. Bottom-left back arrow (`ui_back`), bottom-right green check (`ui_check`) goes to the next page; the last page starts the battle.
4. Charms page shows counts (x2) and disables charms with count 0. Charm effects from config; consumed at wave start.
5. Save the loadout.

## Done when (the user checks)
Run a wave: all three pages work, selections persist, charms are consumed. 0 red errors.

## Finish
Update docs/progress.md, `git add -A`, commit `feat: setup screens`, `git push`. Reply to the user in simple Arabic: what you did, files changed, how to verify. Ask at most ONE question and only if blocked.

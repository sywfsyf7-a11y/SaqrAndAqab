# Task 17 — Dark realm every 5 waves
Read AGENTS.md, docs/decisions.md, docs/v2/decisions-v2.md and docs/v2/screens-spec.md first.
FIRST inspect what already exists in the code for this topic and EXTEND it; never create duplicates. Do ONLY this task.

## Steps
1. Dark realm after every wave where `darkRealmAfter` is true (5,10,...,40), using the existing implementation (reuse, do not rewrite). Add: dinar reward (`dinar.perDarkRealm`), and a corruption increase of `corruption.perDarkRealmPercent` from chapter 2 on.
2. Memory hooks: after the dark realm of waves 15,20,25,30,35 play the memory scene listed in `story.json` (task 18) before the victory screen.
3. Counter `memoriesSeen` saved (0..5).

## Done when (the user checks)
Wave 5 and wave 15 both trigger the dark realm; from 15 on corruption rises and a memory scene follows. 0 red errors.

## Finish
Update docs/progress.md, `git add -A`, commit `feat: dark realm every five waves`, `git push`. Reply to the user in simple Arabic: what you did, files changed, how to verify. Ask at most ONE question and only if blocked.

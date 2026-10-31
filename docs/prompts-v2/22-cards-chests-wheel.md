# Task 22 — Hero cards, chests and wheel of fortune
Read AGENTS.md, docs/decisions.md, docs/v2/decisions-v2.md and docs/v2/screens-spec.md first.
FIRST inspect what already exists in the code for this topic and EXTEND it; never create duplicates. Do ONLY this task.

## Steps
1. Hero cards (`heroCards`): collected from chests; each owned card gives a permanent passive; the heroes tab shows the five cards.
2. Chests (`chests`): open with a short animation (shake, lid, light), show the result via `chest.got`.
3. Wheel (`wheel`): spin animation, weighted rewards, 1 free spin per wave won plus purchased spins.
4. No real money anywhere.

## Done when (the user checks)
Open both chests, spin the wheel, and own a card: the passive applies in battle. 0 red errors.

## Finish
Update docs/progress.md, `git add -A`, commit `feat: cards chests wheel`, `git push`. Reply to the user in simple Arabic: what you did, files changed, how to verify. Ask at most ONE question and only if blocked.

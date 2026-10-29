# Task 12 — Dark Realm ("طغيان البرزخ")
Read AGENTS.md and docs/decisions.md first. Do ONLY this task.

## Steps
1. After the VICTORY of waves listed in `game.darkRealmAfterWaves` (5 and 10), before the victory screen, play the Dark Realm stage (`darkRealm` in config).
2. Theme DarkRealm: dark purple/blue light, sprites `bg_dark_*`. Saqr uses `hero_saqr_blue`, floats (sine bob, raised 1 m off the ground) with a pulsing blue aura (scaled additive sprite or tinted copy).
3. Rules: hero invulnerable; haybah instantly full and allies cost 0; allies are summoned automatically every 3 s with `allyPowerMultiplier` stats; enemies spawn every `spawnInterval` with hp * `enemyHpMultiplier`; duration `durationSeconds` with a countdown; title `dark.title` + hint `dark.hint` at the start; one line `story.dark.1` shown at the start (speaker `speaker.entity`).
4. Rewards: dirhams collected in this stage are multiplied by `rewardMultiplier`; `guaranteedCharmDrops` charms are added at the end.
5. After it ends, everything returns to normal (hero sprite, theme, hp) and the normal Victory screen follows (task 09).

## Done when (the user checks)
Win wave 5: the dark stage plays ~35 s with the flying blue hero, enemies die easily, rewards are big; then the normal victory screen. 0 red errors.

## Finish
Commit `feat: dark realm`.

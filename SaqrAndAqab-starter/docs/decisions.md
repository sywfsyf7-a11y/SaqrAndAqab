# Design decisions (final — do not change without the user's request)

| # | Decision |
|---|----------|
| Genre | Hero action + fixed-point defense. Player controls hero Saqr; hero ATTACKS AUTOMATICALLY the nearest enemy in range. Between waves: setup, shop, upgrades. |
| Platform | Android first, landscape. Touch: hold LEFT or RIGHT half of the screen to move. Editor/PC: A/D or arrow keys. |
| Dimension | 2D sprites, 2.5D look: parallax background layers, units on a lane band with depth sorting by Y. |
| Animation | Procedural (code) on a single static sprite: bob, squash/stretch, lunge, hit flash, death fade, knockback. No sprite sheets, no skeletal rigs. |
| Sides | One side only (player = Saqr). Code uses a `Team` enum so a second side can be added later. |
| Language | Arabic only (Modern Standard, simplified). All text in strings_ar.json so other languages can be added later. RTL via RTLTMPro. |
| Setting | Mix: Arabian desert + oasis + One-Thousand-and-One-Nights jinn world. Day (golden desert) and Night (oasis with lanterns). |
| Story | Saqr falls from glowing ropes into the desert, memory lost, hunted by jinn. A mysterious "Blue Entity" saves him only to watch him suffer. SECRET: the Blue Entity is AQAB, Saqr's twin brother. His name is revealed only in the final chapter (not in this MVP). The game title already contains the name "Aqab". |
| MVP scope | 10 waves, boss on wave 10. Enemies: earth jinn, ghoul, ifrit, boss "Marid". Allies: palm farmer, bedouin archer. Abilities: crescent slash, arrow volley. 3 charms (hirz): power, luck, wealth. One currency: dirham. No ads, no real-money purchases, no revives. |
| Dark Realm ("Tughyan al-Barzakh") | After waves 5 and 10: a short bonus stage (~35 s). Saqr floats with a blue aura, invulnerable, free strong allies, weak enemies, triple rewards, guaranteed charm drops. No story cost yet (cost is added in chapter 2). |
| Replacements for non-Arab items | sushi -> dates (tamr), tea -> Arabic coffee, torii gate -> fortress gate, leadership -> "haybah" (prestige), coins -> dirham, omamori -> hirz. |
| Build approach | Everything built from code at runtime. Placeholder art first (coloured shapes), real art swapped in later by file name. |
| Repo | https://github.com/sywfsyf7-a11y/SaqrAndAqab (PUBLIC). Git LFS for images/audio. |
| Model used to build | DeepSeek V4.1 Flash (free) in OpenCode desktop. Fallback: GLM-5.3-Flash. Tasks are small on purpose. |

## Layout (HUD) — our own arrangement
Top-left: hero portrait + HP bar. Top-center: wave progress bar. Top-right: dirhams + pause.
Bottom-left: haybah meter + ally buttons. Bottom-right: ability buttons. Bottom-center: date (healing) button.

## World constants
1 unit = 1 meter. Orthographic camera size 5.4 (16:9). Gate at the LEFT, jinn cave portal at the RIGHT. Enemies walk right -> left.

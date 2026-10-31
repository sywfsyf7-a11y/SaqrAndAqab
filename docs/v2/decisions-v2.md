# Decisions v2 (supersedes the MVP limits in decisions.md)
| Topic | Decision |
|---|---|
| Scope | FULL game: 40 waves in 4 chapters, bosses at 10/20/30/40, dark realm after every 5th wave, 3 endings. |
| Content | 11 enemy types, 8 allies, 8 abilities, 6 charms (hirz), 5 hero cards, 2 chests, wheel of fortune. |
| Currencies | Dirham (earned in battle) and dinar (bosses, dark realms, wheel). No real money, no ads, no VIP. |
| Story | See docs/v2/story-bible.md. Scenes are data in Assets/Resources/Config/story.json. |
| Corruption | The forbidden-energy meter starts in chapter 2; it decides whether the secret ending is possible. |
| Config | docs/v2/balance_full.json replaces balance.json in task 10. strings_ar.json and assets.json are already extended. |
| Visual bar | No grey squares, no flat blue blocks: procedural fallback art for everything (task 09). |
| Tasks | Old tasks 00-08 stay done. Old tasks 09-17 are OBSOLETE; use docs/prompts-v2/09..26. |

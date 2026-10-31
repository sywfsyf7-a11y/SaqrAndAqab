# Task 13 — Main menu and wave banner
Read AGENTS.md, docs/decisions.md, docs/v2/decisions-v2.md and docs/v2/screens-spec.md first.
FIRST inspect what already exists in the code for this topic and EXTEND it; never create duplicates. Do ONLY this task.

## Steps
1. Main menu per screens-spec S1: full-screen background `menu_silhouettes` (procedural fallback: dark orange sunset with silhouettes and glowing red eyes), logo text `app.title` large with a gold outline, hero art (`hero_saqr`) in the centre, three big wooden buttons at the bottom: `menu.options` (left), `menu.play` / `menu.continue` (centre, biggest), `menu.heroes` (right). Small chest and wheel shortcut buttons top-right.
2. Wave banner (screens-spec S2): when a wave starts, the screen dims, `ui_banner_wave` slides in with `wave.banner` in glowing blue and `hud.wave` under it; auto-hide after 2 s.
3. All texts via Loc + RTLTMPro, all frames via UIKit.

## Done when (the user checks)
Menu and wave banner look like screens-spec S1/S2, buttons work, no overlap. 0 red errors.

## Finish
Update docs/progress.md, `git add -A`, commit `feat: menu and wave banner`, `git push`. Reply to the user in simple Arabic: what you did, files changed, how to verify. Ask at most ONE question and only if blocked.

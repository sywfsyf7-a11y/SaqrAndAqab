# برومبتات الصور (araby.ai و Flow)

## قبل ما تبدأ

1. **ولّد شخصية صقر أولاً.** لما تعجبك الصورة، استخدمها كـ **صورة مرجعية** (Reference) لكل الشخصيات التالية حتى يبقى الأسلوب موحداً.
2. كل صورة تحفظها باسمها بالضبط: `hero_saqr.png` (الاسم بالعمود الأول) في المجلد `Assets/Resources/Art/`. اللعبة بتلتقطها تلقائياً.
3. الشخصيات والعناصر على **خلفية بيضاء سادة**. بعدها امسح الخلفية (remove.bg، أو Photopea، أو أداة إزالة الخلفية بـ Flow/araby إن وجدت) وصدّر **PNG شفاف**.
4. الخلفيات الأربع `*_sky` ما بتحتاج حذف خلفية. أما `*_far` و `ground_*` فبتحتاج. اللعبة بتكرر الخلفيات العريضة بالقلب (Mirror) فما يهم لو الحواف ما تتطابق.
5. **لا تكتب اسم أي لعبة أخرى بالبرومبت أبداً** (لا Samurai ولا Zombies). نحن نبني هوية عربية خاصة.
6. اجعل الأعداء تواجه **اليسار**، والبطل والحلفاء تواجه **اليمين** (مكتوب بكل برومبت).
7. إذا ما أعطتك الأداة الحجم المطلوب، لا مشكلة: النسبة (العرض إلى الطول) هي الأهم. اللعبة بتضبط الحجم.

## الأسلوب العام (ألصقه مع كل برومبت)

```
Stylized 2D hand-painted game art, Arabian Nights desert fantasy, chunky readable silhouette, thick warm dark-brown outlines, saturated warm palette (sand gold, terracotta, deep teal, indigo night), slightly exaggerated chibi proportions (head about one third of the body), clean cel shading, no text, no watermark, no logo
```

## المرحلة 1 — ضروري أولاً

| الملف | الحجم | البرومبت |
|---|---|---|
| `hero_saqr.png` | 1024x1024 | young Arab desert warrior named Saqr, lean athletic, short dark beard, cream and red keffiyeh headscarf with black agal ring, indigo tunic with leather armor pieces and a red sash, holding a curved scimitar in the right hand, determined fierce face; facing RIGHT; full body, side view, neutral pose, centered, nothing cropped, on a plain solid white background |
| `ally_farmer.png` | 1024x1024 | humble date-palm farmer, earthy brown thobe, wrapped turban, carrying a basket of dates and a small sickle, kind worried face; facing RIGHT; full body, side view, neutral pose, centered, nothing cropped, on a plain solid white background |
| `enemy_jinn_earth.png` | 1024x1024 | small mischievous earth jinn, cracked clay-brown skin, glowing orange eyes, tiny horns, pointed ears, ragged loincloth, claws, hunched aggressive pose; facing LEFT; full body, side view, neutral pose, centered, nothing cropped, on a plain solid white background |
| `enemy_ghoul.png` | 1024x1024 | large hulking ghoul from Arabian folklore, grey-green rotting skin, hunched with very long arms, ragged cloth, sharp teeth, bone necklace, menacing; facing LEFT; full body, side view, neutral pose, centered, nothing cropped, on a plain solid white background |
| `prop_gate.png` | 2048x1024 | imposing mud-brick desert fortress gate with two watch towers, reinforced wooden door, red and gold banners, Arabian architecture, front orthographic view; on a plain solid white background |
| `prop_cave.png` | 2048x1024 | dark cave mouth in a rocky desert cliff with a glowing swirling blue portal inside, ominous; on a plain solid white background |
| `bg_day_sky.png` | 1920x1080 | golden-hour desert, warm sky, sky only, no ground, no characters, wide landscape, soft clouds |
| `bg_day_far.png` | 3840x1080 | golden-hour desert, warm sky: distant layer ONLY: far dunes, faint mountains, distant mud-brick town and domes in silhouette and haze; PANORAMIC 3.5:1; on a plain solid white background (it will be cut out) |
| `ground_day.png` | 3840x540 | golden-hour desert, warm sky: foreground ground strip, packed sand with small rocks and grass tufts, top edge irregular, flat side view, PANORAMIC 7:1; on a plain solid white background |
| `fx_arrow.png` | 512x128 | single arrow with feather fletching pointing RIGHT; on a plain solid white background |
| `fx_crescent.png` | 1024x1024 | large glowing white-gold crescent slash arc, sword energy wave, bright core and soft edges; on a plain solid black background |
| `ui_dirham.png` | 512x512 | silver dirham coin with engraved geometric star pattern; game UI icon, centered, on a plain solid white background |
| `ui_date.png` | 512x512 | single plump Medjool date fruit on a tiny plate; game UI icon, centered, on a plain solid white background |
| `ui_haybah.png` | 512x512 | golden eagle-wing emblem on a round shield; game UI icon, centered, on a plain solid white background |
| `ui_icon_slash.png` | 512x512 | curved scimitar with a bright crescent slash swoosh, round icon; game UI icon, centered, on a plain solid white background |
| `ui_icon_bow.png` | 512x512 | bow with three arrows, round icon; game UI icon, centered, on a plain solid white background |
| `ui_hirz_power.png` | 512x512 | leather amulet (hirz) with embroidered red fist symbol; game UI icon, centered, on a plain solid white background |
| `ui_hirz_luck.png` | 512x512 | leather amulet (hirz) with embroidered green clover-like star symbol; game UI icon, centered, on a plain solid white background |
| `ui_hirz_wealth.png` | 512x512 | leather amulet (hirz) with embroidered gold coin symbol; game UI icon, centered, on a plain solid white background |
| `ui_ally_farmer.png` | 512x512 | round portrait icon of the date-palm farmer; game UI icon, centered, on a plain solid white background |
| `ui_ally_archer.png` | 512x512 | round portrait icon of the Bedouin archer; game UI icon, centered, on a plain solid white background |
| `ui_portrait_saqr.png` | 512x512 | round portrait icon of Saqr the warrior, angry determined face; game UI icon, centered, on a plain solid white background |

## المرحلة 2

| الملف | الحجم | البرومبت |
|---|---|---|
| `ally_archer.png` | 1024x1024 | Bedouin archer, headscarf with face wrap, desert brown and white clothes, long recurve bow in hand and quiver on back, agile stance; facing RIGHT; full body, side view, neutral pose, centered, nothing cropped, on a plain solid white background |
| `enemy_ifrit.png` | 1024x1024 | ifrit fire jinn floating, body of embers and smoke, glowing red-orange, curved horns, golden bracelets, raising a fireball in one hand; facing LEFT; full body, side view, neutral pose, centered, nothing cropped, on a plain solid white background |
| `boss_marid.png` | 2048x2048 | colossal marid, giant jinn lord, dark indigo skin, golden shackles and ornate armor, broken chains, four curved horns, glowing blue eyes, enormous fists, smoke instead of legs, imposing; facing LEFT; full body, side view, neutral pose, centered, nothing cropped, on a plain solid white background |
| `bg_night_sky.png` | 1920x1080 | moonlit oasis night with blue sky and stars, fireflies, sky only, no ground, no characters, wide landscape, soft clouds |
| `bg_night_far.png` | 3840x1080 | moonlit oasis night with blue sky and stars, fireflies: distant layer ONLY: far dunes, faint mountains, distant mud-brick town and domes in silhouette and haze; PANORAMIC 3.5:1; on a plain solid white background (it will be cut out) |
| `ground_night.png` | 3840x540 | moonlit oasis night with blue sky and stars, fireflies: foreground ground strip, packed sand with small rocks and grass tufts, top edge irregular, flat side view, PANORAMIC 7:1; on a plain solid white background |
| `fx_fireball.png` | 512x512 | small fireball, orange-yellow flames; on a plain solid white background |
| `ui_panel_scroll.png` | 1024x512 | aged parchment scroll banner with rolled ends and subtle Arabic geometric border, empty inside, no text |
| `ui_button.png` | 512x256 | ornate wooden button with brass corners, empty, no text |
| `menu_keyart.png` | 1920x1080 | title key art: Saqr the Arab warrior hanging from glowing ropes of light in a dark sky, arms pulled up, desert far below, dramatic, empty space at the top for the title |

## المرحلة 3 — لاحقاً

| الملف | الحجم | البرومبت |
|---|---|---|
| `hero_saqr_blue.png` | 1024x1024 | the SAME warrior Saqr floating in the air with relaxed legs, electric-blue glowing aura around the whole body, glowing blue eyes, glowing scimitar, tattered cloak flowing upward, dynamic pose; facing RIGHT; full body, side view, neutral pose, centered, nothing cropped, on a plain solid white background |
| `aqab_blue_entity.png` | 1024x1024 | mysterious tall figure with a face that resembles a young Arab man, translucent glowing blue skin, dark robes with silver geometric Arabic patterns, two small curved horns of light, calm mocking smirk, floating slightly; facing LEFT; full body, side view, neutral pose, centered, nothing cropped, on a plain solid white background |
| `story_portrait_entity.png` | 1024x1024 | close-up bust of the same blue entity, glowing blue skin, mocking half-smile, horns of light, dark background glow; no text |
| `prop_lantern.png` | 512x512 | ornate Arabian brass lantern standing on the ground, warm glowing light; on a plain solid white background |
| `prop_palm.png` | 1024x1024 | date palm tree with dates, desert oasis style; on a plain solid white background |
| `bg_dark_sky.png` | 1920x1080 | eerie purple-black otherworld sky with swirling blue energy, sky only, no ground, no characters, wide landscape, soft clouds |
| `bg_dark_far.png` | 3840x1080 | eerie purple-black otherworld sky with swirling blue energy: distant layer ONLY: far dunes, faint mountains, distant mud-brick town and domes in silhouette and haze; PANORAMIC 3.5:1; on a plain solid white background (it will be cut out) |
| `ground_dark.png` | 3840x540 | eerie purple-black otherworld sky with swirling blue energy: foreground ground strip, packed sand with small rocks and grass tufts, top edge irregular, flat side view, PANORAMIC 7:1; on a plain solid white background |
| `story_01_ropes.png` | 1920x1080 | Saqr hanging from glowing ropes of light high in the night sky, cinematic painted illustration, wide landscape |
| `story_02_fall.png` | 1920x1080 | the ropes snapping, Saqr falling toward a vast desert, motion lines, cinematic painted illustration, wide landscape |
| `story_03_chase.png` | 1920x1080 | Saqr running across dunes chased by a pack of jinn and ghouls, one throwing a fireball, cinematic painted illustration, wide landscape |
| `story_04_cave.png` | 1920x1080 | exhausted Saqr entering a dark cave while monsters approach behind him, blue glow deep inside, cinematic painted illustration, wide landscape |
| `story_05_entity.png` | 1920x1080 | the blue glowing entity standing calmly at the cave entrance, hand raised to stop the monsters, cinematic painted illustration, wide landscape |
| `story_06_slam.png` | 1920x1080 | the blue entity slamming the ground, a shockwave blasting the monsters away, cinematic painted illustration, wide landscape |
| `story_07_entity.png` | 1920x1080 | the blue entity pouting mockingly with hand on chest, monsters cowering, cinematic painted illustration, wide landscape |
| `story_08_alone.png` | 1920x1080 | Saqr alone in the cave catching his breath, faint blue glow on his face, cinematic painted illustration, wide landscape |

## ملاحظة
الأسلوب العام يُلصق قبل كل برومبت، مثال: `Stylized 2D hand-painted game art, Arabian Nights desert fan... + (برومبت الصورة)`.
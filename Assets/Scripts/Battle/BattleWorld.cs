using UnityEngine;
using SaqrAndAqab.Core;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.Battle
{
    // Builds the side-view battle world from code: sky + far + ground bands, gate, cave and the hero.
    // The scene stays empty; this object owns everything it creates.
    public class BattleWorld : MonoBehaviour
    {
        const int SkyOrder = -1000;
        const int FarOrder = -900;
        const int GroundOrder = -800;
        const int PropOrder = -500;

        const float FarFactor = 0.3f;
        const float GroundTileWidth = 6f;
        const float PropSink = 0.35f;
        const float HeroStartOffset = 2.5f;
        const float CameraSmoothTime = 0.18f;

        // Placeholder-era haze so the three placeholder bands are easy to tell apart.
        // With real art the far layer tint goes back to opaque white.
        static readonly Color FarTint = new Color(0.78f, 0.84f, 0.96f, 0.60f);
        static readonly Color GroundTint = new Color(1f, 0.93f, 0.80f, 1f);

        Camera _camera;
        Hero _hero;
        Gate _gate;
        EnemySpawner _spawner;
        WaveManager _waves;
        HaybahMeter _haybah;
        CameraShake _shake;
        GameConfig _game;
        readonly System.Collections.Generic.List<AllyConfig> _selectedAllies =
            new System.Collections.Generic.List<AllyConfig>();
        readonly System.Collections.Generic.List<Ability> _abilities =
            new System.Collections.Generic.List<Ability>();
        ParallaxLayer _sky;
        ParallaxLayer _far;
        ParallaxLayer _ground;

        float _camMinX;
        float _camMaxX;
        float _camY;
        float _camVelocity;

        public WorldThemeId Theme { get; private set; }

        public Hero Hero { get { return _hero; } }

        public Gate Gate { get { return _gate; } }

        public WaveManager Waves { get { return _waves; } }

        public HaybahMeter Haybah { get { return _haybah; } }

        public GameConfig Game { get { return _game; } }

        public CameraShake Shake { get { return _shake; } }

        // Allies chosen for this battle (temporary list until the setup screen in task 09).
        public System.Collections.Generic.List<AllyConfig> SelectedAllies { get { return _selectedAllies; } }

        // Abilities chosen for this battle (temporary list until the setup screen in task 09).
        public System.Collections.Generic.List<Ability> Abilities { get { return _abilities; } }

        // Uses an ability if the hero is alive and the cooldown is ready.
        public bool TryUseAbility(Ability ability)
        {
            if (ability == null || _hero == null || _hero.Health == null || !_hero.Health.IsAlive) return false;
            return ability.TryUse();
        }

        // Spends the ally cost and spawns it near the gate.
        public bool TrySpawnAlly(AllyConfig config)
        {
            if (config == null || _hero == null || _game == null) return false;
            if (_haybah == null || !_haybah.TrySpend(config.haybahCost)) return false;

            float x = _game.gateX + 1f + Random.Range(0f, 3f);
            x = Mathf.Clamp(x, _game.worldMinX + 1f, _game.worldMaxX - 1f);
            float y = (_game.laneMinY + _game.laneMaxY) * 0.5f;

            Ally.Create(transform, config, new Vector3(x, y, 0f), _hero, _waves, _game);
            return true;
        }

        void Update()
        {
            // Hit-stop runs on unscaled time so it always ends, even at timeScale 0.
            HitStop.Tick();

            float dt = Time.deltaTime;
            for (int i = 0; i < _abilities.Count; i++)
            {
                if (_abilities[i] != null) _abilities[i].Tick(dt);
            }

            // Debug keys (editor only, task 04 + 05).
            if (InputService.DebugSpawnDummyPressed) DebugSpawnDummy();

            if (InputService.DebugNextWavePressed && _waves != null) _waves.StartNextWave();

            if (InputService.DebugKillAllPressed && _waves != null) _waves.KillAllEnemies();

            if (InputService.DebugJumpToWave10Pressed && _waves != null) _waves.StartWave(10);

            if (InputService.DebugJumpToWave10Pressed && _waves != null) _waves.StartWave(10);
        }

        public void DebugSpawnDummy()
        {
            if (_hero == null) return;

            var stats = new UnitStats();
            stats.hp = 50;
            stats.damage = 0;
            stats.range = 1.6f;
            stats.cooldown = 60f;
            stats.speed = 0f;

            var go = new GameObject("TrainingDummy", typeof(Unit), typeof(SpriteRenderer));
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3(_hero.transform.position.x + 1.2f, _hero.transform.position.y, 0f);

            // Visible stand-in so the dummy can be seen while the real enemy art is still a placeholder.
            const string DummySpriteId = "enemy_jinn_earth";
            var sprite = AssetLibrary.GetSprite(DummySpriteId);
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.flipX = true;
            renderer.sortingOrder = Hero.SortingOrderFor(go.transform.position.y);

            float spriteHeight = sprite != null && sprite.bounds.size.y > 0f ? sprite.bounds.size.y : 1f;
            float scale = AssetLibrary.GetWorldHeight(DummySpriteId) / spriteHeight;
            go.transform.localScale = new Vector3(scale, scale, 1f);

            var dummy = go.GetComponent<Unit>();
            var anim = go.AddComponent<ProceduralAnim>();
            dummy.Init(Team.Enemy, stats);
            anim.SetBaseScale(Vector3.one * scale);
        }

        public static BattleWorld Build(Transform parent, WorldThemeId theme)
        {
            return Build(parent, theme, null);
        }

        public static BattleWorld Build(Transform parent, WorldThemeId theme, BattleSetup setup)
        {
            var go = new GameObject("Battle World");
            if (parent != null) go.transform.SetParent(parent, false);

            var world = go.AddComponent<BattleWorld>();
            world.BuildInternal(theme, setup);
            return world;
        }

        void BuildInternal(WorldThemeId theme, BattleSetup setup)
        {
            Theme = theme;

            // Never keep stale units or coins from a previous battle.
            UnitRegistry.Clear();
            CoinPickups.Clear();

            var balance = ConfigService.Balance;
            var game = balance != null ? balance.game : null;
            var heroConfig = balance != null ? balance.hero : null;
            if (game == null || heroConfig == null)
            {
                Debug.LogError("[Battle] balance.json is not loaded, the world cannot be built.");
                return;
            }

            _game = game;

            _camera = Camera.main;
            if (_camera == null)
            {
                Debug.LogError("[Battle] No main camera, the world cannot be built.");
                return;
            }

            WorldTheme.Apply(theme, _camera, transform);

            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;
            _camY = 0f;
            _camera.transform.position = new Vector3(_camera.transform.position.x, _camY, _camera.transform.position.z);

            _camMinX = game.worldMinX + halfWidth;
            _camMaxX = game.worldMaxX - halfWidth;
            if (_camMinX > _camMaxX)
            {
                float middle = (game.worldMinX + game.worldMaxX) * 0.5f;
                _camMinX = middle;
                _camMaxX = middle;
            }

            float bottom = _camY - halfHeight;

            // Sky is glued to the camera, far moves at 30% and the ground moves with the world.
            _sky = CreateLayer("Sky", WorldTheme.SkySprite(theme), 0f, bottom,
                -halfWidth, halfWidth, SkyOrder, Color.white, 0f, 0f);

            _far = CreateLayer("Far", WorldTheme.FarSprite(theme), FarFactor, bottom,
                _camMinX * FarFactor - halfWidth, _camMaxX * FarFactor + halfWidth, FarOrder, FarTint, 0f, 0f);

            // The ground is stretched from the back edge of the lane down to the bottom of the screen.
            float groundTop = game.laneMaxY;
            _ground = CreateLayer("Ground", WorldTheme.GroundSprite(theme), 1f, bottom,
                game.worldMinX, game.worldMaxX, GroundOrder, GroundTint, GroundTileWidth, groundTop - bottom);

            CreateProp("Gate", "prop_gate", game.gateX, game.laneMinY - PropSink);
            CreateProp("Cave", "prop_cave", game.caveX, game.laneMinY - PropSink);

            float startX = Mathf.Clamp(game.gateX + HeroStartOffset, game.worldMinX + 1f, game.worldMaxX - 1f);
            float startY = (game.laneMinY + game.laneMaxY) * 0.5f;
            _hero = Hero.Create(transform, new Vector3(startX, startY, 0f), game, heroConfig);

            // The charm of power applies to the hero (melee and abilities) for the whole wave.
            if (setup != null) _hero.DamageMultiplier = setup.damageMultiplier;

            int gateHp = balance.gate != null ? balance.gate.hp : 0;
            _gate = Gate.Create(transform, new Vector3(game.gateX, startY, 0f), gateHp);
            _spawner = EnemySpawner.Create(transform, game);
            _waves = WaveManager.Create(transform, balance.waves, _spawner, _gate, _hero);
            _haybah = HaybahMeter.Create(transform);
            _shake = CameraShake.Create(transform);

            // The charm of wealth multiplies every coin collected during the wave.
            if (setup != null) _waves.SetDirhamMultiplier(setup.dirhamMultiplier);

            BuildSelectedAllies(balance, setup);
            BuildAbilities(balance, setup);

            SnapCamera();
        }

        // Swaps the background theme at runtime (used by the Dark Realm).
        public void ApplyTheme(WorldThemeId theme)
        {
            Theme = theme;
            if (_camera != null) WorldTheme.Apply(theme, _camera, transform);

            if (_sky != null) { UnityEngine.Object.Destroy(_sky.gameObject); _sky = null; }
            if (_far != null) { UnityEngine.Object.Destroy(_far.gameObject); _far = null; }
            if (_ground != null) { UnityEngine.Object.Destroy(_ground.gameObject); _ground = null; }

            if (_camera == null || _game == null) return;

            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;
            float bottom = _camY - halfHeight;

            _sky = CreateLayer("Sky", WorldTheme.SkySprite(theme), 0f, bottom,
                -halfWidth, halfWidth, SkyOrder, Color.white, 0f, 0f);

            _far = CreateLayer("Far", WorldTheme.FarSprite(theme), FarFactor, bottom,
                _camMinX * FarFactor - halfWidth, _camMaxX * FarFactor + halfWidth, FarOrder, FarTint, 0f, 0f);

            float groundTop = _game.laneMaxY;
            _ground = CreateLayer("Ground", WorldTheme.GroundSprite(theme), 1f, bottom,
                _game.worldMinX, _game.worldMaxX, GroundOrder, GroundTint, GroundTileWidth, groundTop - bottom);
        }

        // The loadout chosen on the setup screen, capped by slots.allies.
        void BuildSelectedAllies(BalanceConfig balance, BattleSetup setup)
        {
            _selectedAllies.Clear();
            if (balance == null || balance.allies == null) return;

            int slots = balance.slots != null ? balance.slots.allies : 0;
            var ids = setup != null && setup.allyIds.Count > 0 ? setup.allyIds : null;

            if (ids != null)
            {
                for (int i = 0; i < ids.Count && _selectedAllies.Count < slots; i++)
                {
                    var config = FindAlly(balance, ids[i]);
                    if (config != null) _selectedAllies.Add(config);
                }

                if (_selectedAllies.Count > 0) return;
            }

            // No setup (debug build) or an empty loadout: fall back to the default allies.
            for (int i = 0; i < Ally.DefaultIds.Length && _selectedAllies.Count < slots; i++)
            {
                string id = Ally.DefaultIds[i];
                for (int j = 0; j < balance.allies.Count; j++)
                {
                    var config = balance.allies[j];
                    if (config != null && config.id == id)
                    {
                        _selectedAllies.Add(config);
                        break;
                    }
                }
            }
        }

        // The loadout chosen on the setup screen, capped by slots.abilities.
        void BuildAbilities(BalanceConfig balance, BattleSetup setup)
        {
            _abilities.Clear();
            if (balance == null || balance.abilities == null) return;

            int slots = balance.slots != null ? balance.slots.abilities : 0;
            var ids = setup != null && setup.abilityIds.Count > 0 ? setup.abilityIds : null;

            if (ids != null)
            {
                for (int i = 0; i < ids.Count && _abilities.Count < slots; i++)
                {
                    var config = FindAbility(balance, ids[i]);
                    if (config == null) continue;

                    var ability = Ability.Create(config, this);
                    if (ability != null) _abilities.Add(ability);
                }

                if (_abilities.Count > 0) return;
            }

            // No setup (debug build) or an empty loadout: fall back to the default abilities.
            for (int i = 0; i < Ability.DefaultIds.Length && _abilities.Count < slots; i++)
            {
                var config = FindAbility(balance, Ability.DefaultIds[i]);
                if (config == null) continue;

                var ability = Ability.Create(config, this);
                if (ability != null) _abilities.Add(ability);
            }
        }

        static AllyConfig FindAlly(BalanceConfig balance, string id)
        {
            for (int i = 0; i < balance.allies.Count; i++)
            {
                var config = balance.allies[i];
                if (config != null && config.id == id) return config;
            }

            return null;
        }

        static AbilityConfig FindAbility(BalanceConfig balance, string id)
        {
            for (int i = 0; i < balance.abilities.Count; i++)
            {
                var config = balance.abilities[i];
                if (config != null && config.id == id) return config;
            }

            return null;
        }

        void OnDestroy()
        {
            // Leaving the battle must never keep the game frozen by a hit-stop.
            HitStop.Cancel();
        }

        void LateUpdate()
        {
            if (_camera == null || _hero == null) return;

            float camX = Mathf.SmoothDamp(_camera.transform.position.x, _hero.transform.position.x,
                ref _camVelocity, CameraSmoothTime);
            camX = Mathf.Clamp(camX, _camMinX, _camMaxX);
            ApplyCameraX(camX);
        }

        void SnapCamera()
        {
            if (_camera == null || _hero == null) return;

            _camVelocity = 0f;
            ApplyCameraX(Mathf.Clamp(_hero.transform.position.x, _camMinX, _camMaxX));
        }

        void ApplyCameraX(float camX)
        {
            var position = _camera.transform.position;
            Vector2 shake = _shake != null ? _shake.Offset : Vector2.zero;
            _camera.transform.position = new Vector3(camX + shake.x, _camY + shake.y, position.z);

            if (_sky != null) _sky.Refresh(camX);
            if (_far != null) _far.Refresh(camX);
            if (_ground != null) _ground.Refresh(camX);
        }

        ParallaxLayer CreateLayer(string name, string spriteId, float factor, float bottomY,
            float spanMinX, float spanMaxX, int sortingOrder, Color tint, float tileWidth, float height)
        {
            var sprite = AssetLibrary.GetSprite(spriteId);
            if (sprite == null)
            {
                Debug.LogWarning("[Battle] Missing sprite: " + spriteId);
                return null;
            }

            float worldHeight = height > 0f ? height : AssetLibrary.GetWorldHeight(spriteId);
            float copyWidth = tileWidth > 0f ? tileWidth : UniformWidth(sprite, worldHeight);

            var go = new GameObject(name, typeof(ParallaxLayer));
            go.transform.SetParent(transform, false);

            var layer = go.GetComponent<ParallaxLayer>();
            layer.Build(_camera.transform, factor, sprite, new Vector2(copyWidth, worldHeight),
                bottomY, spanMinX, spanMaxX, sortingOrder, tint);
            return layer;
        }

        void CreateProp(string name, string spriteId, float x, float baseY)
        {
            var sprite = AssetLibrary.GetSprite(spriteId);
            if (sprite == null)
            {
                Debug.LogWarning("[Battle] Missing sprite: " + spriteId);
                return;
            }

            float worldHeight = AssetLibrary.GetWorldHeight(spriteId);
            float spriteHeight = sprite.bounds.size.y > 0f ? sprite.bounds.size.y : 1f;
            float scale = worldHeight / spriteHeight;
            float pivotY = sprite.rect.height > 0f ? sprite.pivot.y / sprite.rect.height : 0.5f;

            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(transform, false);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            go.transform.position = new Vector3(x, baseY + worldHeight * pivotY, 0f);

            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = PropOrder;
        }

        static float UniformWidth(Sprite sprite, float height)
        {
            float spriteHeight = sprite.bounds.size.y;
            if (spriteHeight <= 0f) return height;
            return sprite.bounds.size.x * (height / spriteHeight);
        }
    }
}

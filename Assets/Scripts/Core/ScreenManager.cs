using System.Collections.Generic;
using TMPro;
using UnityEngine;
using SaqrAndAqab.Battle;
using SaqrAndAqab.UI;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.Core
{
    public enum Screen
    {
        Menu,
        Story,
        Setup,
        Battle,
        Victory,
        Defeat,
        Shop,
        DarkRealm,
        Settings
    }

    public abstract class GameScreen
    {
        public virtual void Enter() { }
        public virtual void Exit() { }
    }

    // Temporary menu: title + play button. The real menu comes in task 10.
    public class MenuScreen : GameScreen
    {
        static readonly Color TitleColor = new Color(0.95f, 0.85f, 0.60f, 1f);

        RectTransform _root;

        public override void Enter()
        {
            if (_root == null) _root = Build();
            if (_root != null) _root.gameObject.SetActive(true);
        }

        public override void Exit()
        {
            if (_root != null) _root.gameObject.SetActive(false);
        }

        static RectTransform Build()
        {
            var canvas = GameBootstrap.UICanvas;
            if (canvas == null)
            {
                Debug.LogError("[Menu] No UI canvas.");
                return null;
            }

            var root = UIFactory.CreateUIRoot(canvas, "Menu UI");

            var title = UIFactory.CreateText(root, "app.title", 140, TitleColor, TextAlignmentOptions.Center, true);
            UIFactory.Place(title.rectTransform, new Vector2(0f, 230f), new Vector2(1500f, 320f));

            var button = UIFactory.CreateButton(root, "menu.play", OnPlayPressed);
            UIFactory.Place(button.GetComponent<RectTransform>(), new Vector2(0f, -140f), new Vector2(460f, 120f));

            return root;
        }

        static void OnPlayPressed()
        {
            GameFlow.BeginRun();
            ScreenManager.Show(Screen.Setup);
        }
    }

    // Battle: the world plus the HUD. The wave comes from GameFlow.
    public class BattleScreen : GameScreen
    {
        BattleWorld _world;
        BattleHud _hud;
        BattleSetup _setup;

        public override void Enter()
        {
            Time.timeScale = 1f;
            if (SaveService.Data == null) SaveService.Load();

            _setup = BattleSetup.CreateForWave(GameFlow.Wave);
            GameFlow.CharmDropBonus = _setup != null ? _setup.charmDropBonus : 0f;

            if (_world == null) _world = BattleWorld.Build(GameBootstrap.Root, WorldThemeId.Day, _setup);
            if (_hud == null) _hud = BattleHud.Create(GameBootstrap.UICanvas, _world);
            if (_hud != null) _hud.gameObject.SetActive(true);

            if (_world != null && _world.Waves != null)
            {
                _world.Waves.OnWaveComplete += HandleWaveComplete;
                _world.Waves.OnDefeat += HandleDefeat;
                _world.Waves.StartWave(_setup != null ? _setup.wave : GameFlow.Wave);
            }
        }

        public override void Exit()
        {
            Time.timeScale = 1f;
            HitStop.Cancel();

            if (_world != null && _world.Waves != null)
            {
                _world.Waves.OnWaveComplete -= HandleWaveComplete;
                _world.Waves.OnDefeat -= HandleDefeat;
            }

            if (_world != null)
            {
                Object.Destroy(_world.gameObject);
                _world = null;
            }

            if (_hud != null)
            {
                Object.Destroy(_hud.gameObject);
                _hud = null;
            }

            _setup = null;
        }

        void HandleWaveComplete(int wave)
        {
            GameFlow.WaveWon(wave, _world != null && _world.Waves != null ? _world.Waves.RunDirhams : 0);
            ScreenManager.Show(Screen.Victory);
        }

        void HandleDefeat()
        {
            GameFlow.WaveLost(_world != null && _world.Waves != null ? _world.Waves.DefeatReason : null);
            ScreenManager.Show(Screen.Defeat);
        }
    }

    // Setup screen: pick the wave's loadout (task 09).
    public class SetupScreen : GameScreen
    {
        SetupScreenView _view;

        public override void Enter()
        {
            Time.timeScale = 1f;
            if (GameBootstrap.UICanvas == null) return;

            _view = SetupScreenView.Build(GameBootstrap.UICanvas, GameFlow.Wave, OnStartPressed);
        }

        public override void Exit()
        {
            if (_view != null)
            {
                _view.Destroy();
                _view = null;
            }
        }

        static void OnStartPressed()
        {
            ScreenManager.Show(Screen.Battle);
        }
    }

    // Victory screen (task 09).
    public class VictoryScreen : GameScreen
    {
        VictoryScreenView _view;

        public override void Enter()
        {
            Time.timeScale = 1f;
            if (SaveService.Data == null) SaveService.Load();

            int wave = GameFlow.LastWave > 0 ? GameFlow.LastWave : GameFlow.Wave;
            var balance = ConfigService.Balance;

            int award = 0;
            if (balance != null && balance.waves != null)
            {
                for (int i = 0; i < balance.waves.Count; i++)
                {
                    var w = balance.waves[i];
                    if (w != null && w.wave == wave) { award = w.victoryAward; break; }
                }
            }

            int collected = GameFlow.LastCollectedDirhams;

            if (wave > SaveService.Data.highestWaveCleared) SaveService.Data.highestWaveCleared = wave;

            var unlocked = LoadoutService.ApplyUnlocks(wave);

            var charm = LoadoutService.RollCharmDrop(GameFlow.CharmDropBonus);
            if (charm != null) LoadoutService.AddCharm(charm.id, LoadoutService.CharmDropAmount());

            // The charm used in this wave is gone once the battle ends.
            SaveService.Data.loadoutCharms.Clear();

            int total = award + collected;
            SaveService.Data.dirhams += total;
            SaveService.Save();

            if (GameBootstrap.UICanvas != null)
            {
                _view = VictoryScreenView.Build(GameBootstrap.UICanvas, wave, award, collected, total,
                    charm, unlocked, OnNextPressed);
            }
        }

        public override void Exit()
        {
            if (_view != null)
            {
                _view.Destroy();
                _view = null;
            }
        }

        // Task 10 will build the shop; until then go straight to the next setup.
        static void OnNextPressed()
        {
            GameFlow.NextWave();
            ScreenManager.Show(Screen.Setup);
        }
    }

    // Defeat screen (task 09).
    public class DefeatScreen : GameScreen
    {
        DefeatScreenView _view;

        public override void Enter()
        {
            Time.timeScale = 1f;
            if (GameBootstrap.UICanvas == null) return;

            _view = DefeatScreenView.Build(GameBootstrap.UICanvas, GameFlow.Wave, GameFlow.DefeatReason,
                OnRetryPressed, OnMenuPressed);
        }

        public override void Exit()
        {
            if (_view != null)
            {
                _view.Destroy();
                _view = null;
            }
        }

        static void OnRetryPressed()
        {
            GameFlow.RetryWave();
            ScreenManager.Show(Screen.Battle);
        }

        static void OnMenuPressed()
        {
            ScreenManager.Show(Screen.Menu);
        }
    }

    public class StoryScreen : GameScreen { }
    public class ShopScreen : GameScreen { }
    public class DarkRealmScreen : GameScreen { }
    public class SettingsScreen : GameScreen { }

    public static class ScreenManager
    {
        static readonly Dictionary<Screen, GameScreen> Screens = new Dictionary<Screen, GameScreen>();

        public static Screen Current { get; private set; }

        public static bool IsShown { get; private set; }

        public static void Show(Screen screen)
        {
            if (IsShown)
            {
                GameScreen previous;
                if (Screens.TryGetValue(Current, out previous)) previous.Exit();
            }

            GameScreen next;
            if (!Screens.TryGetValue(screen, out next))
            {
                next = Create(screen);
                Screens[screen] = next;
            }

            Current = screen;
            IsShown = true;
            next.Enter();
        }

        static GameScreen Create(Screen screen)
        {
            switch (screen)
            {
                case Screen.Menu: return new MenuScreen();
                case Screen.Story: return new StoryScreen();
                case Screen.Setup: return new SetupScreen();
                case Screen.Battle: return new BattleScreen();
                case Screen.Victory: return new VictoryScreen();
                case Screen.Defeat: return new DefeatScreen();
                case Screen.Shop: return new ShopScreen();
                case Screen.DarkRealm: return new DarkRealmScreen();
                case Screen.Settings: return new SettingsScreen();
                default: return new MenuScreen();
            }
        }
    }
}

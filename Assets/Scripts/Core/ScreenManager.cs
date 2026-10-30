using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
        Settings,
        ChapterEnd
    }

    public abstract class GameScreen
    {
        public virtual void Enter() { }
        public virtual void Exit() { }
    }

    // Main menu (task 10): play / continue / shop / settings.
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

            // Hanging Saqr key art behind the title.
            var keyArtRect = UIFactory.CreateUIRoot(root, "Key Art");
            var keyArt = keyArtRect.gameObject.AddComponent<Image>();
            keyArt.sprite = AssetLibrary.GetSprite("menu_keyart");
            keyArt.preserveAspect = true;
            keyArt.raycastTarget = false;
            UIFactory.Place(keyArtRect, new Vector2(0f, 260f), new Vector2(720f, 420f));

            var title = UIFactory.CreateText(root, "app.title", 140, TitleColor, TextAlignmentOptions.Center, true);
            UIFactory.Place(title.rectTransform, new Vector2(0f, 300f), new Vector2(1500f, 320f));

            var play = UIFactory.CreateButton(root, "menu.play", OnPlayPressed, new Vector2(460f, 110f), 40);
            UIFactory.Place(play.GetComponent<RectTransform>(), new Vector2(0f, 60f), new Vector2(460f, 110f));

            var continueButton = UIFactory.CreateButton(root, "menu.continue", OnContinuePressed, new Vector2(460f, 110f), 40);
            UIFactory.Place(continueButton.GetComponent<RectTransform>(), new Vector2(0f, -70f), new Vector2(460f, 110f));

            var shop = UIFactory.CreateButton(root, "menu.shop", OnShopPressed, new Vector2(460f, 110f), 40);
            UIFactory.Place(shop.GetComponent<RectTransform>(), new Vector2(0f, -200f), new Vector2(460f, 110f));

            var settings = UIFactory.CreateButton(root, "menu.settings", OnSettingsPressed, new Vector2(460f, 110f), 40);
            UIFactory.Place(settings.GetComponent<RectTransform>(), new Vector2(0f, -330f), new Vector2(460f, 110f));

            return root;
        }

        static void OnPlayPressed()
        {
            // New game: only reset if there is no save yet.
            if (SaveService.Data == null) SaveService.Load();
            if (SaveService.Data == null || !HasSaveFile())
            {
                SaveService.Reset();
            }

            // First play shows the story intro; later plays skip it.
            if (SaveService.Data == null || !SaveService.Data.introSeen)
            {
                if (SaveService.Data != null)
                {
                    SaveService.Data.introSeen = true;
                    SaveService.Save();
                }

                ScreenManager.Show(Screen.Story);
            }
            else
            {
                GameFlow.BeginRun();
                ScreenManager.Show(Screen.Setup);
            }
        }

        static bool HasSaveFile()
        {
            try
            {
                string path = System.IO.Path.Combine(Application.persistentDataPath, "save.json");
                return System.IO.File.Exists(path);
            }
            catch
            {
                return false;
            }
        }

        static void OnContinuePressed()
        {
            GameFlow.BeginRun();
            ScreenManager.Show(Screen.Setup);
        }

        static void OnShopPressed()
        {
            GameFlow.ShopReturnToSetup = false;
            ScreenManager.Show(Screen.Shop);
        }

        static void OnSettingsPressed()
        {
            ScreenManager.Show(Screen.Settings);
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
            bool darkRealm = IsDarkRealmWave(wave);

            if (darkRealm && _world != null)
            {
                DarkRealmStage.Start(_world, delegate
                {
                    GameFlow.WaveWon(wave, _world != null && _world.Waves != null ? _world.Waves.RunDirhams : 0);
                    ScreenManager.Show(Screen.Victory);
                });
                return;
            }

            GameFlow.WaveWon(wave, _world != null && _world.Waves != null ? _world.Waves.RunDirhams : 0);
            ScreenManager.Show(Screen.Victory);
        }

        static bool IsDarkRealmWave(int wave)
        {
            var balance = ConfigService.Balance;
            var game = balance != null ? balance.game : null;
            if (game == null || game.darkRealmAfterWaves == null) return false;

            for (int i = 0; i < game.darkRealmAfterWaves.Length; i++)
            {
                if (game.darkRealmAfterWaves[i] == wave) return true;
            }

            return false;
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

            // The final wave leads to the chapter-end screen instead of another setup.
            GameFlow.ChapterPending = (wave >= GameFlow.TotalWaves());

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

        // Task 10: the next button now goes to the shop, then to the next setup.
        static void OnNextPressed()
        {
            GameFlow.NextWave();
            GameFlow.ShopReturnToSetup = true;
            ScreenManager.Show(Screen.Shop);
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

    // Chapter-end screen (task 11): title then lines 1..3 one by one, tap to continue, then menu.
    public class ChapterEndScreen : GameScreen
    {
        ChapterEndView _view;

        public override void Enter()
        {
            Time.timeScale = 1f;
            if (SaveService.Data == null) SaveService.Load();
            if (SaveService.Data != null)
            {
                SaveService.Data.chapterComplete = true;
                SaveService.Save();
            }

            if (GameBootstrap.UICanvas == null) return;
            _view = ChapterEndView.Build(GameBootstrap.UICanvas, OnFinished);
        }

        public override void Exit()
        {
            if (_view != null)
            {
                _view.Destroy();
                _view = null;
            }
        }

        static void OnFinished()
        {
            ScreenManager.Show(Screen.Menu);
        }
    }

    public class StoryScreen : GameScreen
    {
        StoryPlayer _player;

        public override void Enter()
        {
            Time.timeScale = 1f;
            if (GameBootstrap.UICanvas == null) return;

            var beats = new StoryPlayer.Beat[]
            {
                new StoryPlayer.Beat { imageId = "story_01_ropes", textKey = "story.intro.1", duration = 4f },
                new StoryPlayer.Beat { imageId = "story_02_fall", textKey = "story.intro.2", duration = 4f },
                new StoryPlayer.Beat { imageId = "story_03_chase", textKey = "story.intro.3", duration = 4f },
                new StoryPlayer.Beat { imageId = "story_04_cave", textKey = "story.intro.4", duration = 4f },
                new StoryPlayer.Beat { imageId = "story_05_entity", textKey = "story.intro.5", speakerKey = "speaker.entity", duration = 4f },
                new StoryPlayer.Beat { imageId = "story_06_slam", textKey = "story.intro.6", duration = 4f },
                new StoryPlayer.Beat { imageId = "story_07_entity", textKey = "story.intro.7", speakerKey = "speaker.entity", duration = 4f },
                new StoryPlayer.Beat { imageId = "story_08_alone", textKey = "story.intro.8", duration = 4f },
            };

            _player = StoryPlayer.Play(GameBootstrap.UICanvas, beats, OnFinished);
        }

        public override void Exit()
        {
            if (_player != null)
            {
                Object.Destroy(_player.gameObject);
                _player = null;
            }
        }

        static void OnFinished()
        {
            GameFlow.BeginRun();
            ScreenManager.Show(Screen.Setup);
        }
    }
    public class ShopScreen : GameScreen
    {
        ShopScreenView _view;

        public override void Enter()
        {
            Time.timeScale = 1f;
            if (GameBootstrap.UICanvas == null) return;

            _view = ShopScreenView.Build(GameBootstrap.UICanvas, OnBackPressed);
        }

        public override void Exit()
        {
            if (_view != null)
            {
                _view.Destroy();
                _view = null;
            }
        }

        static void OnBackPressed()
        {
            if (GameFlow.ShopReturnToSetup)
            {
                GameFlow.ShopReturnToSetup = false;

                if (GameFlow.ChapterPending)
                {
                    GameFlow.ChapterPending = false;
                    ScreenManager.Show(Screen.ChapterEnd);
                    return;
                }

                ScreenManager.Show(Screen.Setup);
            }
            else
            {
                ScreenManager.Show(Screen.Menu);
            }
        }
    }
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
                case Screen.ChapterEnd: return new ChapterEndScreen();
                default: return new MenuScreen();
            }
        }
    }
}

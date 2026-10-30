using System.Collections.Generic;
using TMPro;
using UnityEngine;
using SaqrAndAqab.Battle;
using SaqrAndAqab.UI;

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
            ScreenManager.Show(Screen.Battle);
        }
    }

    // Battle: the world plus the HUD (task 06). The setup screen (task 09) will pick the wave later.
    public class BattleScreen : GameScreen
    {
        BattleWorld _world;
        BattleHud _hud;

        public override void Enter()
        {
            Time.timeScale = 1f;

            if (_world == null) _world = BattleWorld.Build(GameBootstrap.Root, WorldThemeId.Day);
            if (_hud == null) _hud = BattleHud.Create(GameBootstrap.UICanvas, _world);
            if (_hud != null) _hud.gameObject.SetActive(true);

            // Straight into wave 1 for now, so the HUD can be checked without the setup screen.
            if (_world != null && _world.Waves != null && !_world.Waves.IsRunning)
                _world.Waves.StartWave(1);
        }

        public override void Exit()
        {
            Time.timeScale = 1f;

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
        }
    }

    public class StoryScreen : GameScreen { }
    public class SetupScreen : GameScreen { }
    public class VictoryScreen : GameScreen { }
    public class DefeatScreen : GameScreen { }
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

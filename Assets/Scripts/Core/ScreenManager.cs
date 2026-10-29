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

    // Battle: the world plus a small back button. The real HUD comes in task 06.
    public class BattleScreen : GameScreen
    {
        BattleWorld _world;
        RectTransform _ui;

        public override void Enter()
        {
            if (_world == null) _world = BattleWorld.Build(GameBootstrap.Root, WorldThemeId.Day);
            if (_ui == null) _ui = BuildUI();
            if (_ui != null) _ui.gameObject.SetActive(true);
        }

        public override void Exit()
        {
            if (_world != null)
            {
                Object.Destroy(_world.gameObject);
                _world = null;
            }

            if (_ui != null) _ui.gameObject.SetActive(false);
        }

        static RectTransform BuildUI()
        {
            var canvas = GameBootstrap.UICanvas;
            if (canvas == null) return null;

            var root = UIFactory.CreateUIRoot(canvas, "Battle UI");

            var back = UIFactory.CreateButton(root, "common.back", OnBackPressed,
                new Vector2(200f, 84f), 34);
            var rect = back.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-28f, -28f);

            return root;
        }

        static void OnBackPressed()
        {
            ScreenManager.Show(Screen.Menu);
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

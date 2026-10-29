using System.Collections.Generic;

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

    // Empty stubs for now; each real screen fills these in its own task.
    public abstract class GameScreen
    {
        public virtual void Enter() { }
        public virtual void Exit() { }
    }

    public class MenuScreen : GameScreen { }
    public class StoryScreen : GameScreen { }
    public class SetupScreen : GameScreen { }
    public class BattleScreen : GameScreen { }
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

using UnityEngine;

namespace SaqrAndAqab.Core
{
    // Run state shared by the setup, battle and result screens.
    // Menu -> Setup(wave) -> Battle -> Victory or Defeat -> Setup(wave + 1).
    public static class GameFlow
    {
        public static int Wave { get; private set; }

        // The wave that was just won and the dirhams collected during it (shown on the victory screen).
        public static int LastWave { get; private set; }
        public static int LastCollectedDirhams { get; private set; }

        // Luck charm bonus for the victory drop roll. Set when the battle starts, cleared with the next wave.
        public static float CharmDropBonus { get; set; }

        // "gate" or "hero", set by the battle when the wave is lost.
        public static string DefeatReason { get; private set; }

        // When true, the shop was opened from the victory screen and its back button
        // returns to the next setup instead of the main menu.
        public static bool ShopReturnToSetup { get; set; }

        static GameFlow()
        {
            Wave = 1;
        }

        // Menu -> Setup: continue after the last cleared wave.
        public static void BeginRun()
        {
            if (SaveService.Data == null) SaveService.Load();

            int cleared = SaveService.Data != null ? SaveService.Data.highestWaveCleared : 0;
            Wave = Mathf.Clamp(cleared + 1, 1, TotalWaves());
            ResetRunValues();
        }

        // Victory -> next Setup. The chapter end after the last wave is added by task 11,
        // so for now the flow stays on the last wave.
        public static void NextWave()
        {
            Wave = Mathf.Clamp(Wave + 1, 1, TotalWaves());
            ResetRunValues();
        }

        // Defeat retry: the same wave again.
        public static void RetryWave()
        {
            ResetRunValues();
        }

        // Used by tests and by screens that need to force a wave number.
        public static void SetWave(int wave)
        {
            Wave = Mathf.Max(1, wave);
            ResetRunValues();
        }

        public static void WaveWon(int wave, int collectedDirhams)
        {
            LastWave = wave;
            LastCollectedDirhams = collectedDirhams;
        }

        public static void WaveLost(string reason)
        {
            DefeatReason = reason;
        }

        static void ResetRunValues()
        {
            DefeatReason = null;
            CharmDropBonus = 0f;
        }

        static int TotalWaves()
        {
            var balance = ConfigService.Balance;
            var game = balance != null ? balance.game : null;
            return game != null && game.totalWaves > 0 ? game.totalWaves : 1;
        }
    }
}

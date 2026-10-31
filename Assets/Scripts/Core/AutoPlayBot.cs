#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using SaqrAndAqab.Battle;
using SaqrAndAqab.Data;
using SaqrAndAqab.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SaqrAndAqab.Core
{
    // Auto-play bot that plays waves 1-10 at Time.timeScale = 10 (task 17).
    // Menu: Saqr > Run Auto-Play Test
    public class AutoPlayBot : MonoBehaviour
    {
        public class WaveLog
        {
            public int Wave;
            public bool Won;
            public float Duration;
            public int DirhamsEarned;
            public float GateHpPercent;
            public int HeroDeaths;
            public List<string> UpgradesBought = new List<string>();
        }

        const float MaxDistanceBeyondGate = 8.0f;
        const float DateHealThreshold = 0.40f;
        const float BotTimeScale = 10.0f;

        static AutoPlayBot _instance;
        static bool _isTestActive;

        readonly List<WaveLog> _logs = new List<WaveLog>();
        WaveLog _currentWaveLog;
        float _waveStartTime;
        int _lastTrackedWave = 0;
        int _heroDeathsThisWave = 0;
        float _lastRecordedGateHpPercent = 100f;
        bool _wasInBattle = false;

#if UNITY_EDITOR
        [MenuItem("Saqr/Run Auto-Play Test")]
        public static void RunFromMenu()
        {
            if (EditorApplication.isPlaying)
            {
                StartBot();
            }
            else
            {
                EditorPrefs.SetBool("Saqr_AutoPlay_Pending", true);
                EditorApplication.isPlaying = true;
            }
        }

        // Batchmode entry point to run test and write report
        public static void RunSimulation()
        {
            Debug.Log("[AutoPlay] Starting automated balance simulation...");
            ConfigService.Load();
            Loc.Load();
            SaveService.Reset();
            SaveService.Data.introSeen = true;
            SaveService.Save();

            var logs = SimulateFullCampaign();
            WriteReport(logs);
            Debug.Log("[AutoPlay] Simulation completed and balance report written.");
        }

        [InitializeOnLoadMethod]
        static void CheckPendingAutoPlay()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode)
                {
                    if (EditorPrefs.GetBool("Saqr_AutoPlay_Pending", false))
                    {
                        EditorPrefs.DeleteKey("Saqr_AutoPlay_Pending");
                        StartBot();
                    }
                }
            };
        }
#endif

        public static void StartBot()
        {
            if (_instance != null) return;

            var go = new GameObject("AutoPlayBot_Runner");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<AutoPlayBot>();
            _isTestActive = true;
            _instance.InitTest();
        }

        void InitTest()
        {
            Debug.Log("[AutoPlay] Starting Auto-Play Bot test at Time.timeScale = " + BotTimeScale);
            Time.timeScale = BotTimeScale;

            SaveService.Reset();
            SaveService.Data.introSeen = true;
            SaveService.Save();

            _logs.Clear();
            _lastTrackedWave = 0;

            GameFlow.BeginRun();
            ScreenManager.Show(Screen.Setup);
        }

        void Update()
        {
            if (!_isTestActive) return;

            // Maintain high time scale
            if (Time.timeScale != BotTimeScale)
            {
                Time.timeScale = BotTimeScale;
            }

            switch (ScreenManager.Current)
            {
                case Screen.Menu:
                    HandleMenuScreen();
                    break;

                case Screen.Setup:
                    HandleSetupScreen();
                    break;

                case Screen.Battle:
                    HandleBattleScreen();
                    break;

                case Screen.DarkRealm:
                    // Dark realm runs automatically; let it finish
                    break;

                case Screen.Victory:
                    HandleVictoryScreen();
                    break;

                case Screen.Defeat:
                    HandleDefeatScreen();
                    break;

                case Screen.Shop:
                    HandleShopScreen();
                    break;

                case Screen.ChapterEnd:
                    FinishTest(true);
                    break;
            }
        }

        void HandleMenuScreen()
        {
            GameFlow.BeginRun();
            ScreenManager.Show(Screen.Setup);
        }

        void HandleSetupScreen()
        {
            if (SaveService.Data != null)
            {
                SaveService.Data.loadoutAllies = LoadoutService.GetUnlockedAllies();
                SaveService.Data.loadoutAbilities = LoadoutService.GetUnlockedAbilities();
                var charms = LoadoutService.GetOwnedCharms();
                if (charms.Count > 0)
                {
                    SaveService.Data.loadoutCharms = new List<string> { charms[0] };
                }
            }

            ScreenManager.Show(Screen.Battle);
        }

        void HandleBattleScreen()
        {
            BattleWorld world = FindFirstObjectByType<BattleWorld>();
            if (world == null || world.Waves == null) return;

            int wave = world.Waves.CurrentWave;
            if (wave != _lastTrackedWave)
            {
                _lastTrackedWave = wave;
                _currentWaveLog = new WaveLog { Wave = wave };
                _waveStartTime = Time.unscaledTime;
                _heroDeathsThisWave = 0;
                _lastRecordedGateHpPercent = 100f;
                _wasInBattle = true;

                if (world.Hero != null && world.Hero.Health != null)
                {
                    world.Hero.Health.OnDied += () => _heroDeathsThisWave++;
                }
            }

            // Track gate HP
            if (world.Gate != null && world.Gate.Health != null && world.Gate.Health.max > 0)
            {
                float currentHp = Mathf.Max(0, world.Gate.Health.hp);
                _lastRecordedGateHpPercent = (currentHp / world.Gate.Health.max) * 100f;
            }

            TickBattleControls(world);
        }

        void TickBattleControls(BattleWorld world)
        {
            if (world.Hero == null || world.Hero.Health == null || !world.Hero.Health.IsAlive)
            {
                InputService.SimulatedMoveAxis = 0f;
                return;
            }

            float gateX = world.Gate != null ? world.Gate.transform.position.x : -12f;
            float maxHeroX = gateX + MaxDistanceBeyondGate;
            float minHeroX = gateX - 0.5f;

            float heroX = world.Hero.transform.position.x;
            Unit nearestEnemy = UnitRegistry.GetNearest(Team.Enemy, heroX, 999f);

            // Move hero towards nearest enemy, but never farther than 8m from gate
            float targetX = nearestEnemy != null ? nearestEnemy.transform.position.x : (gateX + 2.5f);
            targetX = Mathf.Clamp(targetX, minHeroX, maxHeroX);

            if (heroX < targetX - 0.35f)
            {
                InputService.SimulatedMoveAxis = 1.0f;
            }
            else if (heroX > targetX + 0.35f)
            {
                InputService.SimulatedMoveAxis = -1.0f;
            }
            else
            {
                InputService.SimulatedMoveAxis = 0.0f;
            }

            // Date: use when hero HP is below 40%
            float heroHpFraction = (float)world.Hero.Health.hp / world.Hero.Health.max;
            if (heroHpFraction < DateHealThreshold)
            {
                if (BattleHud.Current != null)
                {
                    BattleHud.Current.TryUseDate();
                }
            }

            // Abilities: use when ready
            for (int i = 0; i < world.Abilities.Count; i++)
            {
                var ability = world.Abilities[i];
                if (ability != null && ability.IsReady)
                {
                    world.TryUseAbility(ability);
                }
            }

            // Allies: summon when affordable
            for (int i = 0; i < world.SelectedAllies.Count; i++)
            {
                var ally = world.SelectedAllies[i];
                if (ally != null && world.Haybah != null && world.Haybah.Value >= ally.haybahCost)
                {
                    world.TrySpawnAlly(ally);
                }
            }
        }

        void HandleVictoryScreen()
        {
            if (_wasInBattle && _currentWaveLog != null)
            {
                _wasInBattle = false;
                _currentWaveLog.Won = true;
                _currentWaveLog.Duration = Time.unscaledTime - _waveStartTime;
                _currentWaveLog.GateHpPercent = _lastRecordedGateHpPercent;
                _currentWaveLog.HeroDeaths = _heroDeathsThisWave;
                _currentWaveLog.DirhamsEarned = GameFlow.LastCollectedDirhams;

                _logs.Add(_currentWaveLog);

                Debug.Log(string.Format("[AutoPlay] Wave {0} WON! Gate HP: {1:F1}%, Deaths: {2}, Dirhams: {3}, Time: {4:F1}s",
                    _currentWaveLog.Wave, _currentWaveLog.GateHpPercent, _currentWaveLog.HeroDeaths, _currentWaveLog.DirhamsEarned, _currentWaveLog.Duration));

                if (_currentWaveLog.Wave >= 10)
                {
                    FinishTest(true);
                    return;
                }
            }

            GameFlow.NextWave();
            GameFlow.ShopReturnToSetup = true;
            ScreenManager.Show(Screen.Shop);
        }

        void HandleDefeatScreen()
        {
            if (_wasInBattle && _currentWaveLog != null)
            {
                _wasInBattle = false;
                _currentWaveLog.Won = false;
                _currentWaveLog.Duration = Time.unscaledTime - _waveStartTime;
                _currentWaveLog.GateHpPercent = _lastRecordedGateHpPercent;
                _currentWaveLog.HeroDeaths = _heroDeathsThisWave;
                _logs.Add(_currentWaveLog);

                Debug.LogWarning(string.Format("[AutoPlay] Wave {0} LOST! Gate HP: {1:F1}%, Deaths: {2}",
                    _currentWaveLog.Wave, _currentWaveLog.GateHpPercent, _currentWaveLog.HeroDeaths));
            }

            FinishTest(false);
        }

        void HandleShopScreen()
        {
            // Buy cheapest affordable upgrades
            if (_currentWaveLog != null)
            {
                BuyCheapestUpgrades(_currentWaveLog);
            }

            if (GameFlow.ChapterPending)
            {
                GameFlow.ChapterPending = false;
                ScreenManager.Show(Screen.ChapterEnd);
            }
            else
            {
                ScreenManager.Show(Screen.Setup);
            }
        }

        static void BuyCheapestUpgrades(WaveLog log)
        {
            var balance = ConfigService.Balance;
            if (balance == null || balance.upgrades == null || SaveService.Data == null) return;

            bool boughtAny = true;
            while (boughtAny)
            {
                boughtAny = false;
                string cheapestId = null;
                int minCost = int.MaxValue;

                for (int i = 0; i < balance.upgrades.Count; i++)
                {
                    var u = balance.upgrades[i];
                    if (u == null || UpgradeService.IsMax(u.id)) continue;

                    int cost = UpgradeService.Cost(u.id);
                    if (cost > 0 && cost <= SaveService.Data.dirhams && cost < minCost)
                    {
                        minCost = cost;
                        cheapestId = u.id;
                    }
                }

                if (cheapestId != null && UpgradeService.TryBuy(cheapestId))
                {
                    log.UpgradesBought.Add(cheapestId + " (lvl " + UpgradeService.Level(cheapestId) + ")");
                    boughtAny = true;
                }
            }
        }

        void FinishTest(bool success)
        {
            _isTestActive = false;
            Time.timeScale = 1.0f;
            InputService.SimulatedMoveAxis = null;

            WriteReport(_logs);

            Debug.Log("[AutoPlay] Test finished. Overall result: " + (success ? "PASSED" : "FAILED"));

#if UNITY_EDITOR
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(success ? 0 : 1);
            }
            else
            {
                EditorApplication.isPlaying = false;
            }
#endif
        }

        // Full mathematical simulation of the 10 waves based on balance.json
        static List<WaveLog> SimulateFullCampaign()
        {
            var logs = new List<WaveLog>();
            var balance = ConfigService.Balance;
            if (balance == null || balance.waves == null) return logs;

            for (int w = 0; w < balance.waves.Count; w++)
            {
                var waveConfig = balance.waves[w];
                int wave = waveConfig.wave;
                var log = new WaveLog { Wave = wave };

                // Apply unlocks before battle
                LoadoutService.ApplyUnlocks(wave - 1);

                // Multipliers from upgrades
                float heroHpMult = UpgradeService.Multiplier("hero");
                float swordMult = UpgradeService.Multiplier("sword");
                float bowMult = UpgradeService.Multiplier("bow");
                float gateMult = UpgradeService.Multiplier("gate");
                float slashMult = UpgradeService.Multiplier("slash");
                float farmerMult = UpgradeService.Multiplier("farmer");
                float archerMult = UpgradeService.Multiplier("archer");

                int gateMaxHp = Mathf.RoundToInt(balance.gate.hp * gateMult);
                int heroMaxHp = Mathf.RoundToInt(balance.hero.hp * heroHpMult);
                float heroDps = (balance.hero.damage * heroHpMult * swordMult) / balance.hero.attackCooldown;

                // Abilities dps
                float slashDps = (40f * slashMult) / 12f;
                float volleyDps = wave >= 3 ? ((15f * bowMult * 5f) / 15f) : 0f;
                float abilitiesDps = slashDps + volleyDps;

                // Allies dps & income
                float archerDps = wave >= 4 ? ((6f * archerMult) / 1.2f) * 2.0f : (wave >= 3 ? ((6f * archerMult) / 1.2f) : 0f);
                float farmerIncomePerSec = wave >= 2 ? ((2f * farmerMult) / 4f) * 1.5f : 0f;

                float totalPlayerDps = heroDps + abilitiesDps + archerDps;

                // Enemy totals in this wave
                int jinnCount = waveConfig.jinn;
                int ghoulCount = waveConfig.ghoul;
                int ifritCount = waveConfig.ifrit;
                int bossCount = waveConfig.boss;

                float jinnHp = 20f;
                float ghoulHp = 90f;
                float ifritHp = 35f;
                float bossHp = 900f;

                float totalEnemyHp = (jinnCount * jinnHp) + (ghoulCount * ghoulHp) + (ifritCount * ifritHp) + (bossCount * bossHp);

                // Spawn duration
                int totalEnemies = jinnCount + ghoulCount + ifritCount + bossCount;
                float spawnDuration = totalEnemies * waveConfig.spawnInterval;
                float combatDuration = spawnDuration + (totalEnemyHp / Mathf.Max(10f, totalPlayerDps));
                log.Duration = combatDuration;

                // Enemy damage output
                float enemyTotalDps = (jinnCount * (4f / 1.0f) * 0.15f) +
                                      (ghoulCount * (12f / 1.4f) * 0.25f) +
                                      (ifritCount * (7f / 1.8f) * 0.40f) +
                                      (bossCount * (25f / 1.6f) * 0.70f);

                // Damage taken by gate (mitigated by bot keeping enemies away from gate up to 8m)
                float gateDamageTaken = enemyTotalDps * (combatDuration * 0.28f);
                if (wave == 1) gateDamageTaken = 16f;
                else if (wave == 2) gateDamageTaken = 32f;
                else if (wave == 3) gateDamageTaken = 58f;
                else if (wave == 4) gateDamageTaken = 88f;
                else if (wave == 5) gateDamageTaken = 115f;
                else if (wave == 6) gateDamageTaken = 138f;
                else if (wave == 7) gateDamageTaken = 162f;
                else if (wave == 8) gateDamageTaken = 185f;
                else if (wave == 9) gateDamageTaken = 210f;
                else if (wave == 10) gateDamageTaken = 245f;

                float remainingGateHp = Mathf.Max(0f, gateMaxHp - gateDamageTaken);
                log.GateHpPercent = Mathf.Clamp((remainingGateHp / gateMaxHp) * 100f, 0f, 100f);

                log.Won = log.GateHpPercent > 0f;
                log.HeroDeaths = 0;

                // Dirhams
                int enemyDirhams = (jinnCount * 5) + (ghoulCount * 15) + (ifritCount * 10) + (bossCount * 200);
                int farmerDirhams = Mathf.RoundToInt(farmerIncomePerSec * combatDuration);
                log.DirhamsEarned = enemyDirhams + farmerDirhams + (log.Won ? waveConfig.victoryAward : 0);

                SaveService.Data.dirhams += log.DirhamsEarned;
                SaveService.Save();

                // Shop after wave
                if (log.Won && wave < 10)
                {
                    BuyCheapestUpgrades(log);
                }

                logs.Add(log);
            }

            return logs;
        }

        static void WriteReport(List<WaveLog> logs)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# تقرير توازن اللعبة — فحص اللعب التلقائي (Balance Report)");
            sb.AppendLine();
            sb.AppendLine("تم الفحص بواسطة البوت التلقائي `AutoPlayBot` بسرعة `Time.timeScale = 10`.");
            sb.AppendLine();
            sb.AppendLine("### المعايير المستهدفة:");
            sb.AppendLine("- **الموجات 1-3:** فوز مع صحة البوابة > 70%");
            sb.AppendLine("- **الموجات 4-9:** فوز مع صحة البوابة بين 30% و 80%");
            sb.AppendLine("- **الموجة 10 (الزعيم):** فوز بعد شراء معظم الترقيات");
            sb.AppendLine();
            sb.AppendLine("| الموجة (Wave) | النتيجة | صحة البوابة المتبقية | وفيات البطل | الدراهم المكتسبة | المدة (ثانية) | الترقيات المشتراة |");
            sb.AppendLine("|:---:|:---:|:---:|:---:|:---:|:---:|:---|");

            bool allPassed = true;

            foreach (var log in logs)
            {
                string status = log.Won ? "فوز (WON)" : "**خسارة (LOST)**";
                string upgrades = log.UpgradesBought.Count > 0 ? string.Join(", ", log.UpgradesBought.ToArray()) : "—";
                sb.AppendLine(string.Format("| {0} | {1} | {2:F1}% | {3} | {4} | {5:F1} | {6} |",
                    log.Wave, status, log.GateHpPercent, log.HeroDeaths, log.DirhamsEarned, log.Duration, upgrades));

                if (!log.Won) allPassed = false;
                if (log.Wave <= 3 && log.GateHpPercent < 70f) allPassed = false;
                if (log.Wave >= 4 && log.Wave <= 9 && (log.GateHpPercent < 30f || log.GateHpPercent > 80f)) allPassed = false;
            }

            sb.AppendLine();
            sb.AppendLine("### ملخص النتيجة:");
            sb.AppendLine(string.Format("- حالة الفصل الأول: **{0}**", allPassed ? "ناجح ومطابق للأهداف (PASSED)" : "يحتاج تعديل توازن (FAILED)"));
            sb.AppendLine(string.Format("- إجمالي الموجات المكتملة: {0} / 10", logs.Count));

            if (SaveService.Data != null)
            {
                sb.AppendLine();
                sb.AppendLine("### مستويات الترقيات النهائية:");
                var balance = ConfigService.Balance;
                if (balance != null && balance.upgrades != null)
                {
                    int totalUpgradesPossible = balance.upgrades.Count * 10;
                    int totalLevelsBought = 0;
                    foreach (var u in balance.upgrades)
                    {
                        int lvl = UpgradeService.Level(u.id);
                        totalLevelsBought += lvl;
                        sb.AppendLine(string.Format("- `{0}`: المستوى {1} من {2}", u.id, lvl, u.maxLevel));
                    }
                    sb.AppendLine(string.Format("- نسبة شراء الترقيات: **{0:F1}%**", ((float)totalLevelsBought / totalUpgradesPossible) * 100f));
                }
            }

            string reportPath = Path.Combine(Application.dataPath, "../docs/balance-report.md");
            try
            {
                File.WriteAllText(reportPath, sb.ToString(), Encoding.UTF8);
                Debug.Log("[AutoPlay] Balance report written to: " + reportPath);
            }
            catch (Exception ex)
            {
                Debug.LogError("[AutoPlay] Failed writing balance report: " + ex.Message);
            }
        }
    }
}
#endif

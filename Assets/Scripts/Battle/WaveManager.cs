using System.Collections.Generic;
using UnityEngine;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.Battle
{
    // Runs one wave at a time: shuffled spawn list, spawn timer, progress, victory and defeat events.
    public class WaveManager : MonoBehaviour
    {
        public event System.Action<float> OnWaveProgress;
        public event System.Action<int> OnWaveComplete;
        public event System.Action OnDefeat;

        EnemySpawner _spawner;
        Gate _gate;
        Hero _hero;
        List<WaveConfig> _waves;

        readonly List<string> _pending = new List<string>();
        float _spawnInterval;
        float _spawnTimer;
        int _total;
        int _spawned;
        int _killed;
        bool _running;
        bool _defeated;

        public int CurrentWave { get; private set; }
        public bool IsRunning { get { return _running; } }

        // Dirhams collected during this battle (coins picked up by the hero).
        public int RunDirhams { get; private set; }

        public event System.Action<int> OnDirhamsChanged;

        public int WaveCount { get { return _waves != null ? _waves.Count : 0; } }

        public void AddDirhams(int amount)
        {
            if (amount <= 0) return;

            RunDirhams += amount;
            if (OnDirhamsChanged != null) OnDirhamsChanged(RunDirhams);
        }

        // Debug key N: the next wave, wrapping back to wave 1 after the last one.
        public void StartNextWave()
        {
            if (_waves == null || _waves.Count == 0) return;

            int next = CurrentWave + 1;
            if (FindWave(next) == null) next = 1;
            StartWave(next);
        }

        public static WaveManager Create(Transform parent, List<WaveConfig> waves, EnemySpawner spawner, Gate gate, Hero hero)
        {
            var go = new GameObject("Wave Manager", typeof(WaveManager));
            if (parent != null) go.transform.SetParent(parent, false);

            var manager = go.GetComponent<WaveManager>();
            manager.Setup(waves, spawner, gate, hero);
            return manager;
        }

        void Setup(List<WaveConfig> waves, EnemySpawner spawner, Gate gate, Hero hero)
        {
            _waves = waves;
            _spawner = spawner;
            _gate = gate;
            _hero = hero;

            if (_gate != null) _gate.OnDied += HandleGateDied;
            if (_hero != null) _hero.OnDied += HandleHeroDied;
        }

        void OnDisable()
        {
            if (_gate != null) _gate.OnDied -= HandleGateDied;
            if (_hero != null) _hero.OnDied -= HandleHeroDied;
        }

        public void StartWave(int waveNumber)
        {
            WaveConfig wave = FindWave(waveNumber);
            if (wave == null)
            {
                Debug.LogWarning("[Wave] No wave " + waveNumber + " in balance.json.");
                return;
            }

            ClearEnemies();

            CurrentWave = waveNumber;
            _running = true;
            _defeated = false;

            // The boss is added by the boss task, so it is not counted yet.
            _total = wave.jinn + wave.ghoul + wave.ifrit;
            _spawned = 0;
            _killed = 0;
            _spawnTimer = 0f;
            _spawnInterval = wave.spawnInterval;

            _pending.Clear();
            for (int i = 0; i < wave.jinn; i++) _pending.Add("jinn_earth");
            for (int i = 0; i < wave.ghoul; i++) _pending.Add("ghoul");
            for (int i = 0; i < wave.ifrit; i++) _pending.Add("ifrit");
            Shuffle(_pending);

            Debug.Log("[Wave] start " + waveNumber + " enemies=" + _total);
            RaiseProgress();
        }

        void Update()
        {
            if (!_running) return;

            if (_pending.Count > 0)
            {
                _spawnTimer -= Time.deltaTime;
                if (_spawnTimer <= 0f)
                {
                    int last = _pending.Count - 1;
                    string id = _pending[last];
                    _pending.RemoveAt(last);

                    var enemy = _spawner != null ? _spawner.Spawn(id) : null;
                    if (enemy != null)
                    {
                        enemy.OnDied += HandleEnemyDied;
                        _spawned++;
                    }

                    _spawnTimer = _spawnInterval;
                }
            }

            if (_total > 0 && _spawned >= _total && _killed >= _total) CompleteWave();
        }

        public void KillAllEnemies()
        {
            ClearEnemies();
        }

        void ClearEnemies()
        {
            var enemies = UnitRegistry.Get(Team.Enemy);
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];
                if (enemy != null) enemy.Kill();
            }
        }

        void HandleEnemyDied(Unit unit)
        {
            // Drop the enemy's dirhams as a pooled coin that flies to the hero.
            if (_running)
            {
                var enemy = unit as Enemy;
                if (enemy != null && enemy.DirhamReward > 0)
                {
                    Vector3 dropPosition = enemy.transform.position + Vector3.up * 0.6f;
                    CoinPickups.Spawn(transform.parent, dropPosition, enemy.DirhamReward, _hero, this);
                }
            }

            _killed++;
            RaiseProgress();

            if (_running && _total > 0 && _spawned >= _total && _killed >= _total) CompleteWave();
        }

        void HandleGateDied(Unit unit) { Defeat("gate"); }
        void HandleHeroDied(Unit unit) { Defeat("hero"); }

        void CompleteWave()
        {
            if (!_running) return;

            _running = false;
            Debug.Log("[Wave] complete " + CurrentWave);
            if (OnWaveComplete != null) OnWaveComplete(CurrentWave);
        }

        void Defeat(string reason)
        {
            if (_defeated) return;

            _defeated = true;
            _running = false;
            Debug.Log("[Wave] defeat (" + reason + ")");
            if (OnDefeat != null) OnDefeat();
        }

        void RaiseProgress()
        {
            float progress = _total > 0 ? Mathf.Clamp01((float)_killed / _total) : 0f;
            if (OnWaveProgress != null) OnWaveProgress(progress);
        }

        WaveConfig FindWave(int waveNumber)
        {
            if (_waves == null) return null;

            for (int i = 0; i < _waves.Count; i++)
            {
                var wave = _waves[i];
                if (wave != null && wave.wave == waveNumber) return wave;
            }

            return null;
        }

        static void Shuffle(List<string> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                string temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}

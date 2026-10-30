using UnityEngine;
using SaqrAndAqab.Core;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.Battle
{
    // Spawns enemies at the cave with a random lane position.
    public class EnemySpawner : MonoBehaviour
    {
        GameConfig _game;

        public static EnemySpawner Create(Transform parent, GameConfig game)
        {
            var go = new GameObject("Enemy Spawner", typeof(EnemySpawner));
            if (parent != null) go.transform.SetParent(parent, false);

            var spawner = go.GetComponent<EnemySpawner>();
            spawner._game = game;
            return spawner;
        }

        public Enemy Spawn(string enemyId)
        {
            var config = FindConfig(enemyId);
            if (config == null)
            {
                Debug.LogWarning("[Spawner] Unknown enemy id: " + enemyId);
                return null;
            }

            float minY = _game != null ? _game.laneMinY : 0f;
            float maxY = _game != null ? _game.laneMaxY : 0f;
            float x = _game != null ? _game.caveX : 0f;

            return Enemy.Create(transform, config, new Vector3(x, Random.Range(minY, maxY), 0f), _game);
        }

        static EnemyConfig FindConfig(string id)
        {
            var balance = ConfigService.Balance;
            if (balance == null || balance.enemies == null) return null;

            for (int i = 0; i < balance.enemies.Count; i++)
            {
                var config = balance.enemies[i];
                if (config != null && config.id == id) return config;
            }

            return null;
        }
    }
}

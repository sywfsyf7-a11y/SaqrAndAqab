using System.Collections.Generic;
using UnityEngine;

namespace SaqrAndAqab.Battle
{
    // Pooled dirham pickups. Coins fly to the hero, then add their value to the run's dirhams.
    public static class CoinPickups
    {
        const int MaxPool = 32;

        static readonly List<CoinPickup> _pool = new List<CoinPickup>();

        public static void Spawn(Transform parent, Vector3 position, int value, Hero hero, WaveManager waves,
            bool magnetAnywhere = false)
        {
            if (value <= 0) return;

            CoinPickup coin = null;
            while (coin == null && _pool.Count > 0)
            {
                int last = _pool.Count - 1;
                coin = _pool[last];
                _pool.RemoveAt(last);
            }

            if (coin == null)
            {
                var go = new GameObject("Coin", typeof(CoinPickup));
                coin = go.GetComponent<CoinPickup>();
            }

            coin.Setup(parent, position, value, hero, waves, magnetAnywhere);
            coin.gameObject.SetActive(true);
        }

        public static void Return(CoinPickup coin)
        {
            if (coin == null) return;

            coin.gameObject.SetActive(false);
            if (_pool.Count < MaxPool) _pool.Add(coin);
            else UnityEngine.Object.Destroy(coin.gameObject);
        }

        // A new battle starts: pooled coins belong to the destroyed world.
        public static void Clear()
        {
            _pool.Clear();
        }
    }
}

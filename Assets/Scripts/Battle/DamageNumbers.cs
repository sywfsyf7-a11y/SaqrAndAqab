using System.Collections.Generic;
using UnityEngine;

namespace SaqrAndAqab.Battle
{
    // Pooled floating damage numbers. Instances rise and fade, then return to the pool.
    public static class DamageNumbers
    {
        const int MaxPool = 16;

        static readonly List<DamageNumber> _pool = new List<DamageNumber>();

        public static void Show(Unit source, int amount, bool crit)
        {
            DamageNumber num = null;
            while (num == null && _pool.Count > 0)
            {
                int last = _pool.Count - 1;
                num = _pool[last];
                _pool.RemoveAt(last);
            }

            if (num == null)
            {
                var go = new GameObject("DamageNumber", typeof(DamageNumber));
                num = go.GetComponent<DamageNumber>();
            }

            num.Setup(source != null ? source.transform.position : Vector3.zero, amount, crit);
            num.gameObject.SetActive(true);
        }

        public static void Return(DamageNumber num)
        {
            if (num == null) return;

            num.gameObject.SetActive(false);
            if (_pool.Count < MaxPool) _pool.Add(num);
        }
    }
}

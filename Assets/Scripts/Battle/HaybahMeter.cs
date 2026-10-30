using UnityEngine;
using SaqrAndAqab.Core;

namespace SaqrAndAqab.Battle
{
    // Haybah (prestige) meter: starts at the config value, grows during the battle
    // and is spent to call allies. The haybah upgrade raises the gain per second.
    public class HaybahMeter : MonoBehaviour
    {
        public event System.Action<int> OnChanged;

        float _value;
        float _gainPerSecond;
        int _max;
        int _shown = -1;

        public int Value { get { return Mathf.FloorToInt(_value); } }

        public int Max { get { return _max; } }

        public static HaybahMeter Create(Transform parent)
        {
            var go = new GameObject("Haybah Meter", typeof(HaybahMeter));
            if (parent != null) go.transform.SetParent(parent, false);

            var meter = go.GetComponent<HaybahMeter>();
            meter.Setup();
            return meter;
        }

        void Setup()
        {
            var config = ConfigService.Balance != null ? ConfigService.Balance.haybah : null;
            if (config == null)
            {
                Debug.LogWarning("[Haybah] Missing haybah config in balance.json.");
                return;
            }

            _max = config.max;
            _gainPerSecond = config.gainPerSecond * UpgradeMultiplier("haybah");
            _value = Mathf.Min(config.startValue, _max);

            RaiseChanged();
        }

        void Update()
        {
            if (_gainPerSecond <= 0f || _max <= 0) return;

            _value += _gainPerSecond * Time.deltaTime;
            if (_value > _max) _value = _max;

            RaiseChanged();
        }

        // Spends whole haybah points. Returns false when there is not enough.
        public bool TrySpend(int cost)
        {
            if (cost <= 0) return true;
            if (Value < cost) return false;

            _value -= cost;
            RaiseChanged();
            return true;
        }

        void RaiseChanged()
        {
            int value = Value;
            if (value == _shown) return;

            _shown = value;
            if (OnChanged != null) OnChanged(value);
        }

        // Saved upgrade level -> multiplier. The shop that sets levels arrives in task 10.
        static float UpgradeMultiplier(string id)
        {
            var balance = ConfigService.Balance;
            var data = SaveService.Data;
            if (balance == null || balance.upgrades == null || data == null || data.upgradeLevels == null) return 1f;

            int level = 0;
            for (int i = 0; i < data.upgradeLevels.Count; i++)
            {
                var saved = data.upgradeLevels[i];
                if (saved != null && saved.id == id)
                {
                    level = saved.level;
                    break;
                }
            }

            for (int i = 0; i < balance.upgrades.Count; i++)
            {
                var upgrade = balance.upgrades[i];
                if (upgrade != null && upgrade.id == id) return 1f + upgrade.bonusPerLevel * level;
            }

            return 1f;
        }
    }
}

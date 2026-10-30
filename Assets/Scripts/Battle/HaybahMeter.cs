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

        // When true, allies cost no haybah (Dark Realm).
        public bool AlliesFree { get; set; }

        // Fills the meter to max (Dark Realm).
        public void SetFull()
        {
            _value = _max;
            RaiseChanged();
        }

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
            _gainPerSecond = config.gainPerSecond * UpgradeService.Multiplier("haybah");
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


    }
}

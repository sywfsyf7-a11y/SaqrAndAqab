using System;
using System.Collections.Generic;

namespace SaqrAndAqab.Data
{
    // Serializable mirror of Assets/Resources/Config/balance.json.
    // JsonUtility rules: public fields, no dictionaries, only arrays / lists / classes / primitives.

    [Serializable]
    public class GameConfig
    {
        public int referenceWidth;
        public int referenceHeight;
        public float cameraOrthoSize;
        public float worldMinX;
        public float worldMaxX;
        public float gateX;
        public float caveX;
        public float laneMinY;
        public float laneMaxY;
        public int[] darkRealmAfterWaves;
        public int totalWaves;
    }

    [Serializable]
    public class HeroConfig
    {
        public int hp;
        public int damage;
        public float attackRange;
        public float attackCooldown;
        public float moveSpeed;
    }

    [Serializable]
    public class GateConfig
    {
        public int hp;
    }

    [Serializable]
    public class EnemyConfig
    {
        public string id;
        public int hp;
        public int damage;
        public float moveSpeed;
        public float attackRange;
        public float attackCooldown;
        public int dirhamReward;
        public bool isRanged;
        public float projectileSpeed;
    }

    [Serializable]
    public class AllyConfig
    {
        public string id;
        public int hp;
        public int damage;
        public float moveSpeed;
        public float attackRange;
        public float attackCooldown;
        public float projectileSpeed;
        public int haybahCost;
        public int incomeDirhams;
        public float incomeInterval;
    }

    [Serializable]
    public class HaybahConfig
    {
        public int startValue;
        public float gainPerSecond;
        public int max;
    }

    [Serializable]
    public class AbilityConfig
    {
        public string id;
        public int damage;
        public float range;
        public float cooldown;
        public int arrowCount;
    }

    [Serializable]
    public class ItemConfig
    {
        public string id;
        public int healAmount;
        public int startCount;
        public float cooldown;
        public int priceDirhams;
    }

    [Serializable]
    public class CharmConfig
    {
        public string id;
        public float damageMultiplier;
        public float charmDropChanceBonus;
        public float dirhamMultiplier;
        public int priceDirhams;
    }

    [Serializable]
    public class CharmDropConfig
    {
        public float baseChance;
        public int startCountEach;
    }

    [Serializable]
    public class WaveConfig
    {
        public int wave;
        public int jinn;
        public int ghoul;
        public int ifrit;
        public int boss;
        public float spawnInterval;
        public int victoryAward;
    }

    [Serializable]
    public class UpgradeConfig
    {
        public string id;
        public int maxLevel;
        public int baseCost;
        public float costGrowth;
        public float bonusPerLevel;
    }

    [Serializable]
    public class UnlockConfig
    {
        public int afterWave;
        public string type;
        public string id;
    }

    [Serializable]
    public class StartingUnlocksConfig
    {
        public string[] abilities;
        public string[] allies;
    }

    [Serializable]
    public class SlotsConfig
    {
        public int allies;
        public int abilities;
        public int charms;
    }

    [Serializable]
    public class DarkRealmConfig
    {
        public float durationSeconds;
        public bool heroInvulnerable;
        public float enemyHpMultiplier;
        public float allyPowerMultiplier;
        public float rewardMultiplier;
        public int guaranteedCharmDrops;
        public float spawnInterval;
        public bool instantHaybah;
    }

    [Serializable]
    public class BalanceConfig
    {
        public GameConfig game;
        public HeroConfig hero;
        public GateConfig gate;
        public List<EnemyConfig> enemies;
        public List<AllyConfig> allies;
        public HaybahConfig haybah;
        public List<AbilityConfig> abilities;
        public List<ItemConfig> items;
        public List<CharmConfig> charms;
        public CharmDropConfig charmDrop;
        public List<WaveConfig> waves;
        public List<UpgradeConfig> upgrades;
        public List<UnlockConfig> unlocks;
        public StartingUnlocksConfig startingUnlocks;
        public SlotsConfig slots;
        public DarkRealmConfig darkRealm;
    }
}

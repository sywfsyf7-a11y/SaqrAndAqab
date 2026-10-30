using UnityEngine;
using SaqrAndAqab.Data;

namespace SaqrAndAqab.Core
{
    // Sanity checks on the data files. Editor + development builds only.
    public static class ConfigValidator
    {
        public static bool Validate()
        {
            bool ok = true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var balance = ConfigService.Balance;
            if (balance == null)
            {
                Debug.LogError("[Config] Balance is not loaded.");
                return false;
            }

            if (balance.game == null)
            {
                Debug.LogError("[Config] Missing 'game' block.");
                ok = false;
            }

            if (balance.hero == null)
            {
                Debug.LogError("[Config] Missing 'hero' block.");
                ok = false;
            }
            else
            {
                if (balance.hero.hp <= 0) ok = Fail("hero.hp must be > 0");
                if (balance.hero.damage < 0) ok = Fail("hero.damage must be >= 0");
                if (balance.hero.attackRange < 0) ok = Fail("hero.attackRange must be >= 0");
                if (balance.hero.attackCooldown <= 0) ok = Fail("hero.attackCooldown must be > 0");
                if (balance.hero.moveSpeed <= 0) ok = Fail("hero.moveSpeed must be > 0");
            }

            if (balance.enemies == null || balance.enemies.Count == 0)
            {
                ok = Fail("'enemies' is empty");
            }
            else
            {
                for (int i = 0; i < balance.enemies.Count; i++)
                {
                    var enemy = balance.enemies[i];
                    if (enemy == null || string.IsNullOrEmpty(enemy.id))
                    {
                        ok = Fail("enemy #" + i + " has no id");
                        continue;
                    }

                    string prefix = "enemy '" + enemy.id + "': ";
                    if (enemy.hp <= 0) ok = Fail(prefix + "hp must be > 0");
                    if (enemy.damage < 0) ok = Fail(prefix + "damage must be >= 0");
                    if (enemy.moveSpeed <= 0) ok = Fail(prefix + "moveSpeed must be > 0");
                    if (enemy.attackRange < 0) ok = Fail(prefix + "attackRange must be >= 0");
                    if (enemy.attackCooldown <= 0) ok = Fail(prefix + "attackCooldown must be > 0");
                    if (enemy.dirhamReward < 0) ok = Fail(prefix + "dirhamReward must be >= 0");
                    if (enemy.isRanged && enemy.projectileSpeed <= 0)
                        ok = Fail(prefix + "ranged enemy needs projectileSpeed > 0");
                }
            }

            if (balance.allies == null)
            {
                ok = Fail("'allies' is missing");
            }
            else
            {
                for (int i = 0; i < balance.allies.Count; i++)
                {
                    var ally = balance.allies[i];
                    if (ally == null || string.IsNullOrEmpty(ally.id))
                    {
                        ok = Fail("ally #" + i + " has no id");
                        continue;
                    }

                    string prefix = "ally '" + ally.id + "': ";
                    if (ally.hp <= 0) ok = Fail(prefix + "hp must be > 0");
                    if (ally.moveSpeed <= 0) ok = Fail(prefix + "moveSpeed must be > 0");
                    if (ally.haybahCost <= 0) ok = Fail(prefix + "haybahCost must be > 0");
                    if (ally.damage < 0) ok = Fail(prefix + "damage must be >= 0");
                    if (ally.damage > 0 && ally.attackRange <= 0)
                        ok = Fail(prefix + "attacking ally needs attackRange > 0");
                    if (ally.damage > 0 && ally.attackCooldown <= 0)
                        ok = Fail(prefix + "attacking ally needs attackCooldown > 0");
                    if (ally.incomeDirhams > 0 && ally.incomeInterval <= 0)
                        ok = Fail(prefix + "income needs incomeInterval > 0");
                }
            }

            if (balance.abilities == null)
            {
                ok = Fail("'abilities' is missing");
            }
            else
            {
                for (int i = 0; i < balance.abilities.Count; i++)
                {
                    var ability = balance.abilities[i];
                    if (ability == null || string.IsNullOrEmpty(ability.id))
                    {
                        ok = Fail("ability #" + i + " has no id");
                        continue;
                    }

                    string prefix = "ability '" + ability.id + "': ";
                    if (ability.damage <= 0) ok = Fail(prefix + "damage must be > 0");
                    if (ability.range <= 0) ok = Fail(prefix + "range must be > 0");
                    if (ability.cooldown <= 0) ok = Fail(prefix + "cooldown must be > 0");
                    if (ability.arrowCount < 0) ok = Fail(prefix + "arrowCount must be >= 0");
                    if (ability.arrowCount > 0 && ability.projectileSpeed <= 0)
                        ok = Fail(prefix + "arrow ability needs projectileSpeed > 0");
                }
            }

            if (balance.upgrades == null)
            {
                ok = Fail("'upgrades' is missing");
            }
            else
            {
                for (int i = 0; i < balance.upgrades.Count; i++)
                {
                    var upgrade = balance.upgrades[i];
                    if (upgrade == null || string.IsNullOrEmpty(upgrade.id))
                    {
                        ok = Fail("upgrade #" + i + " has no id");
                        continue;
                    }

                    string prefix = "upgrade '" + upgrade.id + "': ";
                    if (upgrade.maxLevel <= 0) ok = Fail(prefix + "maxLevel must be > 0");
                    if (upgrade.baseCost <= 0) ok = Fail(prefix + "baseCost must be > 0");
                    if (upgrade.costGrowth < 1f) ok = Fail(prefix + "costGrowth must be >= 1");
                    if (upgrade.bonusPerLevel <= 0) ok = Fail(prefix + "bonusPerLevel must be > 0");
                }
            }

            if (balance.waves == null)
            {
                ok = Fail("'waves' is missing");
            }
            else
            {
                if (balance.game != null && balance.waves.Count != balance.game.totalWaves)
                {
                    ok = Fail("waves count (" + balance.waves.Count + ") != game.totalWaves (" +
                              balance.game.totalWaves + ")");
                }

                for (int i = 0; i < balance.waves.Count; i++)
                {
                    var wave = balance.waves[i];
                    if (wave == null)
                    {
                        ok = Fail("wave #" + i + " is null");
                        continue;
                    }

                    string prefix = "wave " + wave.wave + ": ";
                    if (wave.wave != i + 1) ok = Fail(prefix + "wave number must be " + (i + 1));
                    if (wave.spawnInterval <= 0) ok = Fail(prefix + "spawnInterval must be > 0");
                    if (wave.victoryAward < 0) ok = Fail(prefix + "victoryAward must be >= 0");
                    if (wave.jinn < 0 || wave.ghoul < 0 || wave.ifrit < 0 || wave.boss < 0)
                        ok = Fail(prefix + "enemy counts must be >= 0");
                }
            }

            var catalog = AssetLibrary.Catalog;
            if (catalog != null && catalog.assets != null)
            {
                var seen = new System.Collections.Generic.HashSet<string>();
                for (int i = 0; i < catalog.assets.Count; i++)
                {
                    var entry = catalog.assets[i];
                    if (entry == null || string.IsNullOrEmpty(entry.id)) continue;
                    if (!seen.Add(entry.id)) ok = Fail("duplicate sprite id in assets.json: " + entry.id);
                }
            }

            if (ok) Debug.Log("Config OK");
#endif

            return ok;
        }

        static bool Fail(string message)
        {
            Debug.LogError("[Config] " + message);
            return false;
        }
    }
}

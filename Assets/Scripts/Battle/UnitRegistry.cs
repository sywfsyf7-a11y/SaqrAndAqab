using System.Collections.Generic;
using UnityEngine;

namespace SaqrAndAqab.Battle
{
    // Live units per team. Plain lists only, no physics, no Find.
    public class UnitRegistry
    {
        static readonly List<Unit> PlayerUnits = new List<Unit>();
        static readonly List<Unit> EnemyUnits = new List<Unit>();

        public static void Add(Unit unit)
        {
            if (unit == null) return;

            if (unit.Team == Team.Player) PlayerUnits.Add(unit);
            else EnemyUnits.Add(unit);
        }

        public static void Remove(Unit unit)
        {
            if (unit == null) return;

            PlayerUnits.Remove(unit);
            EnemyUnits.Remove(unit);
        }

        public static List<Unit> Get(Team team)
        {
            return team == Team.Player ? PlayerUnits : EnemyUnits;
        }

        // Nearest alive unit of the given team within maxRange of x.
        public static Unit GetNearest(Team team, float x, float maxRange)
        {
            List<Unit> list = Get(team);
            Unit nearest = null;
            float nearestDistance = maxRange;

            for (int i = 0; i < list.Count; i++)
            {
                Unit unit = list[i];
                if (unit == null || unit.Health == null || !unit.Health.IsAlive) continue;

                float distance = Mathf.Abs(unit.transform.position.x - x);
                if (distance > nearestDistance) continue;

                if (nearest == null || distance < nearestDistance)
                {
                    nearest = unit;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        public static void Clear()
        {
            PlayerUnits.Clear();
            EnemyUnits.Clear();
        }
    }
}

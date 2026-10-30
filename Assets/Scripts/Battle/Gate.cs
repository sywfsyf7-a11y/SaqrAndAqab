using UnityEngine;

namespace SaqrAndAqab.Battle
{
    // The fortress gate: a player-side unit with health and no attack. When it dies the wave is lost.
    public class Gate : Unit
    {
        public static Gate Create(Transform parent, Vector3 position, int hp)
        {
            var go = new GameObject("Gate", typeof(Gate));
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = position;

            var gate = go.GetComponent<Gate>();
            gate.Setup(hp);
            return gate;
        }

        void Setup(int hp)
        {
            var stats = new UnitStats();
            stats.hp = hp;
            stats.damage = 0;
            stats.range = 0f;
            stats.cooldown = 0f;
            stats.speed = 0f;

            Init(Team.Player, stats);
        }
    }
}

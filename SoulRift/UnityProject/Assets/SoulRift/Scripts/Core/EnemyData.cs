using UnityEngine;

namespace SoulRift.Core
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "SoulRift/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public string EnemyName;
        public float MaxHealth = 20f;
        public float MoveSpeed = 2f;
        public float ContactDamage = 10f;
        public float SoulReward = 5f;
        [Range(0.1f, 3f)] public float CollisionRadius = 0.3f;
    }
}

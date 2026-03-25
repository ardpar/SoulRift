using UnityEngine;

namespace SoulRift.Core
{
    [CreateAssetMenu(fileName = "WaveData", menuName = "SoulRift/Wave Data")]
    public class WaveData : ScriptableObject
    {
        public int WaveNumber;
        public float Duration = 90f;
        public EnemySpawnEntry[] Spawns;
        public float SpawnInterval = 2f;
    }

    [System.Serializable]
    public class EnemySpawnEntry
    {
        public EnemyData EnemyType;
        public int Count = 10;
    }
}

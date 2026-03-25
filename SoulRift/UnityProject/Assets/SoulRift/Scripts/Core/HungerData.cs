using UnityEngine;

namespace SoulRift.Core
{
    /// <summary>
    /// Hollow Hunger mekanigi konfigurasyonu.
    /// </summary>
    [CreateAssetMenu(fileName = "HungerData", menuName = "SoulRift/Hunger Data")]
    public class HungerData : ScriptableObject
    {
        [Header("Hunger Stack")]
        [Range(0.5f, 5f)] public float StackInterval = 2.0f;
        [Range(1, 10)] public int MaxStacks = 3;
        [Range(1f, 10f)] public float SoulMultiplier = 3.0f;
    }
}

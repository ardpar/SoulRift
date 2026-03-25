using UnityEngine;

namespace SoulRift.Core
{
    /// <summary>
    /// Soul state esik degerleri ve carpanlari. Tum soul hesaplamalari bu data'dan okunur.
    /// </summary>
    [CreateAssetMenu(fileName = "SoulStateData", menuName = "SoulRift/Soul State Data")]
    public class SoulStateData : ScriptableObject
    {
        [Header("State Esikleri (SoulPercent olarak)")]
        [Range(0.05f, 0.40f)] public float HollowThreshold = 0.25f;
        [Range(0.30f, 0.75f)] public float StableThreshold = 0.60f;
        [Range(0.70f, 0.95f)] public float SurgingThreshold = 0.85f;
        [Range(0.80f, 0.98f)] public float OverflowThreshold = 0.90f;

        [Header("Hysteresis (sinirda titresimi onler)")]
        [Range(0.01f, 0.05f)] public float Hysteresis = 0.02f;

        [Header("Hasar Carpanlari")]
        public float HollowDamageMultiplier = 0.5f;
        public float StableDamageMultiplier = 1.0f;
        public float SurgingDamageMultiplier = 1.75f;
        public float OverflowDamageMultiplier = 3.0f;

        [Header("Hiz Carpanlari")]
        public float HollowSpeedMultiplier = 0.8f;
        public float StableSpeedMultiplier = 1.0f;
        public float SurgingSpeedMultiplier = 1.0f;
        public float OverflowSpeedMultiplier = 1.0f;

        [Header("Surge Warning")]
        [Range(1f, 8f)] public float WarningDuration = 3.5f;

        [Header("Shop")]
        [Range(0.1f, 0.5f)] public float HollowDiscount = 0.3f;
        [Range(1f, 4f)] public float CursedCostMultiplier = 2.0f;

        public float GetDamageMultiplier(SoulState state)
        {
            return state switch
            {
                SoulState.Hollow => HollowDamageMultiplier,
                SoulState.Stable => StableDamageMultiplier,
                SoulState.Surging => SurgingDamageMultiplier,
                SoulState.SurgeWarning => SurgingDamageMultiplier,
                SoulState.Overflow => OverflowDamageMultiplier,
                _ => 1f
            };
        }

        public float GetSpeedMultiplier(SoulState state)
        {
            return state switch
            {
                SoulState.Hollow => HollowSpeedMultiplier,
                SoulState.Stable => StableSpeedMultiplier,
                SoulState.Surging => SurgingSpeedMultiplier,
                SoulState.SurgeWarning => SurgingSpeedMultiplier,
                SoulState.Overflow => OverflowSpeedMultiplier,
                _ => 1f
            };
        }
    }
}

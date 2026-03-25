using System;
using UnityEngine;

namespace SoulRift.Core
{
    /// <summary>
    /// Core mechanic: Ruh yonetimi, 5 state, Hollow Hunger, Surge Warning.
    /// Tum diger sistemler bu sisteme bagimlidir.
    /// </summary>
    public class SoulSystem : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private SoulStateData _stateData;
        [SerializeField] private HungerData _hungerData;

        [Header("Runtime (Debug)")]
        [SerializeField] private float _currentSoul;
        [SerializeField] private float _maxSoul = 100f;
        [SerializeField] private SoulState _currentState = SoulState.Stable;
        [SerializeField] private int _hungerStacks;
        [SerializeField] private float _hungerTimer;
        [SerializeField] private float _warningTimer;

        // --- Public Properties ---
        public float CurrentSoul => _currentSoul;
        public float MaxSoul => _maxSoul;
        public float SoulPercent => _maxSoul > 0 ? _currentSoul / _maxSoul : 0f;
        public SoulState CurrentState => _currentState;
        public int HungerStacks => _hungerStacks;
        public float WarningTimeRemaining => _warningTimer;
        public float DamageMultiplier => _stateData.GetDamageMultiplier(_currentState);
        public float SpeedMultiplier => _stateData.GetSpeedMultiplier(_currentState);

        // --- Events ---
        public event Action<SoulState, SoulState> OnSoulStateChanged;
        public event Action<float> OnSoulValueChanged;
        public event Action<int> OnHungerStackChanged;
        public event Action OnHungerConsumed;
        public event Action OnOverflowEntered;
        public event Action OnWarningStarted;
        public event Action OnPlayerDeath;

        private SoulState _previousState;
        private bool _warningActive;

        private void Awake()
        {
            _currentSoul = _maxSoul * 0.5f; // Stable'da basla
            _previousState = SoulState.Stable;
            EvaluateState();
        }

        private void Update()
        {
            UpdateHunger();
            UpdateWarning();
        }

        // --- Public API ---

        /// <summary>
        /// Ruh ekle (dusman oldurme, Ruh orbu toplama vb.)
        /// </summary>
        public void AddSoul(float amount)
        {
            if (amount <= 0) return;

            // Hunger stack aktifse carpan uygula
            if (_hungerStacks > 0)
            {
                amount *= _hungerData.SoulMultiplier * _hungerStacks;
                _hungerStacks = 0;
                _hungerTimer = 0f;
                OnHungerConsumed?.Invoke();
                OnHungerStackChanged?.Invoke(0);
            }

            _currentSoul = Mathf.Min(_currentSoul + amount, _maxSoul);
            OnSoulValueChanged?.Invoke(SoulPercent);
            EvaluateState();
        }

        /// <summary>
        /// Ruh azalt (hasar alma, item satin alma vb.)
        /// </summary>
        public void RemoveSoul(float amount)
        {
            if (amount <= 0) return;

            // Overflow'da hasar = aninda olum
            if (_currentState == SoulState.Overflow && amount > 0)
            {
                _currentSoul = 0;
                OnSoulValueChanged?.Invoke(0);
                OnPlayerDeath?.Invoke();
                return;
            }

            _currentSoul = Mathf.Max(_currentSoul - amount, 0f);
            OnSoulValueChanged?.Invoke(SoulPercent);
            EvaluateState();
        }

        /// <summary>
        /// Shop'ta item satin alirken Ruh harca. Hollow indirimi otomatik.
        /// </summary>
        public float GetItemCost(float baseCost, bool isCursed, bool isFracturedCharacter)
        {
            float cost = baseCost;

            if (_currentState == SoulState.Hollow)
                cost *= (1f - _stateData.HollowDiscount);

            if (isCursed)
                cost *= isFracturedCharacter ? 1f : _stateData.CursedCostMultiplier;

            return cost;
        }

        /// <summary>
        /// Cursed item slotu acik mi?
        /// </summary>
        public bool IsCursedSlotAvailable(bool isFracturedCharacter)
        {
            if (isFracturedCharacter) return true;
            return _currentState == SoulState.Surging
                || _currentState == SoulState.SurgeWarning
                || _currentState == SoulState.Overflow;
        }

        /// <summary>
        /// MaxSoul'u ayarla (karakter pasifi icin, ornegin The Vessel %150)
        /// </summary>
        public void SetMaxSoul(float newMax)
        {
            _maxSoul = newMax;
            _currentSoul = Mathf.Min(_currentSoul, _maxSoul);
            EvaluateState();
        }

        // --- State Evaluation ---

        private void EvaluateState()
        {
            float percent = SoulPercent;
            SoulState newState = DetermineState(percent);

            if (newState != _currentState)
            {
                SoulState oldState = _currentState;
                _currentState = newState;

                // Warning timer yonetimi
                if (newState == SoulState.SurgeWarning && !_warningActive)
                {
                    _warningActive = true;
                    _warningTimer = _stateData.WarningDuration;
                    OnWarningStarted?.Invoke();
                }
                else if (newState != SoulState.SurgeWarning && newState != SoulState.Overflow)
                {
                    _warningActive = false;
                    _warningTimer = 0f;
                }

                if (newState == SoulState.Overflow)
                {
                    OnOverflowEntered?.Invoke();
                }

                // Hunger timer: sadece Hollow'da aktif
                if (newState != SoulState.Hollow)
                {
                    _hungerTimer = 0f;
                    // Stackler korunur ama timer durur
                }

                OnSoulStateChanged?.Invoke(oldState, newState);
            }
        }

        private SoulState DetermineState(float percent)
        {
            float hyst = _stateData.Hysteresis;

            // Mevcut state'e gore hysteresis uygula
            return _currentState switch
            {
                SoulState.Hollow => percent >= _stateData.HollowThreshold + hyst
                    ? SoulState.Stable : SoulState.Hollow,

                SoulState.Stable => percent < _stateData.HollowThreshold - hyst
                    ? SoulState.Hollow
                    : percent >= _stateData.StableThreshold + hyst
                        ? SoulState.Surging : SoulState.Stable,

                SoulState.Surging => percent < _stateData.StableThreshold - hyst
                    ? SoulState.Stable
                    : percent >= _stateData.SurgingThreshold
                        ? SoulState.SurgeWarning : SoulState.Surging,

                SoulState.SurgeWarning => percent < _stateData.SurgingThreshold - hyst
                    ? SoulState.Surging : SoulState.SurgeWarning,

                SoulState.Overflow => percent < _stateData.SurgingThreshold - hyst
                    ? SoulState.Surging : SoulState.Overflow,

                _ => SoulState.Stable
            };
        }

        // --- Hunger ---

        private void UpdateHunger()
        {
            if (_currentState != SoulState.Hollow) return;

            _hungerTimer += Time.deltaTime;

            if (_hungerTimer >= _hungerData.StackInterval)
            {
                _hungerTimer -= _hungerData.StackInterval;

                if (_hungerStacks < _hungerData.MaxStacks)
                {
                    _hungerStacks++;
                    OnHungerStackChanged?.Invoke(_hungerStacks);
                }
            }
        }

        // --- Warning ---

        private void UpdateWarning()
        {
            if (!_warningActive) return;
            if (_currentState != SoulState.SurgeWarning) return;

            _warningTimer -= Time.deltaTime;

            if (_warningTimer <= 0f)
            {
                _warningActive = false;
                _warningTimer = 0f;

                // Timer bitti, hala esik ustunde → Overflow
                if (SoulPercent >= _stateData.SurgingThreshold)
                {
                    _currentState = SoulState.Overflow;
                    OnOverflowEntered?.Invoke();
                    OnSoulStateChanged?.Invoke(SoulState.SurgeWarning, SoulState.Overflow);
                }
            }
        }
    }
}

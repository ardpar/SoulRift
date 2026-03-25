using UnityEngine;
using SoulRift.Core;

namespace SoulRift.Gameplay
{
    /// <summary>
    /// Player etrafinda Soul state'e gore aura rengi ve Hunger partikulleri yonetir.
    /// S1-13 (Soul VFX) + S1-16 (Hunger partikulleri)
    /// </summary>
    public class SoulVFXController : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private SoulSystem _soulSystem;
        [SerializeField] private SpriteRenderer _playerSprite;

        [Header("Aura Renkleri")]
        [SerializeField] private Color _hollowColor = new Color(0.5f, 0.5f, 0.55f);
        [SerializeField] private Color _stableColor = new Color(0.6f, 0.8f, 1f);
        [SerializeField] private Color _surgingColor = new Color(0.94f, 0.78f, 0.16f);
        [SerializeField] private Color _warningColor = new Color(0.98f, 0.57f, 0.24f);
        [SerializeField] private Color _overflowColor = new Color(0.95f, 0.2f, 0.2f);

        [Header("Aura Gecis")]
        [SerializeField] private float _colorLerpSpeed = 4f;

        [Header("Hunger Partikulleri")]
        [SerializeField] private ParticleSystem _hungerParticles;

        [Header("Hunger Partikul Ayarlari")]
        [SerializeField] private float _baseEmissionRate = 3f;
        [SerializeField] private float _perStackEmissionRate = 5f;

        private Color _targetColor;
        private ParticleSystem.EmissionModule _hungerEmission;
        private bool _hungerParticlesReady;

        private void Start()
        {
            if (_hungerParticles != null)
            {
                _hungerEmission = _hungerParticles.emission;
                _hungerEmission.rateOverTime = 0f;
                _hungerParticles.Stop();
                _hungerParticlesReady = true;
            }

            _targetColor = _stableColor;
            if (_playerSprite != null)
                _playerSprite.color = _targetColor;
        }

        private void OnEnable()
        {
            if (_soulSystem == null) return;
            _soulSystem.OnSoulStateChanged += HandleStateChanged;
            _soulSystem.OnHungerStackChanged += HandleHungerChanged;
            _soulSystem.OnHungerConsumed += HandleHungerConsumed;
        }

        private void OnDisable()
        {
            if (_soulSystem == null) return;
            _soulSystem.OnSoulStateChanged -= HandleStateChanged;
            _soulSystem.OnHungerStackChanged -= HandleHungerChanged;
            _soulSystem.OnHungerConsumed -= HandleHungerConsumed;
        }

        private void Update()
        {
            if (_playerSprite == null) return;

            // Overflow'da nabiz efekti
            if (_soulSystem != null && _soulSystem.CurrentState == SoulState.Overflow)
            {
                float pulse = Mathf.Sin(Time.time * 8f) * 0.3f + 0.7f;
                _playerSprite.color = Color.Lerp(Color.black, _overflowColor, pulse);
            }
            else
            {
                _playerSprite.color = Color.Lerp(_playerSprite.color, _targetColor, Time.deltaTime * _colorLerpSpeed);
            }
        }

        private void HandleStateChanged(SoulState oldState, SoulState newState)
        {
            _targetColor = GetStateColor(newState);

            // Hunger partikulleri sadece Hollow'da aktif
            if (_hungerParticlesReady)
            {
                if (newState != SoulState.Hollow)
                {
                    _hungerEmission.rateOverTime = 0f;
                    _hungerParticles.Stop();
                }
            }
        }

        private void HandleHungerChanged(int stacks)
        {
            if (!_hungerParticlesReady) return;

            if (stacks > 0)
            {
                if (!_hungerParticles.isPlaying)
                    _hungerParticles.Play();

                _hungerEmission.rateOverTime = _baseEmissionRate + _perStackEmissionRate * stacks;
            }
            else
            {
                _hungerEmission.rateOverTime = 0f;
                _hungerParticles.Stop();
            }
        }

        private void HandleHungerConsumed()
        {
            if (!_hungerParticlesReady) return;

            // Patlama efekti — stackler harcandı
            _hungerParticles.Emit(20);
            _hungerEmission.rateOverTime = 0f;
            _hungerParticles.Stop();
        }

        private Color GetStateColor(SoulState state)
        {
            return state switch
            {
                SoulState.Hollow => _hollowColor,
                SoulState.Stable => _stableColor,
                SoulState.Surging => _surgingColor,
                SoulState.SurgeWarning => _warningColor,
                SoulState.Overflow => _overflowColor,
                _ => _stableColor
            };
        }
    }
}

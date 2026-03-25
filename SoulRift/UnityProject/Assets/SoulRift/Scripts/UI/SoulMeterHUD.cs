using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SoulRift.Core;
using SoulRift.Gameplay;

namespace SoulRift.UI
{
    /// <summary>
    /// Soul meter HUD. Ruh yuzdesini ve state rengini gosterir.
    /// WaveManager.OnWaveStarted event'ini dinleyerek wave numarasini gunceller.
    /// </summary>
    public class SoulMeterHUD : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private SoulSystem _soulSystem;
        [SerializeField] private WaveManager _waveManager;

        [Header("UI Elemanlari")]
        [SerializeField] private Image _meterFill;
        [SerializeField] private TextMeshProUGUI _soulText;
        [SerializeField] private TextMeshProUGUI _stateText;
        [SerializeField] private TextMeshProUGUI _hungerText;
        [SerializeField] private TextMeshProUGUI _waveText;

        [Header("State Renkleri")]
        [SerializeField] private Color _hollowColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color _stableColor = Color.white;
        [SerializeField] private Color _surgingColor = new Color(0.94f, 0.7f, 0.16f);
        [SerializeField] private Color _warningColor = new Color(0.98f, 0.57f, 0.24f);
        [SerializeField] private Color _overflowColor = new Color(0.88f, 0.32f, 0.32f);

        private void OnEnable()
        {
            if (_soulSystem != null)
            {
                _soulSystem.OnSoulValueChanged += UpdateMeter;
                _soulSystem.OnSoulStateChanged += UpdateState;
                _soulSystem.OnHungerStackChanged += UpdateHunger;
            }
            if (_waveManager != null)
                _waveManager.OnWaveStarted += HandleWaveStarted;
        }

        private void OnDisable()
        {
            if (_soulSystem != null)
            {
                _soulSystem.OnSoulValueChanged -= UpdateMeter;
                _soulSystem.OnSoulStateChanged -= UpdateState;
                _soulSystem.OnHungerStackChanged -= UpdateHunger;
            }
            if (_waveManager != null)
                _waveManager.OnWaveStarted -= HandleWaveStarted;
        }

        private void Start()
        {
            UpdateMeter(_soulSystem != null ? _soulSystem.SoulPercent : 0.5f);
            if (_soulSystem != null)
                UpdateState(SoulState.Stable, _soulSystem.CurrentState);
        }

        private void UpdateMeter(float soulPercent)
        {
            if (_meterFill != null)
                _meterFill.fillAmount = soulPercent;

            if (_soulText != null)
                _soulText.text = $"{Mathf.RoundToInt(soulPercent * 100)}%";
        }

        private void UpdateState(SoulState oldState, SoulState newState)
        {
            Color stateColor = newState switch
            {
                SoulState.Hollow => _hollowColor,
                SoulState.Stable => _stableColor,
                SoulState.Surging => _surgingColor,
                SoulState.SurgeWarning => _warningColor,
                SoulState.Overflow => _overflowColor,
                _ => _stableColor
            };

            if (_meterFill != null)
                _meterFill.color = stateColor;

            if (_stateText != null)
                _stateText.text = newState.ToString();
        }

        private void UpdateHunger(int stacks)
        {
            if (_hungerText != null)
                _hungerText.text = stacks > 0 ? $"Hunger x{stacks}" : "";
        }

        private void HandleWaveStarted(int waveNumber)
        {
            SetWaveText(waveNumber);
        }

        public void SetWaveText(int wave)
        {
            if (_waveText != null)
                _waveText.text = $"Wave {wave}";
        }
    }
}

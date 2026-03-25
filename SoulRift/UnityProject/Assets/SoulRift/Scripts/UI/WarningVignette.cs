using UnityEngine;
using UnityEngine.UI;
using SoulRift.Core;

namespace SoulRift.UI
{
    /// <summary>
    /// Surge Warning ve Overflow'da ekran kenari vignette efekti.
    /// S1-15: Warning turuncu, Overflow kirmizi nabiz.
    /// </summary>
    public class WarningVignette : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private SoulSystem _soulSystem;
        [SerializeField] private Image _vignetteImage;

        [Header("Renkler")]
        [SerializeField] private Color _warningColor = new Color(0.98f, 0.57f, 0.24f, 0.3f);
        [SerializeField] private Color _overflowColor = new Color(0.95f, 0.15f, 0.15f, 0.45f);

        [Header("Animasyon")]
        [SerializeField] private float _fadeSpeed = 3f;
        [SerializeField] private float _pulseSpeed = 4f;

        private float _targetAlpha;
        private Color _targetColor;
        private bool _active;

        private void Start()
        {
            if (_vignetteImage != null)
            {
                var c = _vignetteImage.color;
                c.a = 0f;
                _vignetteImage.color = c;
                _vignetteImage.raycastTarget = false;
            }
        }

        private void OnEnable()
        {
            if (_soulSystem == null) return;
            _soulSystem.OnSoulStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            if (_soulSystem == null) return;
            _soulSystem.OnSoulStateChanged -= HandleStateChanged;
        }

        private void Update()
        {
            if (_vignetteImage == null) return;

            if (!_active)
            {
                // Fade out
                var c = _vignetteImage.color;
                c.a = Mathf.MoveTowards(c.a, 0f, Time.deltaTime * _fadeSpeed);
                _vignetteImage.color = c;
                return;
            }

            if (_soulSystem.CurrentState == SoulState.Overflow)
            {
                // Kirmizi nabiz
                float pulse = Mathf.Sin(Time.time * _pulseSpeed) * 0.5f + 0.5f;
                float alpha = Mathf.Lerp(_overflowColor.a * 0.5f, _overflowColor.a, pulse);
                _vignetteImage.color = new Color(_overflowColor.r, _overflowColor.g, _overflowColor.b, alpha);
            }
            else if (_soulSystem.CurrentState == SoulState.SurgeWarning)
            {
                // Warning timer'a gore alpha artar
                float timeRatio = 1f - (_soulSystem.WarningTimeRemaining /
                    (_soulSystem.WarningTimeRemaining + 0.01f)); // 0→1 arası
                float alpha = Mathf.Lerp(_warningColor.a * 0.3f, _warningColor.a, 0.5f + Mathf.Sin(Time.time * 3f) * 0.5f);
                _vignetteImage.color = new Color(_warningColor.r, _warningColor.g, _warningColor.b, alpha);
            }
        }

        private void HandleStateChanged(SoulState oldState, SoulState newState)
        {
            _active = newState == SoulState.SurgeWarning || newState == SoulState.Overflow;
        }
    }
}

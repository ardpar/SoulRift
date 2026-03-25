using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SoulRift.Core;

namespace SoulRift.Gameplay
{
    /// <summary>
    /// Oyun akisini yonetir: baslangic, olum, yeniden baslama, wave sayaci.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private SoulSystem _soulSystem;
        [SerializeField] private WaveManager _waveManager;
        [SerializeField] private PoolManager _poolManager;

        [Header("Prefab Warmup")]
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private int _enemyWarmup = 30;
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private int _projectileWarmup = 50;
        [SerializeField] private GameObject _soulOrbPrefab;
        [SerializeField] private int _soulOrbWarmup = 30;

        [Header("Game Over UI")]
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _gameOverText;
        [SerializeField] private Button _restartButton;

        private bool _gameOver;

        private void OnEnable()
        {
            if (_soulSystem != null)
                _soulSystem.OnPlayerDeath += HandlePlayerDeath;
        }

        private void OnDisable()
        {
            if (_soulSystem != null)
                _soulSystem.OnPlayerDeath -= HandlePlayerDeath;
        }

        private void Start()
        {
            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(false);
            if (_restartButton != null)
                _restartButton.onClick.AddListener(RestartGame);

            WarmupPools();
            _waveManager.StartFirstWave();
        }

        private void WarmupPools()
        {
            if (_poolManager == null) return;

            if (_enemyPrefab != null)
                _poolManager.Warmup(_enemyPrefab, _enemyWarmup);
            if (_projectilePrefab != null)
                _poolManager.Warmup(_projectilePrefab, _projectileWarmup);
            if (_soulOrbPrefab != null)
                _poolManager.Warmup(_soulOrbPrefab, _soulOrbWarmup);
        }

        private void HandlePlayerDeath()
        {
            if (_gameOver) return;
            _gameOver = true;

            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                if (_gameOverText != null)
                    _gameOverText.text = $"GAME OVER\nWave {_waveManager.CurrentWave}\n\n<size=24>Tikla veya R'ye bas</size>";
            }

            Time.timeScale = 0f;
            StartCoroutine(WaitForRestart());
        }

        private IEnumerator WaitForRestart()
        {
            // timeScale=0'da calismak icin realtime wait
            yield return new WaitForSecondsRealtime(0.5f);

            while (!UnityEngine.Input.GetKeyDown(KeyCode.R)
                && !UnityEngine.Input.GetMouseButtonDown(0))
            {
                yield return null;
            }

            RestartGame();
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}

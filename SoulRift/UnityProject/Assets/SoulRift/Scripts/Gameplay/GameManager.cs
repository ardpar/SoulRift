using UnityEngine;
using SoulRift.Core;

namespace SoulRift.Gameplay
{
    /// <summary>
    /// Oyun akisini yonetir: baslangic, olum, yeniden baslama.
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

            Debug.Log($"GAME OVER — Wave {_waveManager.CurrentWave}");
            Time.timeScale = 0f;
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}

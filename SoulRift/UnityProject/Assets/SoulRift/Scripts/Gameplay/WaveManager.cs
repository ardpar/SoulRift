using System;
using System.Collections;
using UnityEngine;
using SoulRift.Core;

namespace SoulRift.Gameplay
{
    /// <summary>
    /// Wave yoneticisi. Dusmanları spawn eder, wave ilerlemesini yonetir.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("Konfigürasyon")]
        [SerializeField] private WaveData[] _waves;
        [SerializeField] private Transform _player;
        [SerializeField] private float _spawnRadius = 8f;

        [Header("Dusman Prefab'lari")]
        [SerializeField] private GameObject _basicEnemyPrefab;

        [Header("Ruh Orbu")]
        [SerializeField] private GameObject _soulOrbPrefab;

        [Header("Runtime (Debug)")]
        [SerializeField] private int _currentWaveIndex;
        [SerializeField] private int _enemiesAlive;

        public int CurrentWave => _currentWaveIndex + 1;
        public int EnemiesAlive => _enemiesAlive;

        public event Action<int> OnWaveStarted;
        public event Action<int> OnWaveCompleted;

        private void OnEnable()
        {
            Enemy.OnEnemyDied += HandleEnemyDied;
        }

        private void OnDisable()
        {
            Enemy.OnEnemyDied -= HandleEnemyDied;
        }

        public void StartFirstWave()
        {
            _currentWaveIndex = 0;
            StartCoroutine(RunWave());
        }

        private IEnumerator RunWave()
        {
            // Son wave'den sonra tekrar son wave'i kullan (zorluk artarak)
            int waveIdx = Mathf.Min(_currentWaveIndex, _waves.Length - 1);
            WaveData wave = _waves[waveIdx];
            OnWaveStarted?.Invoke(_currentWaveIndex + 1);

            foreach (var spawnEntry in wave.Spawns)
            {
                for (int i = 0; i < spawnEntry.Count; i++)
                {
                    SpawnEnemy(spawnEntry.EnemyType);
                    yield return new WaitForSeconds(wave.SpawnInterval);
                }
            }

            // Tum dusmanlar spawn oldu, olmelirini bekle
            yield return new WaitUntil(() => _enemiesAlive <= 0);

            OnWaveCompleted?.Invoke(_currentWaveIndex + 1);
            _currentWaveIndex++;

            // Sonraki wave (shop arasi eklenecek)
            yield return new WaitForSeconds(1f);
            StartCoroutine(RunWave());
        }

        private void SpawnEnemy(EnemyData enemyData)
        {
            Vector2 spawnPos = GetSpawnPosition();
            GameObject enemyObj;

            if (PoolManager.Instance != null)
                enemyObj = PoolManager.Instance.Get(_basicEnemyPrefab, spawnPos, Quaternion.identity);
            else
                enemyObj = Instantiate(_basicEnemyPrefab, spawnPos, Quaternion.identity);

            if (enemyObj.TryGetComponent(out Enemy enemy))
                enemy.Init(enemyData, _player, _basicEnemyPrefab);

            _enemiesAlive++;
        }

        private Vector2 GetSpawnPosition()
        {
            if (_player == null) return Vector2.zero;

            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _spawnRadius;
            Vector2 pos = (Vector2)_player.position + offset;

            // Arena sinirlari icinde tut (duvar kalinligi 1 birim)
            pos.x = Mathf.Clamp(pos.x, -13.5f, 13.5f);
            pos.y = Mathf.Clamp(pos.y, -9.5f, 9.5f);
            return pos;
        }

        private void HandleEnemyDied(Enemy enemy)
        {
            _enemiesAlive = Mathf.Max(0, _enemiesAlive - 1);
            SpawnSoulOrb(enemy.transform.position, enemy.Data.SoulReward);
        }

        private void SpawnSoulOrb(Vector3 position, float soulValue)
        {
            if (_soulOrbPrefab == null) return;

            GameObject orbObj;

            if (PoolManager.Instance != null)
                orbObj = PoolManager.Instance.Get(_soulOrbPrefab, position, Quaternion.identity);
            else
                orbObj = Instantiate(_soulOrbPrefab, position, Quaternion.identity);

            if (orbObj.TryGetComponent(out SoulOrb orb))
                orb.Init(soulValue, _player, _soulOrbPrefab);
        }
    }
}

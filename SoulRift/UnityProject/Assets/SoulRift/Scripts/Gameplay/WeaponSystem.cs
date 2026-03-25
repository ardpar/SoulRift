using UnityEngine;
using SoulRift.Core;

namespace SoulRift.Gameplay
{
    /// <summary>
    /// Oyuncunun silah sistemi. Fire input'unu dinler, mermi spawn eder.
    /// Hasar carpanini Soul System'den alir.
    /// </summary>
    public class WeaponSystem : MonoBehaviour
    {
        [Header("Silah Ayarlari")]
        [SerializeField] private float _baseDamage = 10f;
        [SerializeField] private float _fireRate = 0.2f;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private GameObject _projectilePrefab;

        [Header("Referanslar")]
        [SerializeField] private SoulSystem _soulSystem;
        [SerializeField] private PlayerController _playerController;

        private float _fireCooldown;

        private void Update()
        {
            _fireCooldown -= Time.deltaTime;

            if (_playerController.IsFirePressed && _fireCooldown <= 0f)
            {
                Fire();
                _fireCooldown = _fireRate;
            }
        }

        private void Fire()
        {
            Vector2 aimDir = _playerController.AimDirection;
            if (aimDir.sqrMagnitude < 0.01f) return;

            float damage = _baseDamage * (_soulSystem != null ? _soulSystem.DamageMultiplier : 1f);

            Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position;
            GameObject bullet;

            if (PoolManager.Instance != null)
                bullet = PoolManager.Instance.Get(_projectilePrefab, spawnPos, Quaternion.identity);
            else
                bullet = Instantiate(_projectilePrefab, spawnPos, Quaternion.identity);

            if (bullet.TryGetComponent(out Projectile proj))
                proj.Init(aimDir, damage, _projectilePrefab);
        }
    }
}

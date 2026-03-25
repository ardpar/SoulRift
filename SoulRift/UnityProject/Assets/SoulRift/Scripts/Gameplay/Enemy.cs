using System;
using UnityEngine;
using SoulRift.Core;

namespace SoulRift.Gameplay
{
    /// <summary>
    /// Temel dusman. Oyuncuya dogru hareket eder, temas hasari verir,
    /// oldugunde Ruh orbu birakir.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Enemy : MonoBehaviour, IPoolable
    {
        [SerializeField] private EnemyData _data;

        private float _currentHealth;
        private Rigidbody2D _rb;
        private Transform _target;
        private GameObject _prefabRef;

        public static event Action<Enemy> OnEnemyDied;

        public EnemyData Data => _data;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }

        public void Init(EnemyData data, Transform target, GameObject prefabRef)
        {
            _data = data;
            _target = target;
            _prefabRef = prefabRef;
            _currentHealth = data.MaxHealth;
        }

        private void FixedUpdate()
        {
            if (_target == null) return;

            Vector2 direction = ((Vector2)_target.position - (Vector2)transform.position).normalized;
            _rb.linearVelocity = direction * _data.MoveSpeed;
        }

        public void TakeDamage(float damage)
        {
            _currentHealth -= damage;

            if (_currentHealth <= 0f)
                Die();
        }

        private void Die()
        {
            OnEnemyDied?.Invoke(this);

            if (_prefabRef != null && PoolManager.Instance != null)
                PoolManager.Instance.Release(gameObject, _prefabRef);
            else
                gameObject.SetActive(false);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent(out SoulSystem soul))
            {
                soul.RemoveSoul(_data.ContactDamage * Time.fixedDeltaTime);
            }
        }

        public void OnSpawn()
        {
            _currentHealth = _data != null ? _data.MaxHealth : 20f;
        }

        public void OnDespawn()
        {
            _rb.linearVelocity = Vector2.zero;
            _target = null;
        }
    }
}

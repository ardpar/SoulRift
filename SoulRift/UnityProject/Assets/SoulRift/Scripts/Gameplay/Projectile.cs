using UnityEngine;
using SoulRift.Core;

namespace SoulRift.Gameplay
{
    /// <summary>
    /// Pooled mermi. Ates edildiginde yon ve hasar alir, dusmana carpinca hasar verir.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _speed = 12f;
        [SerializeField] private float _lifetime = 3f;

        private float _damage;
        private float _timer;
        private Rigidbody2D _rb;
        private GameObject _prefabRef;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
        }

        public void Init(Vector2 direction, float damage, GameObject prefabRef)
        {
            _damage = damage;
            _prefabRef = prefabRef;
            _rb.linearVelocity = direction.normalized * _speed;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _lifetime)
                ReturnToPool();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out Enemy enemy))
            {
                enemy.TakeDamage(_damage);
                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            if (_prefabRef != null && PoolManager.Instance != null)
                PoolManager.Instance.Release(gameObject, _prefabRef);
            else
                gameObject.SetActive(false);
        }

        public void OnSpawn()
        {
            _timer = 0f;
        }

        public void OnDespawn()
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }
}

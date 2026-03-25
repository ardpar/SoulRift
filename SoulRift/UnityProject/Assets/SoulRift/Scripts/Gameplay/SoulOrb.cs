using UnityEngine;
using SoulRift.Core;

namespace SoulRift.Gameplay
{
    /// <summary>
    /// Dusman oldugunde birakilan Ruh orbu.
    /// Oyuncuya yakinlasinca cekilir ve toplanir.
    /// </summary>
    public class SoulOrb : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _attractRadius = 2f;
        [SerializeField] private float _attractSpeed = 8f;
        [SerializeField] private float _collectRadius = 0.3f;

        private float _soulValue;
        private Transform _target;
        private GameObject _prefabRef;
        private bool _attracted;

        public void Init(float soulValue, Transform target, GameObject prefabRef)
        {
            _soulValue = soulValue;
            _target = target;
            _prefabRef = prefabRef;
            _attracted = false;
        }

        private void Update()
        {
            if (_target == null) return;

            float dist = Vector2.Distance(transform.position, _target.position);

            if (dist <= _collectRadius)
            {
                Collect();
                return;
            }

            if (dist <= _attractRadius)
                _attracted = true;

            if (_attracted)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    _target.position,
                    _attractSpeed * Time.deltaTime
                );
            }
        }

        private void Collect()
        {
            if (_target.TryGetComponent(out SoulSystem soul))
                soul.AddSoul(_soulValue);

            if (_prefabRef != null && PoolManager.Instance != null)
                PoolManager.Instance.Release(gameObject, _prefabRef);
            else
                gameObject.SetActive(false);
        }

        public void OnSpawn()
        {
            _attracted = false;
        }

        public void OnDespawn()
        {
            _target = null;
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using SoulRift.Core;

namespace SoulRift.Gameplay
{
    /// <summary>
    /// Top-down twin-stick hareket ve aim. Soul System'den hiz carpani alir.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Hareket")]
        [SerializeField] private float _baseSpeed = 5f;

        [Header("Referanslar")]
        [SerializeField] private SoulSystem _soulSystem;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private Vector2 _aimDirection;
        private Camera _mainCamera;

        private PlayerInputActions _inputActions;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _mainCamera = Camera.main;

            _inputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _inputActions.Gameplay.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Gameplay.Disable();
        }

        private void Update()
        {
            _moveInput = _inputActions.Gameplay.Move.ReadValue<Vector2>();
            UpdateAim();
        }

        private void FixedUpdate()
        {
            float speedMultiplier = _soulSystem != null ? _soulSystem.SpeedMultiplier : 1f;
            _rb.linearVelocity = _moveInput.normalized * (_baseSpeed * speedMultiplier);
        }

        private void UpdateAim()
        {
            Vector2 mouseScreenPos = _inputActions.Gameplay.Aim.ReadValue<Vector2>();
            Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
            _aimDirection = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;

            if (_aimDirection.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(_aimDirection.y, _aimDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
            }
        }

        public Vector2 AimDirection => _aimDirection;
        public bool IsFirePressed => _inputActions.Gameplay.Fire.IsPressed();
    }
}

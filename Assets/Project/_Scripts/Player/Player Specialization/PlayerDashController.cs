using UnityEngine;
using Audio;

namespace Player
{
    /// <summary>
    /// Responsbile for handling the player dash mechanic.
    /// </summary>
    public sealed class PlayerDashController : MonoBehaviour
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("References")]
        [SerializeField] private FPSController _controller;
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private SoundData _dashSound;
        
        [Space(10.0f)]
        
        [Header("Dash Settings")]
        [SerializeField] [Min(0.01f)] private float _dashForce = 10.0f;
        [SerializeField] [Min(0.01f)] private float _dashDuration = 0.12f;
        [SerializeField] [Min(0.01f)] private float _dashCooldown = 1.5f;

        // ───── Public properties ────────────────────────────────────────────────
        
        public float CooldownProgress => _cooldownTimer <= 0.0f ? 1.0f : 1.0f - (_cooldownTimer / _dashCooldown);
        
        public bool DashReady => _cooldownTimer <= 0.0f;

        // ───── Private properties ────────────────────────────────────────────────
        
        private PlayerInputActions _input;
        
        private Vector3 _dashDirection;
        
        private float _cooldownTimer;
        private float _dashTimer;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void Awake()
        {
            Debug.Assert(_controller != null, "[PlayerDashController] FPSController not assigned.", this);
            Debug.Assert(_characterController != null, "[PlayerDashController] CharacterController not assigned.", this);
            
            _input = new PlayerInputActions();
        }
        
        private void OnEnable()
        {
            _input.Player.Enable();
            _input.Player.Dash.performed += _ => TryDash();
        }
        
        private void OnDisable() => _input.Player.Disable();
        
        private void Update()
        {
            if (_cooldownTimer > 0.0f)
                _cooldownTimer -= Time.deltaTime;
            
            if (_dashTimer > 0.0f)
            {
                _dashTimer -= Time.deltaTime;
                _characterController.Move(_dashDirection * (_dashForce * Time.deltaTime));
            }
        }

        // ───── Private methods ────────────────────────────────────────────────
        
        private void TryDash()
        {
            if (!DashReady || !_controller.IsGrounded || _controller.IsCrouching) { return; }
            
            _dashDirection = GetDashDirection();
            _dashTimer = _dashDuration;
            _cooldownTimer = _dashCooldown;
            
            if (_dashSound ==  null) { return; }
            
            AudioManager.Instance?.Play(_dashSound, transform.position);
        }
        
        private Vector3 GetDashDirection()
        {
            Vector2 moveInput = _input.Player.Move.ReadValue<Vector2>();
            
            if (Camera.main == null) { return Vector3.zero; }
            
            Vector3 camForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized;
            
            if (moveInput.sqrMagnitude < 0.01f)
                return camForward;
            
            return (camForward * moveInput.y + camRight * moveInput.x).normalized;
        }
    }
}

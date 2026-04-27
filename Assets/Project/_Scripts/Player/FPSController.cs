using UnityEngine;
using UnityEngine.InputSystem;
using Audio;

namespace Player
{
    /// <summary>
    /// First person character controller.
    ///
    /// Authorised Use Instructions:
    ///     - MUST NOT own the camera — camera is driven by FPSCamera.
    ///     - MUST NOT own input — reads from PlayerInputActions.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class FPSController : MonoBehaviour
    {
        // ─── Serialized properties ────────────────────────────────────────
        
        [Header("References")]
        [SerializeField] private Transform _camera;
        [SerializeField] private FootstepSoundController _footstepSoundController;
        
        [Space(10.0f)]
        
        [Header("Movement")]
        [SerializeField] private float _walkSpeed = 5.5f;
        [SerializeField] private float _crouchSpeed = 2.5f;
        [SerializeField] [Range(1.0f, 30.0f)] private float _acceleration = 12.0f;
        [SerializeField] [Range(1.0f, 30.0f)] private float _deceleration = 16.0f;
        [SerializeField] [Range(0.0f, 2.0f)]  private float _airControlFactor = 0.3f; // < 1.0f = Less air control ... > 1.0f = More air control

        [Space(10.0f)]
        
        [Header("Jumping")]
        [SerializeField] private float _jumpForce = 6.0f;
        [SerializeField] [Range(0.0f, 0.5f)] private float _coyoteTime = 0.15f;

        [Space(10.0f)]
        
        [Header("Gravity")]
        [SerializeField] private float _gravityScale = 2.5f;
        [SerializeField] private float _maxFallSpeed = 20.0f;

        [Space(10.0f)]
        
        [Header("Ground Detection")]
        [SerializeField] private float _groundSnapForce = 5.0f;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private float _groundCheckRadius = 0.25f;
        [SerializeField] private float _groundCheckDistance = 0.15f;

        [Space(10.0f)]
        
        [Header("Crouch")]
        [SerializeField] public PlayerCrouchSettings _crouchSettings;

        [Space(10.0f)]
        
        [Header("Overlap Check")]
        [SerializeField] private LayerMask _obstacleMask; // LayerMask of objects player shouldn't stand inside of

        // ─── Private properties ────────────────────────────────────────────

        private CharacterController  _controller;
        private PlayerInputActions   _input;
        private PlayerInventory _inventory;

        // Velocity
        private Vector3 _velocity;
        private Vector3 _horizontalVelocity;
        private float   _verticalVelocity;

        // Ground state
        private bool  _isGrounded;
        private bool  _wasGrounded;
        private float _coyoteTimer;

        // Crouch state
        private bool  _isCrouching;
        private float _currentHeight;
        private float _targetHeight;

        // Input values
        private Vector2 _moveInput;

        // ─── Public properties ───────────────────────────────────────────────

        public bool IsGrounded => _isGrounded;
        public bool IsCrouching => _isCrouching;
        public Vector3 Velocity => _velocity;
        public float NormalisedSpeed   => _horizontalVelocity.magnitude / (_isCrouching ? _crouchSpeed : _walkSpeed);
        public float WalkSpeed => _walkSpeed;

        // ─── Lifecycle methods ──────────────────────────────────────────

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _inventory = GetComponent<PlayerInventory>();

            Debug.Assert(_controller != null,"[FPSController] Missing CharacterController.", this);
            Debug.Assert(_crouchSettings != null,"[FPSController] Missing CrouchSettings.",      this);
            Debug.Assert(_inventory != null, "[FPSController] Missing PlayerInventory.", this);

            _input = new PlayerInputActions();

            _currentHeight = _crouchSettings.StandingHeight;
            _targetHeight  = _crouchSettings.StandingHeight;

            _controller.height = _currentHeight;
            _controller.center = Vector3.up * (_currentHeight * 0.5f);
        }

        private void OnEnable()
        {
            _input.Player.Enable();
            _input.Player.Jump.performed   += OnJumpPerformed;
            _input.Player.Crouch.performed += OnCrouchPerformed;
            _input.Player.Crouch.canceled  += OnCrouchCanceled;
            _input.Player.UseHealthKit.performed += OnUseHealthKitPerformed;
        }

        private void OnDisable()
        {
            _input.Player.Jump.performed   -= OnJumpPerformed;
            _input.Player.Crouch.performed -= OnCrouchPerformed;
            _input.Player.Crouch.canceled  -= OnCrouchCanceled;
            _input.Player.UseHealthKit.performed -= OnUseHealthKitPerformed;
            _input.Player.Disable();
        }

        private void Update()
        {
            _moveInput = _input.Player.Move.ReadValue<Vector2>();

            UpdateGroundState();
            UpdateCrouch();
            UpdateMovement();
            UpdateGravity();
            ApplyMovement();
        }

        // ─── Input callbacks ──────────────────────────────────────────

        private void OnJumpPerformed(InputAction.CallbackContext ctx)
        {
            if (_verticalVelocity > 0.0f) { return; }
            
            bool canJump = _isGrounded || _coyoteTimer > 0.0f;

            if (!canJump)     { return; }
            if (_isCrouching) { TryStand(); return; }

            _verticalVelocity = _jumpForce;
            _coyoteTimer      = 0.0f;
        }

        private void OnCrouchPerformed(InputAction.CallbackContext ctx)
        {
            if (_crouchSettings.Mode == PlayerCrouchSettings.CrouchMode.Toggle)
            {
                if (_isCrouching)
                    TryStand();
                else
                    Crouch();
            }
            else
            {
                Crouch();
            }
        }

        private void OnCrouchCanceled(InputAction.CallbackContext ctx)
        {
            if (_crouchSettings.Mode == PlayerCrouchSettings.CrouchMode.Hold && _isCrouching)
            {
                TryStand();
            }
        }
        
        private void OnUseHealthKitPerformed(InputAction.CallbackContext ctx)
        {
            _inventory?.TryUseHealthKit();
        }

        // ─── Private methods ──────────────────────────────────────────

        /// <summary>
        /// Sphere cast downward to detect ground.
        /// </summary>
        private void UpdateGroundState()
        {
            _wasGrounded = _isGrounded;

            Vector3 sphereOrigin = transform.position + Vector3.up * _groundCheckRadius;

            _isGrounded = Physics.SphereCast(sphereOrigin,_groundCheckRadius,Vector3.down,out RaycastHit hit,_groundCheckDistance + _groundCheckRadius,_groundMask);

            // Also accept CharacterController's own ground check
            // as a fallback — catches edge cases on flat geometry
            if (!_isGrounded)
                _isGrounded = _controller.isGrounded;

            // Coyote time
            if (_wasGrounded && !_isGrounded)
                _coyoteTimer = _coyoteTime;
            else if (_isGrounded)
                _coyoteTimer = 0.0f;
            else
                _coyoteTimer -= Time.deltaTime;
        }

        /// <summary>
        /// Smoothly interpolates controller height for crouch transitions.
        /// </summary>
        private void UpdateCrouch()
        {
            if (Mathf.Approximately(_currentHeight, _targetHeight)) { return; }

            _currentHeight = Mathf.Lerp(_currentHeight,_targetHeight,1.0f - Mathf.Pow(0.1f,Time.deltaTime * _crouchSettings.CrouchSpeed));

            // Snap when close enough
            if (Mathf.Abs(_currentHeight - _targetHeight) < 0.01f)
                _currentHeight = _targetHeight;

            _controller.height = _currentHeight;
            _controller.center = Vector3.up * (_currentHeight * 0.5f);
        }

        /// <summary>
        /// Calculates horizontal velocity
        /// </summary>
        private void UpdateMovement()
        {
            float targetSpeed = _isCrouching ? _crouchSpeed : _walkSpeed;

            Vector3 inputDir = new Vector3(_moveInput.x, 0.0f, _moveInput.y).normalized;
            
            Quaternion cameraYawRotation = Quaternion.Euler(0.0f, _camera.eulerAngles.y, 0.0f);
            Vector3 targetVelocity = cameraYawRotation * inputDir * targetSpeed;

            // Reduce control in air
            float controlFactor = _isGrounded ? 1.0f : _airControlFactor;

            if (inputDir.magnitude > 0.0f)
            {
                // Accelerate toward target
                _horizontalVelocity = Vector3.Lerp(_horizontalVelocity,targetVelocity,1.0f - Mathf.Pow(0.1f,Time.deltaTime * _acceleration * controlFactor));
            }
            else
            {
                // Decelerate toward zero
                _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, Vector3.zero,1.0f - Mathf.Pow(0.1f,Time.deltaTime * _deceleration * controlFactor));

                // Snap to zero when nearly stopped
                if (_horizontalVelocity.magnitude < 0.01f)
                    _horizontalVelocity = Vector3.zero;
            }
        }

        private void UpdateGravity()
        {
            if (_isGrounded && _verticalVelocity < 0.0f)
            {
                // Apply snap force to keep controller on slopes/stairs
                _verticalVelocity = -_groundSnapForce;
            }
            else
            {
                // Apply gravity
                _verticalVelocity += Physics.gravity.y * _gravityScale * Time.deltaTime;

                // Clamp to terminal velocity
                _verticalVelocity = Mathf.Max(_verticalVelocity, -_maxFallSpeed);
            }
        }

        private void ApplyMovement()
        {
            _velocity = _horizontalVelocity + Vector3.up * _verticalVelocity;

            _controller.Move(_velocity * Time.deltaTime);
        }

        /// <summary>
        /// Initiates crouch — sets target height.
        /// </summary>
        private void Crouch()
        {
            if (_isCrouching) { return; }

            _isCrouching = true;
            _targetHeight = _crouchSettings.CrouchingHeight;
            
            _footstepSoundController?.SetCrouching(true);
        }

        /// <summary>
        /// Attempts to stand. Checks for overhead obstruction first —
        /// prevents standing inside geometry.
        /// </summary>
        private void TryStand()
        {
            if (!_isCrouching) { return; }

            // Check for overhead obstruction
            float heightDiff      = _crouchSettings.StandingHeight - _crouchSettings.CrouchingHeight;
            Vector3 overheadOrigin = transform.position + Vector3.up * _crouchSettings.CrouchingHeight;

            bool obstructed = Physics.SphereCast(overheadOrigin,_controller.radius * 0.9f, Vector3.up, out RaycastHit _,heightDiff, _obstacleMask);

            if (obstructed) { return; }

            _isCrouching  = false;
            _targetHeight = _crouchSettings.StandingHeight;
            
            _footstepSoundController?.SetCrouching(false);
        }
    }
}
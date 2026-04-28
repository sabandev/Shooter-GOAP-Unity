using UnityEngine;

namespace Player
{
    /// <summary>
    /// Drives the player's Animator Controller parameters from
    /// FPSController state each frame.
    ///
    /// Reads velocity from FPSController, converts to local space
    /// relative to the body transform, and writes normalised
    /// SpeedX/SpeedZ to drive the 2D locomotion blend tree.
    ///
    /// Also drives IsCrouching, IsGrounded, and exposes methods
    /// for PlayerBodyRotation to fire turn triggers.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public sealed class PlayerAnimator : MonoBehaviour
    {
        // ─── Serialized fields ────────────────────────────────────────

        [Header("References")]
        [Tooltip("The FPSController on the player root.")]
        [SerializeField] private FPSController _controller;

        [Header("Smoothing")]
        [Tooltip("Damp time for SpeedX and SpeedZ parameters. " +
                 "Higher = smoother but less responsive blend.")]
        [SerializeField] [Range(0.0f, 0.5f)]
        private float _speedDampTime = 0.1f;

        // ─── Pre-hashed parameter IDs ─────────────────────────────────

        private static readonly int _speedXHash =
            Animator.StringToHash("SpeedX");
        private static readonly int _speedZHash =
            Animator.StringToHash("SpeedZ");
        private static readonly int _isCrouchingHash =
            Animator.StringToHash("IsCrouching");
        private static readonly int _isGroundedHash =
            Animator.StringToHash("IsGrounded");
        private static readonly int _turnLeftHash =
            Animator.StringToHash("TurnLeft");
        private static readonly int _turnRightHash =
            Animator.StringToHash("TurnRight");
        private static readonly int _isTurningHash =
            Animator.StringToHash("IsTurning");

        // ─── Private state ────────────────────────────────────────────

        private Animator _animator;

        // ─── Properties ───────────────────────────────────────────────

        /// <summary>
        /// Normalised movement speed 0-1.
        /// Read by PlayerBodyRotation to suppress turns during movement.
        /// </summary>
        public float NormalisedSpeed { get; private set; }

        // ─── Unity lifecycle ──────────────────────────────────────────

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            Debug.Assert(_animator   != null,
                "[PlayerAnimator] Missing Animator.",      this);
            Debug.Assert(_controller != null,
                "[PlayerAnimator] Missing FPSController.", this);
        }

        private void Update()
        {
            UpdateLocomotion();
            UpdateStates();
        }

        // ─── Public API ───────────────────────────────────────────────

        /// <summary>
        /// Fires the TurnLeft trigger. Called by PlayerBodyRotation
        /// when body yaw needs to catch up to camera yaw.
        /// </summary>
        public void TriggerTurnLeft()
        {
            _animator.ResetTrigger(_turnRightHash);
            _animator.SetTrigger(_turnLeftHash);
            _animator.SetBool(_isTurningHash, true);
        }

        /// <summary>
        /// Fires the TurnRight trigger.
        /// </summary>
        public void TriggerTurnRight()
        {
            _animator.ResetTrigger(_turnLeftHash);
            _animator.SetTrigger(_turnRightHash);
            _animator.SetBool(_isTurningHash, true);
        }

        /// <summary>
        /// Clears the IsTurning flag. Called by PlayerBodyRotation
        /// when the turn is complete.
        /// </summary>
        public void EndTurn()
        {
            _animator.SetBool(_isTurningHash, false);
        }

        // ─── Private methods ──────────────────────────────────────────

        private void UpdateLocomotion()
        {
            // Get world velocity from controller, flatten to XZ
            Vector3 worldVelocity   = _controller.Velocity;
            worldVelocity.y         = 0.0f;

            // Convert to local space relative to this body transform
            // so SpeedZ is forward/back relative to facing direction
            // and SpeedX is left/right relative to facing direction
            Vector3 localVelocity   =
                transform.InverseTransformDirection(worldVelocity);

            float maxSpeed          = _controller.WalkSpeed;

            float normX = maxSpeed > 0.0f
                ? Mathf.Clamp(localVelocity.x / maxSpeed, -1.0f, 1.0f)
                : 0.0f;

            float normZ = maxSpeed > 0.0f
                ? Mathf.Clamp(localVelocity.z / maxSpeed, -1.0f, 1.0f)
                : 0.0f;

            NormalisedSpeed = new Vector2(normX, normZ).magnitude;

            _animator.SetFloat(_speedXHash, normX,
                _speedDampTime, Time.deltaTime);
            _animator.SetFloat(_speedZHash, normZ,
                _speedDampTime, Time.deltaTime);
        }

        private void UpdateStates()
        {
            _animator.SetBool(_isCrouchingHash, _controller.IsCrouching);
            _animator.SetBool(_isGroundedHash,  _controller.IsGrounded);
        }
    }
}
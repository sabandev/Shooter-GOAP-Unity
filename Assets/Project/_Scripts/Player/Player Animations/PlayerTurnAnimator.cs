using UnityEngine;

namespace Player
{
    /// <summary>
    /// Manages the player body's yaw rotation independently from
    /// the camera.
    ///
    /// The body follows the camera yaw but only rotates past a
    /// configurable threshold. Below the threshold the body stays
    /// put and feet adjust naturally via iStep.
    ///
    /// When the threshold is exceeded the body snaps toward the
    /// camera yaw and fires a turn animation via PlayerAnimator.
    /// </summary>
    public sealed class PlayerTurnAnimator : MonoBehaviour
    {
        // ─── Serialized fields ────────────────────────────────────────

        [Header("References")]
        [Tooltip("The camera transform whose yaw this body follows.")]
        [SerializeField] private Transform _camera;

        [Tooltip("PlayerAnimator on the body mesh child.")]
        [SerializeField] private PlayerAnimator _playerAnimator;

        [Header("Turn Settings")]
        [Tooltip("Angle in degrees the camera must diverge from body " +
                 "before a turn is triggered.")]
        [SerializeField] [Range(30.0f, 90.0f)]
        private float _turnThreshold = 60.0f;

        [Tooltip("How quickly the body rotates to match camera yaw " +
                 "during a turn animation.")]
        [SerializeField] [Range(60.0f, 360.0f)]
        private float _turnSpeed = 180.0f;

        [Tooltip("Minimum speed before turns are suppressed. " +
                 "Prevents turn animations firing during locomotion " +
                 "where the blend tree handles direction naturally.")]
        [SerializeField] [Range(0.0f, 1.0f)]
        private float _maxSpeedForTurn = 0.15f;

        // ─── Private state ────────────────────────────────────────────

        private float _bodyYaw;
        private float _targetYaw;
        private bool  _isTurning;

        // ─── Unity lifecycle ──────────────────────────────────────────

        private void Awake()
        {
            Debug.Assert(_camera         != null,
                "[PlayerBodyRotation] Camera not assigned.",         this);
            Debug.Assert(_playerAnimator != null,
                "[PlayerBodyRotation] PlayerAnimator not assigned.", this);

            _bodyYaw   = transform.eulerAngles.y;
            _targetYaw = _bodyYaw;
        }

        private void Update()
        {
            float cameraYaw = _camera.eulerAngles.y;

            if (_isTurning)
            {
                TickTurning(cameraYaw);
            }
            else
            {
                TickDetection(cameraYaw);
            }

            // Apply body yaw
            transform.rotation = Quaternion.Euler(0.0f, _bodyYaw, 0.0f);
        }

        // ─── Private methods ──────────────────────────────────────────

        private void TickDetection(float cameraYaw)
        {
            // Suppress turns during movement — locomotion blend tree
            // handles direction changes while walking
            if (_playerAnimator.NormalisedSpeed > _maxSpeedForTurn)
            {
                // While moving, body yaw tracks camera yaw directly
                _bodyYaw = cameraYaw;
                return;
            }

            float angleDiff = Mathf.DeltaAngle(_bodyYaw, cameraYaw);

            if (Mathf.Abs(angleDiff) < _turnThreshold) { return; }

            // Threshold exceeded — start turn
            _isTurning  = true;
            _targetYaw  = cameraYaw;

            if (angleDiff > 0.0f)
                _playerAnimator.TriggerTurnRight();
            else
                _playerAnimator.TriggerTurnLeft();
        }

        private void TickTurning(float cameraYaw)
        {
            // Rotate body toward target yaw at turn speed
            _bodyYaw = Mathf.MoveTowardsAngle(
                _bodyYaw,
                _targetYaw,
                _turnSpeed * Time.deltaTime);

            float remaining = Mathf.Abs(
                Mathf.DeltaAngle(_bodyYaw, _targetYaw));

            // Turn complete when close enough
            if (remaining < 2.0f)
            {
                _bodyYaw   = _targetYaw;
                _isTurning = false;
                _playerAnimator.EndTurn();
            }
        }
    }
}
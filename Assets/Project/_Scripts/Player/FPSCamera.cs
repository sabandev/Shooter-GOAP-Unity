using UnityEngine;

namespace Player
{
    /// <summary>
    /// Drives the FPS camera.
    /// </summary>
    public sealed class FPSCamera : MonoBehaviour
    {
        // ─── Serialized properies ────────────────────────────────────────
        [Header("References")]
        [SerializeField] private FPSController _controller;

        [Space(10.0f)]
        
        [Header("Look")]
        [SerializeField] private float _sensitivity = 0.15f;
        [SerializeField] [Range(60.0f, 90.0f)] private float _maxPitchAngle = 85.0f;
        [SerializeField] [Range(0.0f, 0.9f)] private float _lookSmoothing = 0.05f; // The higher, the smoother but laggier

        [Space(10.0f)]
        
        [Header("Eye Height")]
        [SerializeField] private PlayerCrouchSettings _crouchSettings;
        [SerializeField] [Range(1.0f, 20.0f)] private float _eyeHeightSpeed = 12.0f; // How fast camera lerps between eye heights

        [Space(10.0f)]
        
        [Header("Head Bob")]
        [SerializeField] private PlayerHeadBobSettings _standingBob;
        [SerializeField] private PlayerHeadBobSettings _crouchingBob;

        // ─── Private properties ────────────────────────────────────────────

        private PlayerInputActions _input;

        // Look 
        private float   _pitch;
        private float   _yaw;
        private Vector2 _smoothedLookDelta;

        // Eye height 
        private float _currentEyeHeight;
        private float _targetEyeHeight;

        // Head bob 
        private float   _bobTimer;
        private float   _bobWeight;
        private Vector3 _bobOffset;

        // ─── Lifecycle methods ──────────────────────────────────────────

        private void Awake()
        {
            Debug.Assert(_controller != null, "[FPSCamera] Missing FPSController.",   this);
            Debug.Assert(_crouchSettings != null, "[FPSCamera] Missing CrouchSettings.",  this);
            Debug.Assert(_standingBob != null,"[FPSCamera] Missing standing bob settings.", this);
            Debug.Assert(_crouchingBob != null,"[FPSCamera] Missing crouching bob settings.", this);

            _input = new PlayerInputActions();

            _currentEyeHeight = _crouchSettings.StandingEyeHeight;
            _targetEyeHeight  = _crouchSettings.StandingEyeHeight;

            _yaw = _controller.transform.eulerAngles.y;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        private void OnEnable() => _input.Player.Enable();
        private void OnDisable() => _input.Player.Disable();

        private void Update()
        {
            UpdateLook();
            UpdateEyeHeight();
            UpdateHeadBob();
            ApplyTransform();
        }

        // ─── Private methods ──────────────────────────────────────────
        
        private void UpdateLook()
        {
            Vector2 lookDelta = _input.Player.Look.ReadValue<Vector2>() * _sensitivity;

            // Smooth mouse input
            _smoothedLookDelta = Vector2.Lerp( _smoothedLookDelta, lookDelta, 1.0f - Mathf.Pow(0.1f, Time.deltaTime * 60.0f * (1.0f - _lookSmoothing)));

            _yaw += _smoothedLookDelta.x;
            _pitch -= _smoothedLookDelta.y;
            _pitch = Mathf.Clamp(_pitch, -_maxPitchAngle, _maxPitchAngle);
            
            _controller.transform.rotation = Quaternion.Euler(0.0f, _yaw, 0.0f);
        }
        
        private void UpdateEyeHeight()
        {
            _targetEyeHeight = _controller.IsCrouching ? _crouchSettings.CrouchingEyeHeight : _crouchSettings.StandingEyeHeight;

            _currentEyeHeight = Mathf.Lerp(_currentEyeHeight,_targetEyeHeight,1.0f - Mathf.Pow(0.1f,Time.deltaTime * _eyeHeightSpeed));
        }

        /// <summary>
        /// Calculates head bob offset based on controller speed
        /// and current stance. Different settings for standing
        /// and crouching.
        /// </summary>
        private void UpdateHeadBob()
        {
            PlayerHeadBobSettings bob = _controller.IsCrouching ? _crouchingBob : _standingBob;

            float speed = _controller.NormalisedSpeed;

            float targetWeight = speed >= bob.MinSpeedThreshold && _controller.IsGrounded? speed: 0.0f;

            _bobWeight = Mathf.Lerp(_bobWeight,targetWeight,1.0f - Mathf.Pow(0.1f, Time.deltaTime * bob.BlendSpeed));

            if (_bobWeight > 0.001f)
            {
                // Advance bob timer by speed so bobs faster
                _bobTimer += Time.deltaTime * bob.Frequency *
                             _controller.NormalisedSpeed;

                float vertical   = Mathf.Sin(_bobTimer * Mathf.PI * 2.0f) * bob.VerticalAmplitude;
                float horizontal = Mathf.Cos(_bobTimer * Mathf.PI) * bob.HorizontalAmplitude;

                _bobOffset = new Vector3(horizontal, vertical, 0.0f) * _bobWeight;
            }
            else
            {
                // Smoothly return to zero when stopping
                _bobOffset = Vector3.Lerp(_bobOffset, Vector3.zero,1.0f - Mathf.Pow(0.1f,Time.deltaTime * bob.BlendSpeed));

                if (_bobOffset.magnitude < 0.0001f)
                    _bobOffset = Vector3.zero;
            }
        }
        
        private void ApplyTransform()
        {
            transform.position = _controller.transform.position + Vector3.up * _currentEyeHeight + _bobOffset;
            
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0.0f);
        }
    }
}
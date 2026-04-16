using UnityEngine;

namespace Player
{
    /// <summary>
    /// Rotates the spine bone by a fraction of the camera pitch
    /// in LateUpdate — after the Animator has applied its pose.
    ///
    /// This makes the body feel responsive to vertical aim without
    /// needing procedural IK or separate aim animations.
    /// Subtle values (0.2-0.4) look natural. Over 0.5 looks wrong.
    /// </summary>
    public sealed class PlayerSpineAnimator : MonoBehaviour
    {
        // ─── Serialized fields ────────────────────────────────────────

        [Header("References")]
        [Tooltip("The camera whose pitch drives the spine lean.")]
        [SerializeField] private Transform _camera;

        [Tooltip("The spine bone to rotate. Usually 'Spine' or " +
                 "'mixamorig:Spine' on Mixamo/Basic Motions characters.")]
        [SerializeField] private Transform _spineBone;

        [Header("Lean Settings")]
        [Tooltip("Fraction of camera pitch applied to spine. " +
                 "0.3 = spine rotates 30% of camera pitch. " +
                 "Start here and tune by eye.")]
        [SerializeField] [Range(0.0f, 0.6f)]
        private float _leanFraction = 0.3f;

        [Tooltip("How quickly the spine lean blends in and out. " +
                 "Prevents snapping when looking direction changes quickly.")]
        [SerializeField] [Range(1.0f, 20.0f)]
        private float _leanSpeed = 8.0f;

        // ─── Private state ────────────────────────────────────────────

        private float _currentLean;

        // ─── Unity lifecycle ──────────────────────────────────────────

        private void Awake()
        {
            Debug.Assert(_camera    != null,
                "[PlayerSpineLean] Camera not assigned.",     this);
            Debug.Assert(_spineBone != null,
                "[PlayerSpineLean] Spine bone not assigned.", this);
        }

        private void LateUpdate()
        {
            // Get camera pitch — convert Unity's 0-360 to -180 to 180
            float pitch = _camera.localEulerAngles.x;
            if (pitch > 180.0f) pitch -= 360.0f;

            // Smooth the lean
            float targetLean = pitch * _leanFraction;

            _currentLean = Mathf.Lerp(
                _currentLean,
                targetLean,
                1.0f - Mathf.Pow(0.1f, Time.deltaTime * _leanSpeed));

            // Apply to spine bone — X rotation = pitch
            _spineBone.Rotate(_currentLean, 0.0f, 0.0f, Space.Self);
        }
    }
}
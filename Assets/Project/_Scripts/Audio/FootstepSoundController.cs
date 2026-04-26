using UnityEngine;

namespace Audio
{
    /// <summary>
    /// Plays footstep sounds driven by animation events.
    /// Shared between player and AI.
    ///
    /// Authorised Use Instructions:
    ///     - MUST place this on the same GameObject as the Animator.
    ///     - MUST add animation events to walk/run clips calling
    ///       LeftFootstep() AND RightFoostep()
    /// </summary>
    public sealed class FootstepSoundController : MonoBehaviour
    {
        // ───── Nested type ────────────────────────────────────────────────
        public enum FootstepMode
        {
            AnimationEvents,
            StrideTimer,
        }
        
        // ─── Serialized properties ────────────────────────────────────────

        [Header("Mode")]
        [SerializeField] private FootstepMode _mode = FootstepMode.AnimationEvents;
        
        [Header("Sounds")]
        [SerializeField] private FootstepSounds _footstepSounds;

        [Space(10.0f)]
        
        [Header("Surface Detection")]
        [SerializeField] private float _groundCheckDistance = 1.5f;
        [SerializeField] private LayerMask _groundMask;

        [Space(10.0f)]
        
        [Header("Volume Scaling")]
        [SerializeField] [Range(0.0f, 1.0f)] private float _crouchVolumeScale = 0.4f;
        
        [Space(10.0f)]
        
        [Header("Stride Timer Settings")]
        [SerializeField] private float _strideLength = 1.4f;
        [SerializeField] private float _crouchStrideLength = 1.0f;
        [SerializeField] private float _minSpeed = 0.1f;
        
        [Space(10.0f)]
        
        [Header("Animation Event Settings")]
        [SerializeField] private float _footstepCooldown = 1.0f;

        // ─── Private properties ────────────────────────────────────────────

        private Vector3 _lastPosition;
        
        private float _lastFootstepTime;
        private float _strideAccumulator;
        
        private bool _isCrouching;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void Start()
        {
            _lastPosition = transform.position;
        }
        
        private void Update()
        {
            if (_mode == FootstepMode.StrideTimer)
                TickStrideTimer();
        }
        
        // ─── Public methods ──────────────────

        /// <summary>
        /// Called by animation event on left foot strike frame.
        /// </summary>
        public void FootstepLeft()
        {
            if (_mode == FootstepMode.StrideTimer) { return;}
            if (Time.time - _lastFootstepTime < _footstepCooldown) { return; }
            
            _lastFootstepTime = Time.time;
            PlayFootstep();
        }

        /// <summary>
        /// Called by animation event on right foot strike frame.
        /// </summary>
        public void FootstepRight()
        {
            if (_mode == FootstepMode.StrideTimer) { return; }
            if (Time.time - _lastFootstepTime < _footstepCooldown) { return; }
            
            _lastFootstepTime = Time.time;
            PlayFootstep();
        }

        public void SetCrouching(bool isCrouching)
        {
            _isCrouching = isCrouching;
        }

        // ─── Private methods ──────────────────────────────────────────

        /// <summary>
        /// Accumulates distance travelled, when it passes the stride length threshold,
        /// plays a footstep sound. Used to play footstep sounds on entities with
        /// unreliable movement animations.
        /// </summary>
        private void TickStrideTimer()
        {
            Vector3 currentPosition = transform.position;
            float distanceMoved = Vector3.Distance(currentPosition, _lastPosition);
            _lastPosition = currentPosition;
            
            float speed = distanceMoved / Time.deltaTime;
            
            if (speed < _minSpeed)
            {
                _strideAccumulator = 0.0f;
                return;
            }
            
            _strideAccumulator += distanceMoved;
            
            float stride = _isCrouching ? _crouchStrideLength : _strideLength;
            
            if (_strideAccumulator >= stride)
            {
                _strideAccumulator -= stride;
                
                if (Time.time - _lastFootstepTime < _footstepCooldown) { return; }
                _lastFootstepTime = Time.time;
                
                PlayFootstep();
            }
        }
        
        private void PlayFootstep()
        {
            if (AudioManager.Instance == null) { return; }

            SoundData sound = GetSurfaceSound();
            if (sound == null)              { return; }

            float volumeMultiplier = _isCrouching ? _crouchVolumeScale : 1.0f;
            
            AudioManager.Instance.Play(sound, transform.position, volumeMultiplier);
        }

        private SoundData GetSurfaceSound()
        {
            if (_footstepSounds == null) { return null; }

            if (Physics.Raycast(transform.position + Vector3.up * 0.1f,Vector3.down,out RaycastHit hit,_groundCheckDistance,_groundMask))
            {
                SurfaceType surface = hit.collider.GetComponentInParent<SurfaceType>();

                SurfaceType.Surface type = surface != null ? surface.Type : SurfaceType.Surface.Default;

                return _footstepSounds.GetSound(type);
            }

            return null;
        }
        
        
    }
}
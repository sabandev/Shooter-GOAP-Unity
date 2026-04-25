using UnityEngine;

namespace Audio
{
    /// <summary>
    /// Data asset for describing how a sound should be played in the game world.
    ///
    /// Authorised Use Instructions:
    ///     - AudioManager.Play must play the clips, THIS class should NEVER
    ///       play AudioClips directly.
    /// </summary>
    [CreateAssetMenu(fileName = "SoundData_New", menuName = "Audio/Sound Data")]
    public sealed class SoundData : ScriptableObject
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("Clips")]
        [SerializeField] private AudioClip[] _clips;
        
        [Space(10.0f)]
        
        [Header("Volume")]
        [SerializeField] [Range(0.0f, 1.0f)] private float _minVolume = 0.8f;
        [SerializeField] [Range(0.0f, 1.0f)] private float _maxVolume = 1.0f;
        
        [Space(10.0f)]
        
        [Header("Pitch")] // Random pitch range. 0.95-1.05 for subtle variation. Wider for more variation
        [SerializeField] [Range(0.1f, 3.0f)] private float _minPitch = 0.95f;
        [SerializeField] [Range(0.1f, 3.0f)] private float _maxPitch = 1.05f;
        
        [Space(10.0f)]
        
        [Header("Spatial")] // 0 = 2D (e.g. UI sounds) | 1 = 3D (world sounds)
        [SerializeField] [Range(0.0f, 1.0f)] private float _spatialBlend = 1.0f;
        [SerializeField] [Min(0.0f)] private float _maxDistance = 30.0f; // Max distance sound hits 0 volume
        [SerializeField] [Min(0.0f)] private float _minDistance = 1.0f;
        
        [Space(10.0f)]
        
        [Header("Category")]
        [SerializeField] private AudioCategory _category = AudioCategory.SFX;

        // ───── Public properties ────────────────────────────────────────────────
        public AudioCategory Category => _category;
        
        public float SpatialBlend => _spatialBlend;
        public float MaxDistance => _maxDistance;
        public float MinDistance => _minDistance;
        
        public bool IsValid => _clips != null && _clips.Length > 0;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void OnValidate()
        {
            _minVolume = Mathf.Clamp(_minVolume, 0.0f, _maxVolume);
            _maxVolume = Mathf.Clamp(_maxVolume, _minVolume, 1.0f);
            _minPitch = Mathf.Clamp(_minPitch, 0.1f, _maxPitch);
            _maxPitch = Mathf.Clamp(_maxPitch, _minPitch, 3.0f);
            _minDistance = Mathf.Max(0.0f, _minDistance);
            _maxDistance = Mathf.Max(_minDistance + 0.01f, _maxDistance);
            
            Debug.Assert(_clips != null && _clips.Length > 0, "[SoundData] No AudioClips assigned. Must assign at least 1 AudioClip.", this);
        }
        
        // ───── Public methods ────────────────────────────────────────────────
        public AudioClip GetClip()
        {
            if (_clips == null || _clips.Length == 0) { return null; }
            return _clips[Random.Range(0, _clips.Length)];
        }
        
        public float GetVolume() => Random.Range(_minVolume, _maxVolume);
        public float GetPitch() => Random.Range(_minPitch, _maxPitch);
    }
}


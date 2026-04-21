using UnityEngine;

namespace Audio
{
    /// <summary>
    /// Maps surface type to footstep SoundData asset.
    /// Shared between AI and player.
    /// </summary>
    [CreateAssetMenu(fileName = "FootstepSounds_New", menuName = "Audio/Footstep Sounds")]
    public sealed class FootstepSounds : ScriptableObject
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [SerializeField] private SoundData _default;
        [SerializeField] private SoundData _metal;
        [SerializeField] private SoundData _wood;
        [SerializeField] private SoundData _concrete;
        [SerializeField] private SoundData _flesh;
        [SerializeField] private SoundData _glass;

        // ───── Public methods ────────────────────────────────────────────────
        
        public SoundData GetSound(SurfaceType.Surface surface)
        {
            SoundData sound = surface switch
            {
                SurfaceType.Surface.Metal => _metal,
                SurfaceType.Surface.Wood => _wood,
                SurfaceType.Surface.Concrete => _concrete,
                SurfaceType.Surface.Flesh => _flesh,
                SurfaceType.Surface.Glass => _glass,
                _ => _default
            };
            
            return sound != null ? sound : _default;
        }
    }
}

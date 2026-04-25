using UnityEngine;

namespace Weapons
{
    /// <summary>
    /// Fades out the muzzle flash light over its lifetime.
    /// </summary>
    public sealed class MuzzleFlashLight : MonoBehaviour
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [SerializeField] private Light _light;
        [SerializeField] private float _initialIntensity = 5.0f;
        [SerializeField] private float _fadeSpeed        = 20.0f;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void OnEnable()
        {
            if (_light != null)
                _light.intensity = _initialIntensity;
        }

        private void Update()
        {
            if (_light == null) { return; }

            _light.intensity = Mathf.MoveTowards(_light.intensity,0.0f,Time.deltaTime * _fadeSpeed);
        }
    }
}

using UnityEngine;

namespace Weapons
{
    /// <summary>
    /// Represents a special hitbox on a GameObject that can be used to apply
    /// a damage multiplier.
    /// </summary>
    public sealed class Hitbox : MonoBehaviour
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("Damage Multiplier Setting")]
        [SerializeField] [Min(0.0f)] private float _damageMultiplier = 1.0f;

        // ───── Public properties ────────────────────────────────────────────────
        
        public float DamageMultiplier => _damageMultiplier;
        
        public IHealth RootHealth { get; private set; }
        public IDamageContext RootDamageContext { get; private set; }

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void Awake()
        {
            RootHealth = GetComponentInParent<IHealth>();
            RootDamageContext = GetComponentInParent<IDamageContext>();
            
            Debug.Assert(RootHealth != null, "[Hitbox] IHealth is not assigned in parent.", this);
            Debug.Assert(RootDamageContext != null, "[Hitbox] IDamageContext is not assigned in parent.", this);
        }
    }
}
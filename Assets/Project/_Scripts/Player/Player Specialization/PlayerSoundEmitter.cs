using UnityEngine;
using Weapons;

namespace Player
{
    /// <summary>
    /// Responsible for emitting player sounds into the environment for listeners
    /// through sound events.
    /// </summary>
    public sealed class PlayerSoundEmitter : MonoBehaviour
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("References")]
        [SerializeField] private WeaponHolder _weaponHolder;
        [SerializeField] private FPSController _controller;
        
        [Space(10.0f)]
        
        [Header("Gunshot sounds")]
        [SerializeField] [Min(0.0f)] private float _gunshotRadius = 30.0f;
        
        [Space(10.0f)]
        
        [Header("Footstep sounds")]
        [SerializeField] [Min(0.0f)] private float _walkFootstepRadius = 6.0f;
        [SerializeField] [Min(0.0f)] private float _crouchFootstepRadius = 2.0f;

        // ───── Private properties ────────────────────────────────────────────────
        
        private Weapon _subscribedWeapon;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void OnEnable()
        {
            _weaponHolder.OnWeaponEquipped += OnWeaponEquipped;
            _weaponHolder.OnWeaponUnequipped += OnWeaponUnequipped;
        }
        
        private void OnDisable()
        {
            _weaponHolder.OnWeaponEquipped -= OnWeaponEquipped;
            _weaponHolder.OnWeaponUnequipped -= OnWeaponUnequipped;
            UnsubscribeFromWeapon();
        }

        // ───── Public methods ────────────────────────────────────────────────
        
        /// <summary>
        /// Called by FootstepSoundController.
        /// </summary>
        public void EmitFootstep()
        {
            float radius = _controller.IsCrouching ? _crouchFootstepRadius : _walkFootstepRadius;
            SoundEventRegistry.Register(transform.position, radius);
        }

        // ───── Private methods ────────────────────────────────────────────────
        
        private void OnWeaponEquipped(Weapon weapon, int slot)
        {
            UnsubscribeFromWeapon();
            _subscribedWeapon = weapon;
            _subscribedWeapon.OnFired += OnWeaponFired;
        }
        
        private void OnWeaponUnequipped(int slot) => UnsubscribeFromWeapon();
        
        private void OnWeaponFired(Vector3 hitPoint) => SoundEventRegistry.Register(transform.position, _gunshotRadius);
        
        private void UnsubscribeFromWeapon()
        {
            if (_subscribedWeapon == null) { return; }
            
            _subscribedWeapon.OnFired -= OnWeaponFired;
            _subscribedWeapon = null;
        }
    }
}
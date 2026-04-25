using UnityEngine;
using Weapons;

namespace Player
{
    /// <summary>
    /// Reads weapon input and delegates to WeaponHolder.
    /// </summary>
    [RequireComponent(typeof(WeaponHolder))]
    public sealed class PlayerWeaponInput : MonoBehaviour
    {
        // ───── Private properties ────────────────────────────────────────────────
        
        private WeaponHolder _weaponHolder;
        private PlayerInputActions _input;
        private bool _fireHeld;
        private bool _firePressedThisFrame;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void Awake()
        {
            _weaponHolder = GetComponent<WeaponHolder>();
            _input        = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _input.Player.Enable();
            _input.Player.Fire.performed += ctx =>
            {
                _fireHeld = true;
                _firePressedThisFrame = true;
            };
            _input.Player.Fire.canceled         += ctx => _fireHeld = false;
            _input.Player.Reload.performed      += ctx => _weaponHolder.TryReload();
            _input.Player.WeaponSlot1.performed += ctx => _weaponHolder.EquipSlot(0);
            _input.Player.WeaponSlot2.performed += ctx => _weaponHolder.EquipSlot(1);
            _input.Player.CycleWeaponButton.performed += ctx => _weaponHolder.CycleWeapon();
            _input.Player.ScrollWeapon.performed += ctx =>
            {
                float scroll = ctx.ReadValue<float>();
                if (scroll != 0.0f)
                    _weaponHolder.CycleWeapon();
            };
            _input.Player.DropWeapon.performed  += ctx => _weaponHolder.DropActiveWeapon();
        }

        private void OnDisable()
        {
            _input.Player.Disable();
        }

        private void Update()
        {
            Weapon active = _weaponHolder.ActiveWeapon;
            
            if (active != null)
            {
                switch (active.Data.Mode)
                {
                    case WeaponData.FireMode.FullAuto:
                        if (_fireHeld)
                            _weaponHolder.TryFire();
                        break;
                    
                    case WeaponData.FireMode.SemiAuto:
                        if (_firePressedThisFrame)
                            _weaponHolder.TryFire();
                        break;
                }
            }
            
            _firePressedThisFrame = false;
        }
    }
}
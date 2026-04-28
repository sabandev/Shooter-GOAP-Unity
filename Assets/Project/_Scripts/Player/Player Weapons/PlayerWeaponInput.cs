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
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("References")]
        [SerializeField] private PlayerAimController _aimController;
        [SerializeField] private PlayerLeanController _leanController;
        
        [Space(10.0f)]
        
        [Header("Settings")]
        [SerializeField] private bool _toggleAim = false;
        
        // ───── Private properties ────────────────────────────────────────────────
        
        private WeaponHolder _weaponHolder;
        private PlayerInputActions _input;
        
        private bool _fireHeld;
        private bool _firePressedThisFrame;
        private bool _leanLeftHeld;
        private bool _leanRightHeld;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void Awake()
        {
            _weaponHolder = GetComponent<WeaponHolder>();
            _input        = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _input.Player.Enable();
            
            // Fire
            _input.Player.Fire.performed += ctx =>
            {
                _fireHeld = true;
                _firePressedThisFrame = true;
            };
            _input.Player.Fire.canceled         += ctx => _fireHeld = false;
            
            // Aim
            _input.Player.Aim.performed += _ =>
            {
                if (_aimController == null) { return; }
                if (_toggleAim)
                    _aimController.SetAiming(!_aimController.IsAiming);
                else
                    _aimController.SetAiming(true);
            };
            _input.Player.Aim.canceled += _ =>
            {
                if (_aimController == null || _toggleAim) { return; }
                _aimController.SetAiming(false);
            };
            
            // Reload
            _input.Player.Reload.performed      += ctx => _weaponHolder.TryReload();
            
            // Weapon slot navigation
            _input.Player.WeaponSlot1.performed += ctx => _weaponHolder.EquipSlot(0);
            _input.Player.WeaponSlot2.performed += ctx => _weaponHolder.EquipSlot(1);
            _input.Player.CycleWeaponButton.performed += ctx => _weaponHolder.CycleWeapon();
            _input.Player.ScrollWeapon.performed += ctx =>
            {
                float scroll = ctx.ReadValue<float>();
                if (scroll != 0.0f)
                    _weaponHolder.CycleWeapon();
            };
            
            // Weapon drop
            _input.Player.DropWeapon.performed  += ctx => _weaponHolder.DropActiveWeapon();
            
            // Lean
            _input.Player.LeanLeft.performed += _ => { _leanLeftHeld = true; UpdateLeanInput(); };
            _input.Player.LeanLeft.canceled += _ => { _leanLeftHeld = false; UpdateLeanInput(); };
            _input.Player.LeanRight.performed += _ => { _leanRightHeld = true; UpdateLeanInput(); };
            _input.Player.LeanRight.canceled += _ => { _leanRightHeld = false; UpdateLeanInput(); };
        }

        private void OnDisable()
        {
            if (!_toggleAim)
                _aimController?.SetAiming(false);
            
            _leanLeftHeld = _leanLeftHeld = false;
            _leanController?.SetLeanInput(0.0f);
            
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

        // ───── Private methods ────────────────────────────────────────────────
        
        private void UpdateLeanInput()
        {
            float input = 0.0f;
            if (_leanRightHeld) input += 1.0f;
            if (_leanLeftHeld) input -= 1.0f;
            
            _leanController?.SetLeanInput(input);
        }
    }
}
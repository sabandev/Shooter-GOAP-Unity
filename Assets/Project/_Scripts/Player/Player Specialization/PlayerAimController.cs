using System;
using UnityEngine;
using Weapons;

namespace Player
{
    /// <summary>
    /// Responsible for aiming-down-sights behaviour for the player
    /// </summary>
    public sealed class PlayerAimController : MonoBehaviour
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("References")]
        [SerializeField] private Camera _camera;
        [SerializeField] private FPSCamera _fpsCamera;
        [SerializeField] private WeaponHolder _weaponHolder;
        [SerializeField] private ViewModelSway _viewModelSway;
        
        [Space(10.0f)]
        
        [Header("Aim Settings")]
        [SerializeField] [Range(1.0f, 20.0f)] private float _fovTransitionSpeed = 12.0f;
        [SerializeField] [Range(0.0f, 1.0f)] private float _aimSensitivityMultiplier = 0.6f;
        [SerializeField] [Range(0.0f, 1.0f)] private float _aimSwayMultiplier = 0.25f;

        // ───── Public properties ────────────────────────────────────────────────
        
        public event Action<bool> OnAimChanged;
        
        public bool IsAiming { get; private set; }
        
        public float AimT { get; private set; }

        // ───── Private properties ────────────────────────────────────────────────
        
        private float _weaponAimFOVOffset;
        private float _baseFOV;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void Awake()
        {
            Debug.Assert(_camera != null, "[PlayerAimController] Camera not assigned.", this);
            Debug.Assert(_fpsCamera != null, "[PlayerAimController] FPSCamera not assigned.", this);
            Debug.Assert(_weaponHolder != null, "[PlayerAimController] WeaponHolder not assigned.", this);
            Debug.Assert(_viewModelSway != null, "[PlayerAimController] ViewModelSway not assigned.", this);
            
            _baseFOV = _camera.fieldOfView;
        }
        
        private void OnEnable()
        {                                                                                                                                                        
            _weaponHolder.OnWeaponEquipped   += OnWeaponEquipped;                                                                                                                                                               
            _weaponHolder.OnWeaponUnequipped += OnWeaponUnequipped;
        }                                                                                                                                                                                                                       
            
        private void OnDisable()
        {
            _weaponHolder.OnWeaponEquipped   -= OnWeaponEquipped;                                                                                                                                                               
            _weaponHolder.OnWeaponUnequipped -= OnWeaponUnequipped;
            SetAiming(false);                                                                                                                                                                                                   
        }       
                                                                                                                                                                                                                            
        private void Update()
        {                                                                                                                                                                                                                       
            // Smooth aim blend
            float targetT = IsAiming ? 1.0f : 0.0f;
            AimT = Mathf.Lerp(AimT, targetT, 1.0f - Mathf.Pow(0.1f, Time.deltaTime * _fovTransitionSpeed));                                                                                                                     

            // FOV                                                                                                                                                                                                              
            _camera.fieldOfView = _baseFOV + _weaponAimFOVOffset * AimT;
                                                                                                                                                                                                                            
            // Sensitivity — scale down proportionally with the zoom                                                                                                                                                            
            float fovRatio = _camera.fieldOfView / _baseFOV;
            float sensitivity = Mathf.Lerp(1.0f, _aimSensitivityMultiplier * fovRatio, AimT);                                                                                                                                   
            _fpsCamera.SetSensitivityMultiplier(sensitivity);                                                                                                                                                                   
                                                                                                                                                                                                                                
            // Sway                                                                                                                                                                                                             
            if (_viewModelSway != null)
                _viewModelSway.SetAimMultiplier(1.0f - AimT * (1.0f - _aimSwayMultiplier));
        }                                                                                                                                                                                                                       

        // ─── Private methods ──────────────────────────────────────────                                                                                                                                                       
        
        private void OnWeaponEquipped(Weapon weapon, int slot)
        {
            _weaponAimFOVOffset = weapon.Data.AimFOVOffset;
            weapon.SetAiming(IsAiming);                                                                                                                                                                                         
        }
                                                                                                                                                                                                                            
        private void OnWeaponUnequipped(int slot)                                                                                                                                                                               
        {
            _weaponAimFOVOffset = 0.0f;                                                                                                                                                                                         
        }

        // ───── Public methods ────────────────────────────────────────────────
        
        public void SetAiming(bool value)                                                                                                                                                                                      
        {       
            if (IsAiming == value) { return; }
            
            IsAiming = value;
            _weaponHolder.ActiveWeapon?.SetAiming(value);
            OnAimChanged?.Invoke(value);                                                                                                                                                                                        
        }
    }
}

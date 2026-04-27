using Player;
using UnityEngine;
using Weapons;

namespace UI
{
    /// <summary>
    /// Responsible for handling the crosshair appearance.
    /// </summary>
    public sealed class CrosshairUI : MonoBehaviour
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("References")]
        [SerializeField] private WeaponHolder _weaponHolder;
        [SerializeField] private FPSController _controller;
        [SerializeField] private PlayerAimController _playerAimController;
        
        [Space(10.0f)]
        
        [Header("Crosshair Bars")]
        [SerializeField] private RectTransform _top;
        [SerializeField] private RectTransform _bottom;                                                                                                                                                                         
        [SerializeField] private RectTransform _left;
        [SerializeField] private RectTransform _right;
        
        [Space(10.0f)]
        
        [Header("Spread")]
        [SerializeField] [Min(1.0f)] private float _baseSpread = 40.0f;
        [SerializeField] [Min(1.0f)] private float _aimSpread = 12.0f;
        [SerializeField] [Min(1.0f)] private float _moveSpreadBonus = 30.0f;
        [SerializeField] [Min(1.0f)] private float _fireBloom = 10.0f;
        [SerializeField] [Range(1.0f, 20.0f)] private float _recoverySpeed = 8.0f;
        [SerializeField] [Range(1.0f, 20.0f)] private float _transitionSpeed = 14.0f;

        // ───── Private properties ────────────────────────────────────────────────
        
        private Weapon _subscribedWeapon;
        
        private float _currentBloom;
        private float _currentSpread;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void Awake()
        {
            Debug.Assert(_weaponHolder != null, "[CrosshairUI] WeaponHolder not assigned.", this);
            Debug.Assert(_controller != null, "[CrosshairUI] FPSController not assigned.", this);
            Debug.Assert(_playerAimController != null, "[CrosshairUI] PlayerAimController not assigned.", this);
        }
        
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
        
        private void Update()
        {
            _currentBloom = Mathf.Lerp(_currentBloom, 0.0f, 1.0f - Mathf.Pow(0.1f, Time.deltaTime * _recoverySpeed));
            
            float aimT = _playerAimController != null ? _playerAimController.AimT : 0.0f;
            float target = Mathf.Lerp(_baseSpread, _aimSpread, aimT) + _controller.NormalisedSpeed * _moveSpreadBonus * (1.0f - aimT) + _currentBloom;                                                                                                                                                                                       
                  
            _currentSpread = Mathf.Lerp(_currentSpread, target, 1.0f - Mathf.Pow(0.1f, Time.deltaTime * _transitionSpeed));
                                                                                                                                                                                                                                  
            if (_top != null) _top.anchoredPosition    = new Vector2( 0f,            _currentSpread);                                                                                                                        
            if (_bottom != null) _bottom.anchoredPosition = new Vector2( 0f,           -_currentSpread);
            if (_left != null) _left.anchoredPosition   = new Vector2(-_currentSpread, 0f);                                                                                                                                   
            if (_right != null) _right.anchoredPosition  = new Vector2( _currentSpread, 0f);  
        }
        
        // ─── Private methods ──────────────────────────────────────────                                                                                                                                                       
   
        private void OnWeaponEquipped(Weapon weapon, int slot)                                                                                                                                                                  
        {       
            UnsubscribeFromWeapon();
            _subscribedWeapon          = weapon;
            _subscribedWeapon.OnFired += _ => _currentBloom += _fireBloom;
        }                                                                                                                                                                                                                       
   
        private void OnWeaponUnequipped(int slot) => UnsubscribeFromWeapon();                                                                                                                                                   
                  
        private void UnsubscribeFromWeapon()                                                                                                                                                                                    
        {       
            if (_subscribedWeapon == null) { return; }
            _subscribedWeapon.OnFired -= _ => _currentBloom += _fireBloom;
            _subscribedWeapon          = null;                                                                                                                                                                                  
        }
    }
}

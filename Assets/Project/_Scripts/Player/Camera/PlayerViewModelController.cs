using UnityEngine;
using Weapons;

namespace Player
{
    /// <summary>
    /// Drives the view model Animator from weapon events.
    /// Lives on ViewModelArms GameObject.
    ///
    /// This is the only script that touches ViewModelArms Animator.
    /// All procedural animation (sway, bob, recoil) is on ViewModelRoot.
    /// </summary>
    public sealed class ViewModelController : MonoBehaviour
    {
        // ─── Serialized properties ────────────────────────────────────────

        [Header("References")]
        [SerializeField] private WeaponHolder _weaponHolder;
        [SerializeField] private WorldWeaponController _worldWeaponController;
        [SerializeField] private Animator _animator;

        // ─── Hashes ────────────────────────────────────

        // Arms
        private static readonly int _weaponEquippedHash = Animator.StringToHash("WeaponEquipped");
        private static readonly int _isReloadingHash = Animator.StringToHash("IsReloading");
        private static readonly int _firingStateHash = Animator.StringToHash("Fire");
        private static readonly int _idleStateHash = Animator.StringToHash("Armed Idle");
        
        // Guns
        private static readonly int _weaponFireStateHash = Animator.StringToHash("Fire");
        private static readonly int _weaponReloadBoolHash = Animator.StringToHash("IsReloading");
        private static readonly int _weaponEmptyBoolHash = Animator.StringToHash("IsEmpty");

        // ─── Private state ────────────────────────────────────────────

        private Weapon _subscribedWeapon;
        private Animator _weaponAnimator;

        // ─── Unity lifecycle ──────────────────────────────────────────

        private void Awake()
        {
            Debug.Assert(_weaponHolder != null,"[PlayerViewModelController] WeaponHolder not assigned.", this);
            Debug.Assert(_worldWeaponController != null, "[PlayerViewModelController] WorldWeaponController not assigned.", this);
            Debug.Assert(_animator != null,"[PlayerViewModelController] Animator not assigned.",this);
        }

        private void OnEnable()
        {
            _weaponHolder.OnWeaponEquipped   += OnWeaponEquipped;
            _weaponHolder.OnWeaponUnequipped += OnWeaponUnequipped;
            
            _worldWeaponController.OnWeaponMeshSpawned += OnWeaponMeshSpawned;
            _worldWeaponController.OnWeaponMeshDespawned += OnWeaponMeshDespawned;
        }

        private void OnDisable()
        {
            _weaponHolder.OnWeaponEquipped   -= OnWeaponEquipped;
            _weaponHolder.OnWeaponUnequipped -= OnWeaponUnequipped;
            
            _worldWeaponController.OnWeaponMeshSpawned -= OnWeaponMeshSpawned;
            _worldWeaponController.OnWeaponMeshDespawned -= OnWeaponMeshDespawned;
            
            UnsubscribeFromWeapon();
        }

        // ─── Private methods ──────────────────────────────────────────

        private void OnWeaponEquipped(Weapon weapon, int slot)
        {
            _animator.SetBool(_weaponEquippedHash, true);
            _animator.SetBool(_isReloadingHash, weapon.IsReloading);
            
            if (_weaponAnimator != null)
                _weaponAnimator.SetBool(_weaponEmptyBoolHash, weapon.IsEmpty);
            SubscribeToWeapon(weapon);
        }

        private void OnWeaponUnequipped(int slot)
        {
            _animator.Play(_idleStateHash, 0, 0.0f);
            _animator.SetBool(_weaponEquippedHash, false);
            UnsubscribeFromWeapon();
        }
        
        private void OnWeaponMeshSpawned(Animator weaponAnimator)
        {
            _weaponAnimator = weaponAnimator;
            
            if (_subscribedWeapon != null)
                _weaponAnimator?.SetBool(_weaponEmptyBoolHash, _subscribedWeapon.IsEmpty);
        }
        private void OnWeaponMeshDespawned() { _weaponAnimator = null; }

        private void SubscribeToWeapon(Weapon weapon)
        {
            UnsubscribeFromWeapon();
            _subscribedWeapon = weapon;
            _subscribedWeapon.OnFired           += OnFired;
            _subscribedWeapon.OnReloadStarted   += OnReloadStarted;
            _subscribedWeapon.OnReloadCompleted += OnReloadCompleted;
            _subscribedWeapon.OnAmmoChanged += OnAmmoChanged;
        }

        private void UnsubscribeFromWeapon()
        {
            if (_subscribedWeapon == null) { return; }
            _subscribedWeapon.OnFired -= OnFired;
            _subscribedWeapon.OnReloadStarted -= OnReloadStarted;
            _subscribedWeapon.OnReloadCompleted -= OnReloadCompleted;
            _subscribedWeapon.OnAmmoChanged -= OnAmmoChanged;
            _subscribedWeapon = null;
        }

        private void OnFired(Vector3 hitPoint)
        {
            _animator.Play(_firingStateHash, 0, 0.0f);
            _weaponAnimator?.Play(_weaponFireStateHash, 0, 0.0f);
        }

        private void OnReloadStarted()
        {
            _animator.SetBool(_isReloadingHash, true);
            _weaponAnimator?.SetBool(_weaponReloadBoolHash, true);
        }

        private void OnReloadCompleted()
        {
            _animator.SetBool(_isReloadingHash, false);
            _weaponAnimator?.SetBool(_weaponReloadBoolHash, false);
        }
        
        private void OnAmmoChanged(int current, int reserve)
        {
            _weaponAnimator?.SetBool(_weaponEmptyBoolHash, current == 0);
        }
    }
}
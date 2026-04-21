using UnityEngine;

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
        [SerializeField] private Animator _animator;

        // ─── Hashes ────────────────────────────────────

        private static readonly int _weaponEquippedHash = Animator.StringToHash("WeaponEquipped");
        private static readonly int _isReloadingHash = Animator.StringToHash("IsReloading");
        private static readonly int _isFiringHash = Animator.StringToHash("IsFiring");
        private static readonly int _fireAnimSpeedHash = Animator.StringToHash("FireAnimSpeed");
        private static readonly int _fireLayerIndex = 1;

        // ─── Private state ────────────────────────────────────────────

        private Weapon _subscribedWeapon;

        // ─── Unity lifecycle ──────────────────────────────────────────

        private void Awake()
        {
            Debug.Assert(_weaponHolder != null,"[ViewModelController] WeaponHolder not assigned.", this);
            Debug.Assert(_animator != null,"[ViewModelController] Animator not assigned.",     this);
            
            _animator?.SetFloat(_fireAnimSpeedHash, 1.0f);
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
            UnsubscribeFromWeapon();
        }

        // ─── Private methods ──────────────────────────────────────────

        private void OnWeaponEquipped(Weapon weapon, int slot)
        {
            _animator.SetBool(_weaponEquippedHash, true);
            SubscribeToWeapon(weapon);
            
            // SetFireLayerSpeed(weapon.Data.FireRate);
            // AUDIO: equip sound here
        }

        private void OnWeaponUnequipped(int slot)
        {
            _animator.SetBool(_weaponEquippedHash, false);
            UnsubscribeFromWeapon();
            // AUDIO: holster sound here
        }

        private void SubscribeToWeapon(Weapon weapon)
        {
            UnsubscribeFromWeapon();
            _subscribedWeapon = weapon;
            _subscribedWeapon.OnFired           += OnFired;
            _subscribedWeapon.OnReloadStarted   += OnReloadStarted;
            _subscribedWeapon.OnReloadCompleted += OnReloadCompleted;
        }

        private void UnsubscribeFromWeapon()
        {
            if (_subscribedWeapon == null) { return; }
            _subscribedWeapon.OnFired           -= OnFired;
            _subscribedWeapon.OnReloadStarted   -= OnReloadStarted;
            _subscribedWeapon.OnReloadCompleted -= OnReloadCompleted;
            _subscribedWeapon = null;
        }

        private void OnFired(Vector3 hitPoint)
        {
            _animator.ResetTrigger(_isFiringHash);
            _animator.SetTrigger(_isFiringHash);
            _animator.Update(0.0f);
            // AUDIO: fire sound here
        }

        private void OnReloadStarted()
        {
            _animator.SetBool(_isReloadingHash, true);
            // AUDIO: reload start sound here
        }

        private void OnReloadCompleted()
        {
            _animator.SetBool(_isReloadingHash, false);
            // AUDIO: reload complete sound here
        }
        
        /// <summary>
        /// To avoid the animation from being delayed when the shoot button is spammed.
        /// Keeps animation in sync with shooting.
        /// </summary>
        /// <param name="fireRate"></param>
        private void SetFireLayerSpeed(float fireRate)
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(_fireLayerIndex);
            AnimatorClipInfo[] clips = _animator.GetCurrentAnimatorClipInfo(_fireLayerIndex);
            
            float clipLength = clips.Length > 0 ? clips[0].clip.length : 0.5f;
            float interval = 1.0f / fireRate;
            float animSpeed = clipLength / interval;
            
            _animator.SetLayerWeight(_fireLayerIndex, 1.0f);
            _animator.speed = 1.0f;
            
            _animator.SetFloat(_fireAnimSpeedHash, animSpeed);
        }
    }
}
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Weapons;

namespace Player
{
    /// <summary>
    /// Manages left hand IK on the view model skeleton only.
    ///
    /// Positions IK target at weapon's LeftHandForegrip each frame.
    /// Drops weight during reload so animation drives left arm.
    /// Right hand has no IK — weapon is on right hand bone.
    ///
    /// Because weapon and IK target are both in camera/view model
    /// space, there is no slipping when the camera rotates.
    /// </summary>
    public sealed class ViewModelHandIK : MonoBehaviour
    {
        // ─── Serialized fields ────────────────────────────────────────

        [Header("IK")]
        [SerializeField] private TwoBoneIKConstraint _leftHandIK;
        [SerializeField] private Transform           _leftHandTarget;

        [Header("References")]
        [SerializeField] private WeaponHolder _weaponHolder;

        [Header("Settings")]
        [SerializeField] [Range(1.0f, 20.0f)]
        private float _blendSpeed = 10.0f;

        [Tooltip("Left IK weight during reload — 0 lets animation drive freely.")]
        [SerializeField] [Range(0.0f, 1.0f)]
        private float _reloadWeight = 0.0f;

        // ─── Private state ────────────────────────────────────────────

        private float  _targetWeight;
        private float  _currentWeight;
        private Weapon _subscribedWeapon;

        // ─── Unity lifecycle ──────────────────────────────────────────

        private void Awake()
        {
            Debug.Assert(_leftHandIK     != null,
                "[ViewModelHandIK] Left hand IK not assigned.",    this);
            Debug.Assert(_leftHandTarget != null,
                "[ViewModelHandIK] Left hand target not assigned.", this);
            Debug.Assert(_weaponHolder   != null,
                "[ViewModelHandIK] WeaponHolder not assigned.",     this);
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

        private void LateUpdate()
        {
            UpdateIKTarget();
            BlendWeight();
        }

        // ─── Private methods ──────────────────────────────────────────

        private void UpdateIKTarget()
        {
            Weapon weapon = _weaponHolder?.ActiveWeapon;

            // if (weapon?.LeftHandForegrip == null) { return; }
            //
            // // Both weapon and target are in view model / camera space
            // // so this never slips when looking around
            // _leftHandTarget.SetPositionAndRotation(
            //     weapon.LeftHandForegrip.position,
            //     weapon.LeftHandForegrip.rotation);
        }

        private void BlendWeight()
        {
            _currentWeight = Mathf.Lerp(
                _currentWeight,
                _targetWeight,
                1.0f - Mathf.Pow(
                    0.1f, Time.deltaTime * _blendSpeed));

            _leftHandIK.weight = _currentWeight;
        }

        private void OnWeaponEquipped(Weapon weapon, int slot)
        {
            _targetWeight = 1.0f;
            SubscribeToWeapon(weapon);
        }

        private void OnWeaponUnequipped(int slot)
        {
            _targetWeight = 0.0f;
            UnsubscribeFromWeapon();
        }

        private void SubscribeToWeapon(Weapon weapon)
        {
            UnsubscribeFromWeapon();
            _subscribedWeapon = weapon;
            _subscribedWeapon.OnReloadStarted   += OnReloadStarted;
            _subscribedWeapon.OnReloadCompleted += OnReloadCompleted;
        }

        private void UnsubscribeFromWeapon()
        {
            if (_subscribedWeapon == null) { return; }
            _subscribedWeapon.OnReloadStarted   -= OnReloadStarted;
            _subscribedWeapon.OnReloadCompleted -= OnReloadCompleted;
            _subscribedWeapon = null;
        }

        private void OnReloadStarted()   => _targetWeight = _reloadWeight;
        private void OnReloadCompleted() => _targetWeight = 1.0f;
    }
}
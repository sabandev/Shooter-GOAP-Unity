using UnityEngine;
using Weapons;  

namespace Player
{
    /// <summary>
    /// Applies procedural recoil to ViewModelRoot.
    /// Kicks position and rotation on fire, recovers smoothly.
    /// Optionally applies fraction to camera pitch.
    /// </summary>
    public sealed class ViewModelRecoil : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WeaponHolder _weaponHolder;
        [SerializeField] private FPSCamera    _fpsCamera;

        [Header("Kick")]
        [SerializeField] private float _kickUp    = 2.0f;
        [SerializeField] private float _kickBack  = 0.05f;
        [SerializeField] private float _drift     = 0.3f;

        [Header("Camera")]
        [SerializeField] [Range(0.0f, 1.0f)]
        private float _cameraFraction = 0.4f;

        private Vector3    _recoilPos = Vector3.zero;
        private Quaternion _recoilRot = Quaternion.identity;
        private Weapon     _subscribedWeapon;

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
            if (_subscribedWeapon == null) { return; }

            float speed = _subscribedWeapon.Data.RecoilRecoverySpeed;

            _recoilPos = Vector3.Lerp(
                _recoilPos, Vector3.zero,
                1.0f - Mathf.Pow(0.1f, Time.deltaTime * speed));

            _recoilRot = Quaternion.Slerp(
                _recoilRot, Quaternion.identity,
                1.0f - Mathf.Pow(0.1f, Time.deltaTime * speed));

            // Apply additively so bob and sway are unaffected
            transform.localPosition += _recoilPos;
            transform.localRotation *= _recoilRot;
        }

        private void OnWeaponEquipped(Weapon weapon, int slot)
        {
            SubscribeToWeapon(weapon);
        }

        private void OnWeaponUnequipped(int slot)
        {
            UnsubscribeFromWeapon();
        }

        private void SubscribeToWeapon(Weapon weapon)
        {
            UnsubscribeFromWeapon();
            _subscribedWeapon         = weapon;
            _subscribedWeapon.OnFired += OnFired;
        }

        private void UnsubscribeFromWeapon()
        {
            if (_subscribedWeapon == null) { return; }
            _subscribedWeapon.OnFired -= OnFired;
            _subscribedWeapon         = null;
        }

        private void OnFired(Vector3 hitPoint)
        {
            if (_subscribedWeapon?.Data == null) { return; }

            WeaponData data = _subscribedWeapon.Data;

            float kick  = data.RecoilVertical.Evaluate(0.0f);
            float drift = Random.Range(
                -data.RecoilHorizontal.Evaluate(0.0f),
                 data.RecoilHorizontal.Evaluate(0.0f));

            _recoilPos = new Vector3(0.0f, 0.0f, -_kickBack);
            _recoilRot = Quaternion.Euler(
                -kick  * _kickUp,
                 drift * _drift,
                 0.0f);

            _fpsCamera?.ApplyRecoil(kick * _cameraFraction);
            // AUDIO: additional recoil/fire sound here if needed
        }
    }
}
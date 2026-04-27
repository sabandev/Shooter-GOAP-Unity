using UnityEngine;
using Weapons;

namespace Player
{
    /// <summary>
    /// Applies procedural weapon sway to ViewModelRoot.
    /// </summary>
    public sealed class ViewModelSway : MonoBehaviour
    {
        // ───── Serialized propertise ────────────────────────────────────────────────
        
        [Header("References")]
        [SerializeField] private WeaponHolder _weaponHolder;

        [Space(10.0f)]
        
        [Header("Settings")]
        [SerializeField] private float _swayAmount    = 0.06f;
        [SerializeField] private float _swayMaxAngle  = 6.0f;
        [SerializeField] private float _rollAmount = 0.08f;
        [SerializeField] private float _rollMaxAngle = 10.0f;
        [SerializeField] [Range(1.0f, 20.0f)] private float _returnSpeed = 8.0f;

        // ───── Private properties ────────────────────────────────────────────────
        
        private PlayerInputActions _input;
        
        private Quaternion _currentSway = Quaternion.identity;
        
        private float _aimMultiplier = 1.0f;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void Awake() => _input = new PlayerInputActions();
        private void OnEnable()  => _input.Player.Enable();
        private void OnDisable() => _input.Player.Disable();

        private void Update()
        {
            float amount = _weaponHolder.HasWeapon ? _swayAmount * _aimMultiplier : _swayAmount * 0.3f;

            Vector2 look = _input.Player.Look.ReadValue<Vector2>();

            float swayX = Mathf.Clamp(-look.y * amount, -_swayMaxAngle, _swayMaxAngle);
            float swayY = Mathf.Clamp(look.x * amount, -_swayMaxAngle * _aimMultiplier, _swayMaxAngle * _aimMultiplier);
            float swayZ = Mathf.Clamp(-look.x * _rollAmount, -_rollMaxAngle, _rollMaxAngle);

            Quaternion target = Quaternion.Euler(swayX, swayY, swayZ);

            _currentSway = Quaternion.Slerp(_currentSway, target,1.0f - Mathf.Pow(0.1f, Time.deltaTime * _returnSpeed));

            transform.localRotation = _currentSway;
        }

        // ───── Public properties ────────────────────────────────────────────────
        
        public void SetAimMultiplier(float multiplier) => _aimMultiplier = multiplier;
    }
}
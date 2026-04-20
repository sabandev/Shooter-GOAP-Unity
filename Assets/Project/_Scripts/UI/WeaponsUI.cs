using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// Displays weapon inventory state.
    ///
    /// Hidden until first weapon pickup.
    /// Shows both weapon slots with icon and ammo count.
    /// Fades out when all weapons are dropped.
    ///
    /// Subscribes to WeaponHolder and Weapon events —
    /// no polling, updates only when state changes.
    /// </summary>
    public sealed class WeaponUI : MonoBehaviour
    {
        // ─── Serialized fields ────────────────────────────────────────

        [Header("References")]
        [SerializeField] private WeaponHolder _weaponHolder;
        [SerializeField] private CanvasGroup          _canvasGroup;

        [Header("Slot 0")]
        [SerializeField] private Image           _icon0;
        [SerializeField] private TextMeshProUGUI _ammoText0;
        [SerializeField] private GameObject      _slot0Root;

        [Header("Slot 1")]
        [SerializeField] private Image           _icon1;
        [SerializeField] private TextMeshProUGUI _ammoText1;
        [SerializeField] private GameObject      _slot1Root;

        [Header("Active Slot Highlight")]
        [Tooltip("Colour of the icon when weapon is active.")]
        [SerializeField] private Color _activeColour =
            Color.white;

        [Tooltip("Colour of the icon when weapon is in inactive slot.")]
        [SerializeField] private Color _inactiveColour =
            new Color(1f, 1f, 1f, 0.4f);

        [Tooltip("Colour tint when slot is empty.")]
        [SerializeField] private Color _emptyColour =
            new Color(1f, 1f, 1f, 0.15f);

        [Header("Fade")]
        [SerializeField] [Range(1.0f, 10.0f)]
        private float _fadeSpeed = 4.0f;

        // ─── Private state ────────────────────────────────────────────

        private float    _targetAlpha;
        private Weapon[] _subscribedWeapons = new Weapon[2];

        // ─── Unity lifecycle ──────────────────────────────────────────

        private void Awake()
        {
            Debug.Assert(_weaponHolder != null,
                "[WeaponUI] WeaponHolder not assigned.", this);
            Debug.Assert(_canvasGroup  != null,
                "[WeaponUI] CanvasGroup not assigned.",  this);

            // Hidden until first pickup
            _canvasGroup.alpha = 0.0f;
            _targetAlpha       = 0.0f;
        }

        private void OnEnable()
        {
            _weaponHolder.OnWeaponEquipped   += OnWeaponEquipped;
            _weaponHolder.OnWeaponUnequipped += OnWeaponUnequipped;
            _weaponHolder.OnWeaponPickedUp   += OnWeaponPickedUp;
            _weaponHolder.OnWeaponDropped    += OnWeaponDropped;
        }

        private void OnDisable()
        {
            _weaponHolder.OnWeaponEquipped   -= OnWeaponEquipped;
            _weaponHolder.OnWeaponUnequipped -= OnWeaponUnequipped;
            _weaponHolder.OnWeaponPickedUp   -= OnWeaponPickedUp;
            _weaponHolder.OnWeaponDropped    -= OnWeaponDropped;

            UnsubscribeAllWeapons();
        }

        private void Update()
        {
            // Fade in/out
            if (Mathf.Approximately(_canvasGroup.alpha, _targetAlpha)) { return; }

            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, _targetAlpha, Time.deltaTime * _fadeSpeed);
        }

        // ─── Event handlers ───────────────────────────────────────────

        private void OnWeaponPickedUp(Weapon weapon, int slot)
        {
            // Show UI on first pickup
            _targetAlpha = 1.0f;

            RefreshSlot(slot);
            SubscribeToWeapon(weapon, slot);
        }

        private void OnWeaponEquipped(Weapon weapon, int slot)
        {
            RefreshAllSlots();
        }

        private void OnWeaponUnequipped(int slot)
        {
            RefreshAllSlots();
        }

        private void OnWeaponDropped(Weapon weapon)
        {
            // Find which slot was dropped
            for (int i = 0; i < 2; i++)
            {
                if (_subscribedWeapons[i] == weapon)
                {
                    UnsubscribeFromWeapon(i);
                    break;
                }
            }

            RefreshAllSlots();

            // Fade out if no weapons remain
            if (!_weaponHolder.HasWeapon &&
                _weaponHolder.GetWeapon(0) == null &&
                _weaponHolder.GetWeapon(1) == null)
            {
                _targetAlpha = 0.0f;
            }
        }

        // ─── Private methods ──────────────────────────────────────────

        private void RefreshAllSlots()
        {
            RefreshSlot(0);
            RefreshSlot(1);
        }

        private void RefreshSlot(int slot)
        {
            Weapon weapon  = _weaponHolder.GetWeapon(slot);
            bool           isActive = _weaponHolder.ActiveSlot == slot;

            Image           icon     = slot == 0 ? _icon0     : _icon1;
            TextMeshProUGUI ammoText = slot == 0 ? _ammoText0 : _ammoText1;
            GameObject      root     = slot == 0 ? _slot0Root : _slot1Root;

            if (weapon == null)
            {
                // Empty slot — dim icon, clear ammo
                icon.sprite = null;
                icon.color  = _emptyColour;
                ammoText.text = string.Empty;
                return;
            }

            // Set icon from WeaponData
            if (weapon.Data.HUDIcon != null)
            {
                icon.sprite  = weapon.Data.HUDIcon;
                icon.enabled = true;
            }

            // Active vs inactive tint
            icon.color = isActive ? _activeColour : _inactiveColour;

            // Ammo display
            UpdateAmmoText(ammoText, weapon);
        }

        private void UpdateAmmoText(TextMeshProUGUI text, Weapon  weapon)
        {
            if (weapon == null)
            {
                text.text = string.Empty;
                return;
            }

            // Format: "12 / 60"
            // Current magazine bold and larger, reserve normal
            text.text = $"<b>{weapon.CurrentAmmo}</b> / {weapon.ReserveAmmo}";
        }

        private void SubscribeToWeapon(Weapon weapon, int slot)
        {
            UnsubscribeFromWeapon(slot);

            _subscribedWeapons[slot]            = weapon;
            _subscribedWeapons[slot].OnAmmoChanged += OnAmmoChanged;
        }

        private void UnsubscribeFromWeapon(int slot)
        {
            if (_subscribedWeapons[slot] == null) { return; }

            _subscribedWeapons[slot].OnAmmoChanged -= OnAmmoChanged;
            _subscribedWeapons[slot]                = null;
        }

        private void UnsubscribeAllWeapons()
        {
            for (int i = 0; i < _subscribedWeapons.Length; i++)
                UnsubscribeFromWeapon(i);
        }

        private void OnAmmoChanged(int current, int reserve)
        {
            // Find which slot this weapon is in and refresh it
            for (int i = 0; i < 2; i++)
            {
                if (_subscribedWeapons[i] == null) { continue; }

                TextMeshProUGUI text = i == 0 ? _ammoText0 : _ammoText1;
                UpdateAmmoText(text, _subscribedWeapons[i]);
            }
        }
    }
}
using UnityEngine;
using Audio;

/// <summary>
/// Manages weapon inventory.
/// Shared between player and AI.
///
/// Authorised Use Instructions:
///     - Can own ONLY gameplay state — which weapons are held,
///       which is active, equip/drop/swap logic.
///     - CANNOT own visuals. ViewModelController and
///       WorldWeaponController subscribe to events here
///       and handle all presentation independently.
/// </summary>
public sealed class WeaponHolder : MonoBehaviour
{
    // ─── Serialized properties ───────────────────────────────────────────────────
    [Header("Audio")]
    [SerializeField] private SoundData _equipSound;
    [SerializeField] private SoundData _dropSound;
    [SerializeField] private SoundData _pickupSound;

    // AUDIO: OnWeaponEquipped → equip sound
    // AUDIO: OnWeaponDropped  → drop sound

    // ─── Private properties ────────────────────────────────────────────

    private readonly Weapon[] _slots = new Weapon[2];
    private int _activeSlot = -1;

    // ─── Public properties ───────────────────────────────────────────────

    public event System.Action<Weapon, int> OnWeaponEquipped;
    public event System.Action<int>         OnWeaponUnequipped;
    public event System.Action<Weapon, int> OnWeaponPickedUp;
    public event System.Action<Weapon>      OnWeaponDropped;
    
    public Weapon ActiveWeapon => _activeSlot >= 0 ? _slots[_activeSlot] : null;
    public Weapon GetWeapon(int slot) => slot >= 0 && slot < _slots.Length ? _slots[slot] : null;

    public int    ActiveSlot => _activeSlot;
    public bool   HasWeapon  => ActiveWeapon != null;


    // ─── Public API ───────────────────────────────────────────────

    public bool TryPickupWeapon(Weapon weapon)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] != null) { continue; }

            _slots[i] = weapon;
            OnWeaponPickedUp?.Invoke(weapon, i);
            AudioManager.Instance.Play(_pickupSound, transform.position);

            if (_activeSlot < 0)
                EquipSlot(i);

            return true;
        }

        // Both full — swap with active
        SwapForWeapon(weapon);
        return true;
    }

    public void EquipSlot(int slot)
    {
        if (slot < 0 || slot >= _slots.Length) { return; }
        if (_slots[slot] == null)               { return; }
        if (slot == _activeSlot)                { return; }

        if (_activeSlot >= 0)
            UnequipCurrent();

        _activeSlot = slot;

        // _slots[slot].gameObject.SetActive(false);
        _slots[slot].OnEquipped();

        OnWeaponEquipped?.Invoke(_slots[slot], slot);
        AudioManager.Instance.Play(_equipSound, transform.position);
    }

    public void CycleWeapon()
    {
        for (int i = 1; i <= _slots.Length; i++)
        {
            int check = (_activeSlot + i) % _slots.Length;

            if (_slots[check] != null && check != _activeSlot)
            {
                EquipSlot(check);
                return;
            }
        }
    }

    public void DropActiveWeapon()
    {
        if (_activeSlot < 0)             { return; }
        if (_slots[_activeSlot] == null)  { return; }

        DropSlot(_activeSlot);
    }

    public bool TryFire()   => ActiveWeapon?.TryFire()   ?? false;
    public bool TryReload() => ActiveWeapon?.TryReload() ?? false;

    // ─── Private methods ──────────────────────────────────────────

    private void UnequipCurrent()
    {
        if (_activeSlot < 0) { return; }

        OnWeaponUnequipped?.Invoke(_activeSlot);
        _activeSlot = -1;
    }

    private void DropSlot(int slot)
    {
        Weapon weapon = _slots[slot];
        if (weapon == null) { return; }

        weapon.transform.SetParent(null);
        weapon.transform.position =
            transform.position +
            transform.forward * 1.0f +
            Vector3.up * 0.5f;

        weapon.gameObject.SetActive(true);
        weapon.OnDropped();

        _slots[slot] = null;

        if (_activeSlot == slot)
        {
            OnWeaponUnequipped?.Invoke(slot);
            _activeSlot = -1;

            int other = slot == 0 ? 1 : 0;
            if (_slots[other] != null)
                EquipSlot(other);
        }

        OnWeaponDropped?.Invoke(weapon);
        AudioManager.Instance.Play(_dropSound, transform.position);
    }

    private void SwapForWeapon(Weapon newWeapon)
    {
        if (_activeSlot < 0) { return; }

        DropSlot(_activeSlot);
        _slots[_activeSlot] = newWeapon;
        EquipSlot(_activeSlot);
    }
}
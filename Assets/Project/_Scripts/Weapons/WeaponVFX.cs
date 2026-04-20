using UnityEngine;

/// <summary>
/// Handles all visual effects for a weapon.
///
/// Authorised Use Instructions:
///     - The muzzle and shell eject points MUST be on the VIEW MODEL.
/// </summary>
public sealed class WeaponVFX : MonoBehaviour
{
    // ───── Serialized properties ────────────────────────────────────────────────
    [Header("References")]
    [SerializeField] private WeaponHolder _weaponHolder;
    
    [Space(10.0f)]
    
    [Header("Attachment Points")]
    [SerializeField] private Transform _muzzlePoint;
    [SerializeField] private Transform _shellEjectPoint;

    // ───── Private properties ────────────────────────────────────────────────
    
    private Weapon _weapon;

    // ───── Lifecycle methods ────────────────────────────────────────────────
    
    private void Awake()
    {
        _weaponHolder = GetComponentInParent<WeaponHolder>();
        
        Debug.Assert(_weaponHolder    != null,"[WeaponVFX] WeaponHolder not assigned.",     this);
        Debug.Assert(_muzzlePoint     != null, "[WeaponVFX] MuzzlePoint not assigned.",      this);
        Debug.Assert(_shellEjectPoint != null,"[WeaponVFX] ShellEjectPoint not assigned.",  this);
    }

    private void OnEnable()
    {
        _weaponHolder.OnWeaponEquipped += OnWeaponEquipped;
        _weaponHolder.OnWeaponUnequipped += OnWeaponUnequipped;
        
        if (_weaponHolder.HasWeapon)
        {
            OnWeaponEquipped(_weaponHolder.ActiveWeapon,
                _weaponHolder.ActiveSlot);
        }
    }
    private void OnDisable()
    {
        _weaponHolder.OnWeaponEquipped -= OnWeaponEquipped;
        _weaponHolder.OnWeaponUnequipped -= OnWeaponUnequipped;
        UnsubscribeFromWeapon();
    }

    // ───── Private methods ────────────────────────────────────────────────

    private void OnWeaponEquipped(Weapon weapon, int slot)
    {
        SubscribeToWeapon(weapon);
        weapon.SetMuzzlePoint(_muzzlePoint);
    }
    
    private void OnWeaponUnequipped(int slot) => UnsubscribeFromWeapon();
    
    private void SubscribeToWeapon(Weapon weapon)
    {
        UnsubscribeFromWeapon();
        _weapon = weapon;
        _weapon.OnFired += OnFired;
    }
    
    private void UnsubscribeFromWeapon()
    {
        if (_weapon == null) { return; }
        
        _weapon.OnFired -= OnFired;
        _weapon = null;
    }
    
    private void OnFired(Vector3 hitPoint)
    {
        SpawnMuzzleFlash();
        SpawnShellCasing();
        SpawnBulletTrail(hitPoint);
    }

    private void SpawnMuzzleFlash()
    {
        if (_weapon?.Data?.MuzzleFlashPrefab == null || _muzzlePoint == null) { return; }

        ObjectPool.Get(_weapon.Data.MuzzleFlashPrefab, _muzzlePoint.position, _muzzlePoint.rotation);
    }

    private void SpawnShellCasing()
    {
        if (_weapon?.Data?.ShellCasingPrefab == null || _shellEjectPoint == null) { return; }

        PooledObject casing = ObjectPool.Get(_weapon.Data.ShellCasingPrefab, _shellEjectPoint.position, _shellEjectPoint.rotation);
        
        if (casing == null) { return; }

        if (casing.TryGetComponent(out ShellCasing sc))
            sc.Eject(_shellEjectPoint.right);
    }
    
    private void SpawnBulletTrail(Vector3 hitPoint)
    {
        if (_weapon?.Data?.BulletTrailPrefab == null) { return; }
        
        PooledObject trail = ObjectPool.Get(_weapon.Data.BulletTrailPrefab, _muzzlePoint.position, Quaternion.identity);
        
        if (trail == null) { return; }
        
        if (trail.TryGetComponent(out BulletTrail bt))
            bt.Initialise(_muzzlePoint.position, hitPoint);
    }
}
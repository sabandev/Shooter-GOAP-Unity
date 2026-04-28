using System;
using UnityEngine;
using Weapons;

/// <summary>
/// Manages weapon presence on the world body skeleton.
/// Spawns a visual-only world weapon mesh on the right hand bone.
/// </summary>
public sealed class WorldWeaponController : MonoBehaviour
{
    // ─── Serialized fields ────────────────────────────────────────

    [Header("References")]
    [SerializeField] private WeaponHolder _weaponHolder;
    [SerializeField] private Transform    _weaponAttachPoint;

    [Header("World Weapon Offset")]
    [SerializeField] private Vector3 _positionOffset = Vector3.zero;
    [SerializeField] private Vector3 _rotationOffset = Vector3.zero;

    // ───── Public properties ────────────────────────────────────────────────
    
    public event Action<Animator> OnWeaponMeshSpawned;
    public event Action OnWeaponMeshDespawned;

    // ─── Private properties ────────────────────────────────────────────

    private GameObject _worldWeaponInstance;

    // ─── Lifecycle methods ──────────────────────────────────────────

    private void Awake()
    {
        Debug.Assert(_weaponHolder  != null,"[WorldWeaponController] WeaponHolder not assigned.",    this);
        Debug.Assert(_weaponAttachPoint != null,"[WorldWeaponController] Right hand bone not assigned.", this);
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
    }

    // ─── Private methods ──────────────────────────────────────────

    private void OnWeaponEquipped(Weapon weapon, int slot) { SpawnWorldWeapon(weapon); }
    private void OnWeaponUnequipped(int slot) { DespawnWorldWeapon(); }

    private void SpawnWorldWeapon(Weapon weapon)
    {
        DespawnWorldWeapon();

        if (weapon.Data.WorldWeaponMeshPrefab == null) { return; }

        _worldWeaponInstance = Instantiate(weapon.Data.WorldWeaponMeshPrefab, _weaponAttachPoint);
        _worldWeaponInstance.transform.SetLocalPositionAndRotation(_positionOffset,Quaternion.Euler(_rotationOffset));

        SetLayerRecursive(_worldWeaponInstance, LayerMask.NameToLayer("ViewModel"));
        
        Animator weaponAnimator = _worldWeaponInstance.GetComponent<Animator>();
        OnWeaponMeshSpawned?.Invoke(weaponAnimator);
    }

    private void DespawnWorldWeapon()
    {
        if (_worldWeaponInstance == null) { return; }
        Destroy(_worldWeaponInstance);
        _worldWeaponInstance = null;
        OnWeaponMeshDespawned?.Invoke();
    }

    private static void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }
}
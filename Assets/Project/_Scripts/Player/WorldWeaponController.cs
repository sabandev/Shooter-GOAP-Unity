using UnityEngine;

/// <summary>
/// Manages weapon presence on the world body skeleton.
///
/// Spawns a visual-only world weapon mesh on the right hand bone.
/// Drives world body Animator upper body layer.
///
/// Completely independent from ViewModelController.
/// Both subscribe to the same WeaponHolder events.
/// </summary>
public sealed class WorldWeaponController : MonoBehaviour
{
    // ─── Serialized fields ────────────────────────────────────────

    [Header("References")]
    [SerializeField] private WeaponHolder _weaponHolder;
    // [SerializeField] private Animator     _worldAnimator;
    [SerializeField] private Transform    _rightHandBone;

    [Header("World Weapon Offset")]
    [SerializeField] private Vector3 _positionOffset = Vector3.zero;
    [SerializeField] private Vector3 _rotationOffset = Vector3.zero;

    // ─── Hashes ────────────────────────────────────

    private static readonly int _weaponEquippedHash = Animator.StringToHash("WeaponEquipped");
    private static readonly int _isReloadingHash = Animator.StringToHash("IsReloading");
    private static readonly int _isFiringHash = Animator.StringToHash("IsFiring");

    // ─── Private state ────────────────────────────────────────────

    private GameObject _worldWeaponInstance;
    private Weapon     _subscribedWeapon;

    // ─── Unity lifecycle ──────────────────────────────────────────

    private void Awake()
    {
        Debug.Assert(_weaponHolder  != null,"[WorldWeaponController] WeaponHolder not assigned.",    this);
        // Debug.Assert(_worldAnimator != null,"[WorldWeaponController] World Animator not assigned.",  this);
        Debug.Assert(_rightHandBone != null,"[WorldWeaponController] Right hand bone not assigned.", this);
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
        // _worldAnimator.SetBool(_weaponEquippedHash, true);
        SpawnWorldWeapon(weapon);
        SubscribeToWeapon(weapon);
    }

    private void OnWeaponUnequipped(int slot)
    {
        // _worldAnimator.SetBool(_weaponEquippedHash, false);
        DespawnWorldWeapon();
        UnsubscribeFromWeapon();
    }

    private void SpawnWorldWeapon(Weapon weapon)
    {
        DespawnWorldWeapon();

        if (weapon.Data.WorldWeaponMeshPrefab == null) { return; }

        _worldWeaponInstance = Instantiate(
            weapon.Data.WorldWeaponMeshPrefab,
            _rightHandBone);

        _worldWeaponInstance.transform.SetLocalPositionAndRotation(
            _positionOffset,
            Quaternion.Euler(_rotationOffset));

        SetLayerRecursive(
            _worldWeaponInstance,
            LayerMask.NameToLayer("ViewModel"));
    }

    private void DespawnWorldWeapon()
    {
        if (_worldWeaponInstance == null) { return; }
        Destroy(_worldWeaponInstance);
        _worldWeaponInstance = null;
    }

    private void SubscribeToWeapon(Weapon weapon)
    {
        UnsubscribeFromWeapon();
        _subscribedWeapon = weapon;
        _subscribedWeapon.OnFired           += OnFired;
        // _subscribedWeapon.OnReloadStarted   += OnReloadStarted;
        // _subscribedWeapon.OnReloadCompleted += OnReloadCompleted;
    }

    private void UnsubscribeFromWeapon()
    {
        if (_subscribedWeapon == null) { return; }
        _subscribedWeapon.OnFired           -= OnFired;
        // _subscribedWeapon.OnReloadStarted   -= OnReloadStarted;
        // _subscribedWeapon.OnReloadCompleted -= OnReloadCompleted;
        _subscribedWeapon = null;
    }

    private void OnFired(Vector3 hitPoint)
    {
        // _worldAnimator.ResetTrigger(_isFiringHash);
        // _worldAnimator.SetTrigger(_isFiringHash);
    }

    // private void OnReloadStarted()  => _worldAnimator.SetBool(_isReloadingHash, true);
    // private void OnReloadCompleted() => _worldAnimator.SetBool(_isReloadingHash, false);

    private static void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }
}
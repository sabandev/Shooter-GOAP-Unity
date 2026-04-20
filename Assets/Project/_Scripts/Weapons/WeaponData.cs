using UnityEngine;

/// <summary>
/// Immutable weapon configuration asset.
/// </summary>
[CreateAssetMenu(fileName = "WeaponData_New", menuName = "Weapons/Weapon Data")]
public sealed class WeaponData : ScriptableObject
{
    // ───── Nested type ────────────────────────────────────────────────
    
    public enum FireMode
    {
        SemiAuto,
        FullAuto,
    }
    
    // ─── Identity ─────────────────────────────────────────────────

    [Header("Identity")]
    public string WeaponName  = "Weapon";
    public Sprite HUDIcon;

    // ─── Combat ───────────────────────────────────────────────────

    [Header("Combat")]
    public float Damage       = 25.0f;
    public float FireRate     = 8.0f;      // rounds per second
    public float Range        = 50.0f;
    public float Spread       = 1.5f;      // degrees
    public FireMode Mode       = FireMode.SemiAuto;

    [Header("Ammo")]
    public int   MagazineSize   = 12;
    public int   MaxReserveAmmo = 60;

    [Header("Reload")]
    public float ReloadDuration = 2.0f;
    [Range(0.0f, 1.0f)] public float ReloadRefillTime = 0.6f;

    // ─── Recoil ───────────────────────────────────────────────────

    [Header("Recoil")]
    public AnimationCurve RecoilVertical = AnimationCurve.EaseInOut(0.0f, 0.0f, 0.15f, 3.0f);
    public AnimationCurve RecoilHorizontal = AnimationCurve.Linear(0.0f, 0.0f, 0.15f, 0.5f);

    public float RecoilRecoverySpeed = 8.0f;

    [Range(0.0f, 1.0f)] public float CameraRecoilFraction = 0.4f;

    // ─── Procedural animation ─────────────────────────────────────

    [Header("Sway")]
    public float SwayAmount    = 0.06f;
    public float SwayMaxAngle  = 6.0f;
    public float SwaySpeed     = 8.0f;

    [Header("Bob — Standing")]
    public float BobFrequency  = 1.8f;
    public float BobVertical   = 0.008f;
    public float BobHorizontal = 0.004f;

    [Header("Bob — Crouching")]
    public float CrouchBobFrequency  = 1.2f;
    public float CrouchBobVertical   = 0.005f;
    public float CrouchBobHorizontal = 0.003f;

    // ─── Prefabs ──────────────────────────────────────────────────

    [Header("Prefabs")]
    public GameObject WorldPrefab;
    public GameObject WorldWeaponMeshPrefab;

    // ─── VFX ──────────────────────────────────────────────────────

    [Header("VFX")]
    public GameObject MuzzleFlashPrefab;
    public GameObject BulletTrailPrefab;
    public GameObject ShellCasingPrefab;

    [Header("Impact VFX")]
    public GameObject ImpactDefaultPrefab;
    public GameObject ImpactMetalPrefab;
    public GameObject ImpactWoodPrefab;
    public GameObject ImpactConcretePrefab;
    public GameObject ImpactFleshPrefab;
    public GameObject ImpactGlassPrefab;

    // ─── Audio (wired in sound step) ─────────────────────────────
    // AUDIO: Uncomment when sound system implemented
    // [Header("Audio")]
    // public AudioClip FireSound;
    // public AudioClip ReloadSound;
    // public AudioClip EmptyClickSound;
    // public AudioClip EquipSound;
    // public AudioClip PickupSound;

    // ─── Helpers ──────────────────────────────────────────────────

    public GameObject GetImpactPrefab(SurfaceType.Surface surface)
    {
        return surface switch
        {
            SurfaceType.Surface.Metal    => ImpactMetalPrefab,
            SurfaceType.Surface.Wood     => ImpactWoodPrefab,
            SurfaceType.Surface.Concrete => ImpactConcretePrefab,
            SurfaceType.Surface.Flesh    => ImpactFleshPrefab,
            SurfaceType.Surface.Glass    => ImpactGlassPrefab,
            _                            => ImpactDefaultPrefab
        };
    }
}
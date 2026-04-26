using UnityEngine;
using Audio;

namespace Weapons
{
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
        public string WeaponName = "Weapon";
        public Sprite HUDIcon;
        
        [Space(10.0f)]

        // ─── Combat ───────────────────────────────────────────────────

        [Header("Combat")]
        [Min(0.0f)] public float Damage = 25.0f;
        [Min(0.0f)] public float ImpactForce = 8.0f;
        [Min(0.01f)] public float FireRate = 8.0f; // rounds per second
        [Min(0.1f)] public float Range = 50.0f;
        [Min(0.0f)] public float Spread = 1.5f; // degrees
        public FireMode Mode = FireMode.SemiAuto;
        
        [Space(10.0f)]

        [Header("Ammo")]
        [Min(1)] public int MagazineSize = 12;
        [Min(0)] public int MaxReserveAmmo = 60;
        
        [Space(10.0f)]

        [Header("Reload")]
        [Min(0.01f)] public float ReloadDuration = 2.0f;
        [Range(0.0f, 1.0f)] public float ReloadRefillTime = 0.6f;
        
        [Space(10.0f)]

        // ─── Recoil ───────────────────────────────────────────────────

        [Header("Recoil")]
        public AnimationCurve RecoilVertical = AnimationCurve.EaseInOut(0.0f, 0.0f, 0.15f, 3.0f);
        public AnimationCurve RecoilHorizontal = AnimationCurve.Linear(0.0f, 0.0f, 0.15f, 0.5f);

        [Min(0.01f)] public float RecoilRecoverySpeed = 8.0f;
        [Range(0.0f, 1.0f)] public float CameraRecoilFraction = 0.4f;
        
        [Space(10.0f)]

        // ─── Procedural animation ─────────────────────────────────────

        [Header("Sway")]
        [Min(0.0f)] public float SwayAmount = 0.06f;
        [Min(0.0f)] public float SwayMaxAngle = 6.0f;
        [Min(0.0f)] public float SwaySpeed = 8.0f;
        
        [Space(10.0f)]

        [Header("Bob — Standing")]
        [Min(0.0f)] public float BobFrequency = 1.8f;
        [Min(0.0f)] public float BobVertical = 0.008f;
        [Min(0.0f)] public float BobHorizontal = 0.004f;
        
        [Space(10.0f)]

        [Header("Bob — Crouching")]
        [Min(0.0f)] public float CrouchBobFrequency = 1.2f;
        [Min(0.0f)] public float CrouchBobVertical = 0.005f;
        [Min(0.0f)] public float CrouchBobHorizontal = 0.003f;
        
        [Space(10.0f)]

        // ─── Prefabs ──────────────────────────────────────────────────

        [Header("Prefabs")]
        public GameObject WorldPrefab;
        public GameObject WorldWeaponMeshPrefab;
        
        [Space(10.0f)]

        // ─── VFX ──────────────────────────────────────────────────────

        [Header("VFX")]
        public GameObject MuzzleFlashPrefab;
        public GameObject BulletTrailPrefab;
        public GameObject ShellCasingPrefab;
        
        [Space(10.0f)]

        [Header("Impact VFX")]
        public GameObject ImpactDefaultPrefab;
        public GameObject ImpactMetalPrefab;
        public GameObject ImpactWoodPrefab;
        public GameObject ImpactConcretePrefab;
        public GameObject ImpactFleshPrefab;
        public GameObject ImpactGlassPrefab;
        
        [Space(10.0f)]
        
        [Header("Debris Prefabs")]
        public GameObject DebrisDefaultPrefab;
        public GameObject DebrisMetalPrefab;
        public GameObject DebrisWoodPrefab;
        public GameObject DebrisConcretePrefab;
        public GameObject DebrisFleshPrefab;
        public GameObject DebrisGlassPrefab;

        [Space(10.0f)]
        
        // ─── Audio ─────────────────────────────
        [Header("Impact Audio")]
        public SoundData ImpactDefaultSound;
        public SoundData ImpactMetalSound;
        public SoundData ImpactWoodSound;
        public SoundData ImpactConcreteSound;
        public SoundData ImpactFleshSound;
        public SoundData ImpactGlassSound;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void OnValidate()
        {
            Damage = Mathf.Max(0.0f, Damage);
            FireRate = Mathf.Max(0.01f, FireRate);                                                                                                                                            
            Range = Mathf.Max(0.1f, Range);                                                                                                                                                                            
            Spread = Mathf.Max(0.0f, Spread);                                                                                                                                                                           
            MagazineSize = Mathf.Max(1, MagazineSize);                                                                                                                                                                     
            MaxReserveAmmo = Mathf.Max(MagazineSize, MaxReserveAmmo);                                                                                                                                                              
            ReloadDuration = Mathf.Max(0.01f, ReloadDuration);

            Debug.Assert(!string.IsNullOrWhiteSpace(WeaponName), "[WeaponData] WeaponName is empty. Must assign a WeaponName.", this);
        }
        
        // ─── Public methods ──────────────────────────────────────────────────

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
        
        public SoundData GetImpactSound(SurfaceType.Surface surface)
        {
            return surface switch
            {
                SurfaceType.Surface.Metal => ImpactMetalSound,
                SurfaceType.Surface.Wood => ImpactWoodSound,
                SurfaceType.Surface.Concrete => ImpactConcreteSound,
                SurfaceType.Surface.Flesh => ImpactFleshSound,
                SurfaceType.Surface.Glass => ImpactGlassSound,
                _ => ImpactDefaultSound
            };
        }
        
        public GameObject GetDebrisPrefab(SurfaceType.Surface surface)
        {
            GameObject prefab = surface switch
            {                                                                                                                                                                                                                       
                SurfaceType.Surface.Concrete => DebrisConcretePrefab,
                SurfaceType.Surface.Wood => DebrisWoodPrefab,                                                                                                                                                                   
                SurfaceType.Surface.Metal => DebrisMetalPrefab,
                SurfaceType.Surface.Flesh => DebrisFleshPrefab,                                                                                                                                                                  
                SurfaceType.Surface.Glass => DebrisGlassPrefab,
                _ => DebrisDefaultPrefab                                                                                                                                                                 
            };          
                                                                                                                                                                                                                              
            return prefab != null ? prefab : DebrisDefaultPrefab;

        }
    }
}
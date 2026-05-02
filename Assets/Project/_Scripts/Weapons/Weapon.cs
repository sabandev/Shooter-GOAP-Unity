using System;
using UnityEngine;
using GOAP;
using Audio;

namespace Weapons
{
    /// <summary>
    /// Core weapon gameplay component.
    /// Lives on the world pickup prefab.
    ///
    /// Authorised Use Instructions:
    ///     -  CANNOT own: visuals, animation, IK, procedural FX.
    ///        They belong to ViewModelController, WeaponVFX, and WorldWeaponController.
    /// </summary>
    [RequireComponent(typeof(SmartObject))]
    public sealed class Weapon : MonoBehaviour, IInteractable
    {
        // ─── Serialized properties ────────────────────────────────────────

        [Header("Data")]
        [SerializeField] private WeaponData _data;
        
        [Space(10.0f)]
        
        [Header("Audio")]
        [SerializeField] private SoundData _fireSound;
        [SerializeField] private SoundData _emptyClickSound;
        [SerializeField] private SoundData _reloadStartSound;
        [SerializeField] private SoundData _reloadMidSound;
        [SerializeField] private SoundData _reloadEndSound;

        // ─── Public properties ───────────────────────────────────────────────────

        public event Action<Vector3> OnFired;
        public event Action OnReloadStarted;
        public event Action OnReloadCompleted;
        public event Action<int, int> OnAmmoChanged;
        public event Action<RaycastHit, SurfaceType.Surface> OnHit;
        
        public WeaponData Data => _data;
        
        public Rigidbody RigidBody;
        
        public int CurrentAmmo { get; private set; }
        public int ReserveAmmo { get; private set; }
        
        public bool IsReloading { get; private set; }
        public bool IsEmpty => CurrentAmmo <= 0;
        public bool CanFire => !IsReloading && CurrentAmmo > 0 && _fireTimer <= 0.0f;

        // ─── Implementation ────────────────────────────────────────────

        public string InteractionPrompt => $"Pick Up {_data?.WeaponName ?? "Weapon"}";
        public bool CanInteract => !_isHeld;

        public void Interact(GameObject interactor)
        {
            WeaponHolder holder = interactor.GetComponent<WeaponHolder>();

            if (holder == null) { return; }

            holder.TryPickupWeapon(this);
        }

        // ─── Private properties ────────────────────────────────────────────

        private SmartObject _smartObject;
        
        private LayerMask _hitMask;
        
        private float _fireTimer;
        private float _reloadTimer;
        
        private bool _reloadRefilled;
        private bool _isHeld;
        private bool _isAiming;

        // ─── Lifecycle methods ──────────────────────────────────────────

        private void Awake()
        {
            _smartObject = GetComponent<SmartObject>();
            RigidBody = GetComponent<Rigidbody>();

            Debug.Assert(_data != null, "[Weapon] WeaponData not assigned.", this);
            Debug.Assert(_smartObject != null, "[Weapon] SmartObject not assigned.", this);
            Debug.Assert(RigidBody != null, "[Weapon] RigidBody not assigned.", this);
            
            CurrentAmmo = _data.MagazineSize;
            ReserveAmmo = _data.MaxReserveAmmo;
        }
        
        private void Start()
        {
            _hitMask = ~LayerMask.GetMask("ViewModel", "WorldModel");
        }

        private void Update()
        {
            if (_fireTimer > 0.0f)
                _fireTimer -= Time.deltaTime;

            if (IsReloading)
                TickReload();
        }

        // ─── Public methods ───────────────────────────────────────────────

        public bool TryFire()
        {
            if (Time.timeScale == 0.0f) { return false; }
            
            if (IsEmpty)
                AudioManager.Instance.Play(_emptyClickSound, transform.position);
            
            if (!CanFire) { return false; }

            Fire();
            return true;
        }

        public bool TryReload()
        {
            if (IsReloading) { return false; }
            if (CurrentAmmo >= _data.MagazineSize) { return false; }
            if (ReserveAmmo <= 0) { return false; }

            StartReload();
            return true;
        }

        public void OnEquipped(int ownerLayer)
        {
            _isHeld = true;
            _smartObject.enabled = false;
            RigidBody.isKinematic = true;
            SetRanderersEnabled(false);
            
            _hitMask = ~LayerMask.GetMask("ViewModel", "WorldModel") & ~(1 << ownerLayer); // Prevents player/AI from shooting themselves
        }

        public void OnDropped()
        {
            _isHeld = false;
            IsReloading = false;
            _isAiming = false;
            _reloadTimer = 0.0f;
            _smartObject.enabled = true;
            RigidBody.isKinematic = false;
            SetRanderersEnabled(true);
            
            _hitMask = ~LayerMask.GetMask("ViewModel", "WorldModel");
        }
        
        public void SetAiming(bool value) => _isAiming = value;

        // ─── Private methods ──────────────────────────────────────────

        private void Fire()
        {
            CurrentAmmo--;
            _fireTimer = 1.0f / _data.FireRate;

            float spread = _data.Spread * (_isAiming ? _data.AimSpreadMultiplier : 1.0f);
            Vector3 spreadVector = new Vector3(UnityEngine.Random.Range(-spread, spread), UnityEngine.Random.Range(-spread, spread), 0.0f) * Mathf.Deg2Rad;

            // Fire from camera
            Camera mainCam = Camera.main;
            if (mainCam == null) { return; }

            Vector3 origin = mainCam.transform.position;
            Vector3 direction = mainCam.transform.forward + spreadVector;
            Vector3 hitPoint;
            
            if (Physics.Raycast(origin, direction, out RaycastHit hit, _data.Range, _hitMask))
            {
                ProcessHit(hit, direction);
                hitPoint = hit.point;
            }
            else
            {
                hitPoint = origin + direction * _data.Range;
            }
            
            OnFired?.Invoke(hitPoint);
            OnAmmoChanged?.Invoke(CurrentAmmo, ReserveAmmo);
            PlaySound(_fireSound, transform.position);

            if (CurrentAmmo <= 0 && ReserveAmmo > 0)
                StartReload();
        }

        private void ProcessHit(RaycastHit hit, Vector3 direction)
        {
            float multiplier = 1.0f;
            IHealth targetHealth = null;
            IDamageContext damageContext = null;
            
            if (hit.collider.TryGetComponent(out Hitbox hitbox))
            {
                multiplier = hitbox.DamageMultiplier;
                targetHealth = hitbox.RootHealth;
                damageContext = hitbox.RootDamageContext;
            }
            else
            {
                hit.collider.TryGetComponent(out targetHealth);
                damageContext = targetHealth as IDamageContext;
            }
            
            if (targetHealth != null)
            {
                damageContext?.SetHitContext(hit.point, direction, _data.ImpactForce);
                targetHealth.TakeDamage(_data.Damage * multiplier);
            }
            
            if (hit.collider.TryGetComponent(out IImpactReceiver impactReceiver))
                impactReceiver.ReceiveImpact(hit.point, direction, _data.ImpactForce);
            
            SurfaceType surfaceComp = hit.collider.GetComponentInParent<SurfaceType>();
            SurfaceType.Surface type = surfaceComp != null ? surfaceComp.Type : SurfaceType.Surface.Default;
            
            OnHit?.Invoke(hit, type);
            
            SoundData impactSound = _data.GetImpactSound(type);
            PlaySound(impactSound, hit.point);
        }

        private void StartReload()
        {
            IsReloading     = true;
            _reloadTimer    = 0.0f;
            _reloadRefilled = false;

            OnReloadStarted?.Invoke();
            PlaySound(_reloadStartSound, transform.position);
        }

        private void TickReload()
        {
            _reloadTimer += Time.deltaTime;

            float t = _reloadTimer / _data.ReloadDuration;

            if (!_reloadRefilled && t >= _data.ReloadRefillTime)
            {
                int needed  = _data.MagazineSize - CurrentAmmo;
                int toAdd   = Mathf.Min(needed, ReserveAmmo);
                CurrentAmmo += toAdd;
                ReserveAmmo -= toAdd;
                _reloadRefilled = true;
                OnAmmoChanged?.Invoke(CurrentAmmo, ReserveAmmo);
                PlaySound(_reloadMidSound, transform.position);
            }

            if (_reloadTimer >= _data.ReloadDuration)
            {
                IsReloading = false;
                OnReloadCompleted?.Invoke();
                PlaySound(_reloadEndSound, transform.position);
            }
        }
        
        private void SetRanderersEnabled(bool status)
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = status;
            }
        }
        
        private void PlaySound(SoundData data, Vector3 position)
        {
            if (data == null) { return; }
            
            AudioManager.Instance?.Play(data, position);
        }
    }
}
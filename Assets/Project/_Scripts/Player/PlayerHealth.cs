using UnityEngine;
using Audio;

namespace Player
{
    /// <summary>
    /// Manages player health state.
    /// </summary>
    public sealed class PlayerHealth : MonoBehaviour, IHealth
    {
        // ───── Serialized properties ────────────────────────────────────────────────

        [Header("Health")]
        [SerializeField] private float _maxHealth = 100.0f;
        [SerializeField] private float _lowHealthThreshold = 30.0f;
        [SerializeField] private float _healthKitHealAmount = 25.0f;
        
        [Space(10.0f)]
        
        [Header("Audio")]
        [SerializeField] private SoundData _damageSound;
        [SerializeField] private SoundData _deathSound;
        [SerializeField] private SoundData _lowHealthSound;

        // ─── Public properties ───────────────────────────────────────────────────
        
        public event System.Action<float> OnHealthChanged;
        public event System.Action<float> OnDamageReceived;
        public event System.Action<float> OnHealed;
        public event System.Action OnDeath;
        public event System.Action<bool> OnLowHealthStateChanged;

        public float CurrentHealth  { get; private set; }
        public float MaxHealth      => _maxHealth;
        public float HealthKitHeal    => _healthKitHealAmount;
        public float NormalisedHealth => CurrentHealth / _maxHealth;

        public bool  IsDead         => CurrentHealth <= 0.0f;
        public bool  IsLowHealth    => CurrentHealth <= _lowHealthThreshold;
        
        // ─── Private properties ────────────────────────────────────────────

        private bool _wasLowHealth;

        // ─── Lifecycle methods ──────────────────────────────────────────

        private void Awake()
        {
            CurrentHealth = _maxHealth;
            _wasLowHealth = false;
        }

        // ─── Public methods ───────────────────────────────────────────────
        
        public void TakeDamage(float amount)
        {
            if (IsDead)     { return; }
            if (amount <= 0) { return; }

            CurrentHealth  = Mathf.Max(0.0f, CurrentHealth - amount);

            OnDamageReceived?.Invoke(amount);
            OnHealthChanged?.Invoke(CurrentHealth);
            CheckLowHealthTransition();

            AudioManager.Instance?.Play(_damageSound, transform.position);

            if (IsDead)
                OnDeath?.Invoke();
        }
        
        public void UseHealthKit()
        {
            if (IsDead) { return; }
            if (CurrentHealth >= _maxHealth) { return; }

            float previous = CurrentHealth;
            CurrentHealth = Mathf.Min( _maxHealth, CurrentHealth + _healthKitHealAmount);

            float actualHeal = CurrentHealth - previous;

            OnHealed?.Invoke(actualHeal);
            OnHealthChanged?.Invoke(CurrentHealth);
            CheckLowHealthTransition();
        }
        
        public void Respawn()
        {
            CurrentHealth = _maxHealth;
            _wasLowHealth = false;

            OnHealthChanged?.Invoke(CurrentHealth);
            OnLowHealthStateChanged?.Invoke(false);
        }

        // ─── Private methods ──────────────────────────────────────────

        private void CheckLowHealthTransition()
        {
            bool nowLow = IsLowHealth;
            
            // if (nowLow)
                // AudioManager.Instance?.Play(_lowHealthSound, transform.position);

            if (nowLow != _wasLowHealth)
            {
                _wasLowHealth = nowLow;
                OnLowHealthStateChanged?.Invoke(nowLow);
            }
        }
    }
}
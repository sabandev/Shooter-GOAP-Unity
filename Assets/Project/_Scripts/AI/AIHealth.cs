using System;
using UnityEngine;
using Audio;
using Debug = UnityEngine.Debug;

namespace GOAP                                                                                                                                                                                                                   
{         
    /// <summary>
    /// Responsible for handling the AI's health state.
    /// </summary>
    public sealed class AIHealth : MonoBehaviour, IHealth, IDamageContext                                                                                                                                                                       
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [SerializeField] private float _maxHealth = 100.0f;
        [SerializeField] private SoundData _damageSound;   
        [SerializeField] private SoundData _deathSound;

        // ───── Public properties ────────────────────────────────────────────────
        
        public event Action<Vector3, Vector3, float> OnDeath;
                                                                                                                                                                                                                              
        public float CurrentHealth { get; private set; }
        public float MaxHealth => _maxHealth;   
        
        public bool  IsDead => CurrentHealth <= 0.0f;

        // ───── Private properties ────────────────────────────────────────────────
        
        private Vector3 _hitPoint;
        private Vector3 _hitDirection;
        
        private float _force;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void Awake() => CurrentHealth = _maxHealth;

        // ───── Implementations ────────────────────────────────────────────────
        
        public void SetHitContext(Vector3 hitPoint, Vector3 hitDirection, float force)
        {
            _hitPoint = hitPoint;
            _hitDirection = hitDirection;
            _force = force;
        }
        
        public void TakeDamage(float amount)
        {                                                                                                                                                                                                                       
          if (IsDead || amount <= 0.0f) return;
                                               
          CurrentHealth = Mathf.Max(0.0f, CurrentHealth - amount);
          AudioManager.Instance?.Play(_damageSound, transform.position);                                                                                                                                                      
                                                                        
          if (IsDead)                                                                                                                                                                                                         
          {          
              // AudioManager.Instance?.Play(_deathSound, transform.position);
              OnDeath?.Invoke(_hitPoint, _hitDirection, _force);
          }                                                                                                                                                                                                                   
        }
    }                                                                                                                                                                                                                           
}            
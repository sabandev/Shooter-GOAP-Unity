using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// Manages the player's carried items.
    ///
    /// Authorised Use Instructions:
    ///     - MUST NOT own ANY player state. Must delegate state management to separate player class.
    ///       Representation only.
    /// </summary>
    public sealed class PlayerInventory : MonoBehaviour
    {
        // ───── Item types ────────────────────────────────────────────────
        public enum ItemType
        {
            HealthKit,
        }

        // ───── Serialized properties ────────────────────────────────────────────────
        [SerializeField] private int _maxHealthKits = 3;
        [SerializeField] private int _startingHealthKits = 0;

        // ───── Public properties ────────────────────────────────────────────────
        public event Action<int> OnHealthKitCountChanged;
        
        public int HealthKitCount => _items.TryGetValue(ItemType.HealthKit, out int count) ? count : 0;

        // ───── Private properties ────────────────────────────────────────────────
        private PlayerHealth _health;
        private Dictionary<ItemType, int> _items = new  Dictionary<ItemType, int>();

        // ───── Lifecycle methods ────────────────────────────────────────────────
        private void  Awake()
        {
            _health = GetComponent<PlayerHealth>();
            Debug.Assert(_health != null, "[PlayerInventory] PlayerHealth component is missing.", this);
            
            _items[ItemType.HealthKit] = _startingHealthKits; 
        }
        
        private void OnEnable()
        {
            
        }

        // ───── Public methods ────────────────────────────────────────────────
        public void AddHealthKit()
        {
            int current = HealthKitCount;
            if (current >= _maxHealthKits) { return; }
            
            _items[ItemType.HealthKit] = current + 1;
            OnHealthKitCountChanged?.Invoke(HealthKitCount);
        }
        
        public bool TryUseHealthKit()
        {
            if (HealthKitCount <= 0)  { return false; }
            if (_health.CurrentHealth >= _health.MaxHealth) { return false; }
            
            _items[ItemType.HealthKit] = HealthKitCount - 1;
            
            _health.UseHealthKit();
            OnHealthKitCountChanged?.Invoke(HealthKitCount);
            
            Debug.Log($"[PlayerInventory] Syringe used. Remaining: {HealthKitCount}");
            
            return true;
        }
        
        public int GetCount(ItemType type)
        {
            return _items.TryGetValue(type, out int count) ? count : 0;
        }
    }
}

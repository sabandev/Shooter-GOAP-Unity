using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// A snapshot of facts the agent believes to be true about the world.
    /// Uses Dictionary<string, object> to represent world state pairs.
    /// 
    /// Authorised Use Instructions:
    ///     - Clone() method is kept simple and produces a true value copy as we only deal with 
    ///       primitive types (bool/int/float). If reference-type values are added in the future, revisit this method
    ///     - Prefer TryGet() over Get() in performance-sensitive code/hot loops to avoid double-lookup
    /// </summary>
    public sealed class WorldState
    {
        // -----Private properties-----
        private readonly Dictionary<string, object> _facts;

        // -----Constructors-----
        
        // Creates an empty world state with an optional initial capacity for cached memory allocation
        public WorldState(int capacity = 8)
        {
            _facts = new Dictionary<string, object>(capacity);
        }

        // Produces a copy of another WorldState. Used by Planner to check world states without modification.
        public WorldState(WorldState source)
        {
            _facts = new Dictionary<string, object>(source._facts);
        }

        // -----Public methods-----

        //      -----Facts methods-----
        public void Set(string key, object value) => _facts[key] = value;

        // Returns value of given key or null if the key does not exist
        public object Get(string key) => _facts.TryGetValue(key, out object value) ? value : null;

        // Returns value of given key or false if the key does not exist. Preferred.
        public bool TryGet(string key, out object value) => _facts.TryGetValue(key, out value);

        public bool Contains(string key) => _facts.ContainsKey(key);

        public void Remove(string key) => _facts.Remove(key);

        //      -----GOAP methods-----

        // Satisfaction check used by planner to determine whether every key-value pair in the goal
        // is met by this WorldState
        public bool Satisfies(WorldState goal)
        {
            foreach (KeyValuePair<string, object> fact in goal._facts)
            {
                if (!_facts.TryGetValue(fact.Key, out object currentValue))
                {
                    // Debug.Log($"[Satisfies] MISSING key: '{fact.Key}'");
                    return false;
                }

                if (!Equals(currentValue, fact.Value))
                {
                    // Debug.Log($"[Satisfies] MISMATCH key: '{fact.Key}' " +
                    //   $"current: '{currentValue}' ({currentValue?.GetType().Name}) " +
                    //   $"required: '{fact.Value}' ({fact.Value?.GetType().Name})");
                    return false;
                }
            }

            return true;
        }

        // Used by planner to simulate action effects during search
        public void ApplyEffects(WorldState effects)
        {
            foreach (KeyValuePair<string, object> effect in effects._facts)
            {
                _facts[effect.Key] = effect.Value;
            }
        }

        /// <summary>
        /// Returns a read-only enumeration of all facts. Uses a KeyValuePair<string, object> instead of the
        /// already-made WorldStatePair because the GOAP planner calls this within a hot loop, so this 
        /// approach avoids constant memory allocation for WorldStatePair.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, object>> GetFacts() => _facts;

        public WorldState Clone() => new WorldState(this);

        // -----Diagnostics-----
        public override string ToString()
        {
            var sb = new StringBuilder("World State { ");
            foreach (KeyValuePair<string, object> fact in _facts)
                sb.Append($"{fact.Key} = {fact.Value}, ");
            sb.Append("}");
            return sb.ToString();
        }
    }
}


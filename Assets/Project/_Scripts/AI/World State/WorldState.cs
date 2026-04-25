using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// A snapshot of facts the agent believes to be true about the world.
    /// 
    /// Authorised Use Instructions:
    ///     - Clone() method is kept simple and produces a true value copy as we only deal with 
    ///       primitive types (bool/int/float). If reference-type values are added in the future, revisit this method
    ///     - Prefer TryGet() over Get() in performance-sensitive code/hot loops to avoid double-lookup
    ///     - Do NOT use boxed "<string, object>" Dictionaries to avoid over-zealous memory allocation. Used typed
    ///       Dictionaries for new value types.
    /// </summary>
    public sealed class WorldState
    {
        // ───── Private properties ────────────────────────────────────────────────
        
        private readonly Dictionary<string, bool> _boolFacts;
        private readonly Dictionary<string, int> _intFacts;
        private readonly Dictionary<string, float> _floatFacts;
        private readonly Dictionary<string, Transform> _transformFacts;
        private readonly Dictionary<string, Vector3> _vector3Facts;

        // ───── Constructors ────────────────────────────────────────────────
        
        public WorldState(int capacity = 8)
        {
            _boolFacts = new Dictionary<string, bool>(capacity);
            _intFacts = new Dictionary<string, int>(capacity);
            _floatFacts = new Dictionary<string, float>(capacity);
            _transformFacts = new Dictionary<string, Transform>(capacity);
            _vector3Facts = new Dictionary<string, Vector3>(capacity);
        }

        public WorldState(WorldState source)
        {
            _boolFacts = new Dictionary<string, bool>(source._boolFacts);
            _intFacts = new Dictionary<string, int>(source._intFacts);
            _floatFacts = new Dictionary<string, float>(source._floatFacts);
            _transformFacts = new Dictionary<string, Transform>(source._transformFacts);
            _vector3Facts = new Dictionary<string, Vector3>(source._vector3Facts);
        }

        // ───── Setters ────────────────────────────────────────────────
        
        public void Set(string key, bool value) => _boolFacts[key] = value;
        public void Set(string key, int value) => _intFacts[key] = value;
        public void Set(string key, float value) => _floatFacts[key] = value;
        public void Set(string key, Transform value) => _transformFacts[key] = value;
        public void Set(string key, Vector3 value) => _vector3Facts[key] = value;

        // ───── Getters ────────────────────────────────────────────────
        
        public bool GetBool(string key) => _boolFacts.TryGetValue(key, out bool v) ? v : default;
        public int       GetInt(string key)        => _intFacts.TryGetValue(key, out int v)             ? v : default;
        public float     GetFloat(string key)      => _floatFacts.TryGetValue(key, out float v)         ? v : default;                                                                                                      
        public Transform GetTransform(string key)  => _transformFacts.TryGetValue(key, out Transform v) ? v : null;                                                                                                       
        public Vector3   GetVector3(string key)    => _vector3Facts.TryGetValue(key, out Vector3 v)     ? v : default;
        
        public bool TryGet(string key, out bool value)      => _boolFacts.TryGetValue(key, out value);                                                                                                                      
        public bool TryGet(string key, out int value)       => _intFacts.TryGetValue(key, out value);                                                                                                                       
        public bool TryGet(string key, out float value)     => _floatFacts.TryGetValue(key, out value);                                                                                                                     
        public bool TryGet(string key, out Transform value) => _transformFacts.TryGetValue(key, out value);
        public bool TryGet(string key, out Vector3 value)   => _vector3Facts.TryGetValue(key, out value);

        // ───── Public methods ────────────────────────────────────────────────
        
        public bool Contains(string key) => 
                _boolFacts.ContainsKey(key) || 
                _intFacts.ContainsKey(key) || 
                _floatFacts.ContainsKey(key) || 
                _transformFacts.ContainsKey(key) ||
                _vector3Facts.ContainsKey(key);
        
        public void Remove(string key)
        {
            _boolFacts.Remove(key);
            _intFacts.Remove(key);
            _floatFacts.Remove(key);
            _transformFacts.Remove(key);
            _vector3Facts.Remove(key);
        }
        
        /// <summary>
        /// Used in the GOAPPlanner to determine if the goal's desired states are met.
        /// </summary>
        /// <param name="goal"></param>
        /// <returns></returns>
        public bool Satisfies(WorldState goal)
        {
            foreach (var kv in goal._boolFacts)
                if (!_boolFacts.TryGetValue(kv.Key, out bool v) || v != kv.Value) { return false; }
            
            foreach (var kv in goal._intFacts)                                                                                                                                                                              
                if (!_intFacts.TryGetValue(kv.Key, out int v) || v != kv.Value) { return false; }
            
            foreach (var kv in goal._floatFacts)                                                                                                                                                                            
                if (!_floatFacts.TryGetValue(kv.Key, out float v) || !Mathf.Approximately(v, kv.Value)) { return false; }   
            
            foreach (var kv in goal._transformFacts)                                                                                                                                                                        
                if (!_transformFacts.TryGetValue(kv.Key, out Transform v) || v != kv.Value) { return false; }     
            
            foreach (var kv in goal._vector3Facts)                                                                                                                                                                          
                if (!_vector3Facts.TryGetValue(kv.Key, out Vector3 v) || v != kv.Value) { return false; } 
            
            return true;
        }
        
        /// <summary>
        /// Replaces old heuristic logic in the GOAPPlanner. Calculates number of unsatisfied facts in
        /// the current WorldState.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public int CountUnsatisfied(WorldState current)
        {
            int n = 0;
            
            foreach (var kv in _boolFacts)                                                                                                                                                                                  
                if (!current._boolFacts.TryGetValue(kv.Key, out bool v) || v != kv.Value) n++;
            
            foreach (var kv in _intFacts)                                                                                                                                                                                   
                if (!current._intFacts.TryGetValue(kv.Key, out int v) || v != kv.Value) n++;            
            
            foreach (var kv in _floatFacts)                                                                                                                                                                                 
                if (!current._floatFacts.TryGetValue(kv.Key, out float v) || !Mathf.Approximately(v, kv.Value)) n++;   
            
            foreach (var kv in _transformFacts)
                if (!current._transformFacts.TryGetValue(kv.Key, out Transform v) || v != kv.Value) n++;       
            
            foreach (var kv in _vector3Facts)                                                                                                                                                                               
                if (!current._vector3Facts.TryGetValue(kv.Key, out Vector3 v) || v != kv.Value) n++;

            return n;
        }

        /// <summary>
        /// Used by GOAPPlanner to simulate an action's effects during a search for a plan.
        /// </summary>
        /// <param name="effects"></param>
        public void ApplyEffects(WorldState effects)
        {
            foreach (var kv in effects._boolFacts) _boolFacts[kv.Key] = kv.Value;
            foreach (var kv in effects._intFacts) _intFacts[kv.Key] = kv.Value;                                                                                                                                 
            foreach (var kv in effects._floatFacts) _floatFacts[kv.Key] = kv.Value;                                                                                                                                 
            foreach (var kv in effects._transformFacts) _transformFacts[kv.Key] = kv.Value;                                                                                                                                 
            foreach (var kv in effects._vector3Facts) _vector3Facts[kv.Key] = kv.Value;  
        }

        public WorldState Clone() => new WorldState(this);
        
        public void Clear()
        {
            _boolFacts.Clear();
            _intFacts.Clear();
            _floatFacts.Clear();
            _transformFacts.Clear();
            _vector3Facts.Clear();
        }
        
        public void CopyFrom(WorldState source)
        {
            Clear();
            
            foreach (var kv in source._boolFacts) { _boolFacts[kv.Key] = kv.Value; }                                                                                                                                      
            foreach (var kv in source._intFacts) { _intFacts[kv.Key] = kv.Value; }
            foreach (var kv in source._floatFacts) { _floatFacts[kv.Key] = kv.Value; }                                                                                                                                      
            foreach (var kv in source._transformFacts) { _transformFacts[kv.Key] = kv.Value; }
            foreach (var kv in source._vector3Facts) { _vector3Facts[kv.Key] = kv.Value; }  
        }

        // ───── Helper methods ────────────────────────────────────────────────
        
        public override string ToString()
        {
            var sb = new StringBuilder("WorldState { ");
            
            foreach (var kv in _boolFacts)      sb.Append($"{kv.Key}={kv.Value} ");
            foreach (var kv in _intFacts)       sb.Append($"{kv.Key}={kv.Value} ");                                                                                                                                         
            foreach (var kv in _floatFacts)     sb.Append($"{kv.Key}={kv.Value:F2} ");
            foreach (var kv in _transformFacts) sb.Append($"{kv.Key}={kv.Value?.name} ");                                                                                                                                   
            foreach (var kv in _vector3Facts)   sb.Append($"{kv.Key}={kv.Value} ");                                                                                                                                         
            sb.Append("}");                             
            
            return sb.ToString();   
        }
                  
        #if UNITY_EDITOR
        public IEnumerable<KeyValuePair<string, object>> GetFacts()                                                                                                                                      
        {                                                                                                                                                                                                                           
          foreach (var kv in _boolFacts)      yield return new KeyValuePair<string, object>(kv.Key, kv.Value);
          foreach (var kv in _intFacts)       yield return new KeyValuePair<string, object>(kv.Key, kv.Value);                                                                                                                    
          foreach (var kv in _floatFacts)     yield return new KeyValuePair<string, object>(kv.Key, kv.Value);                                                                                                                    
          foreach (var kv in _transformFacts) yield return new KeyValuePair<string, object>(kv.Key, kv.Value);                                                                                                                    
          foreach (var kv in _vector3Facts)   yield return new KeyValuePair<string, object>(kv.Key, kv.Value);                                                                                                                    
        }               
                                                                                                                                                                                                                                  
        public bool TryGetBoxed(string key, out object value)                                                                                                                                                                       
        {
          if (_boolFacts.TryGetValue(key, out bool b))           { value = b; return true; }                                                                                                                                      
          if (_intFacts.TryGetValue(key, out int i))             { value = i; return true; }
          if (_floatFacts.TryGetValue(key, out float f))         { value = f; return true; }                                                                                                                                      
          if (_transformFacts.TryGetValue(key, out Transform t)) { value = t; return true; }
          if (_vector3Facts.TryGetValue(key, out Vector3 v))     { value = v; return true; }                                                                                                                                      
          value = null;
          return false;
        }
        #endif
    }
}


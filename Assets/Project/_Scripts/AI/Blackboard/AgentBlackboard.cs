using System;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Stores the WorldState facts for a single GOAP agent.
    /// Written to by sensors, read by the planner and action instances.
    /// </summary>
    public sealed class AgentBlackboard
    {
        // ───── Public properties ────────────────────────────────────────────────
        
        public WorldState WorldState => _worldState;
        
        // ───── Private properties ────────────────────────────────────────────────
        
        private readonly WorldState _worldState = new();
        private readonly Dictionary<string, List<Action<object>>> _changeCallbacks = new();

        // ───── Getters ────────────────────────────────────────────────
        
        public bool GetBool(string key) => _worldState.GetBool(key);
        public int GetInt(string key) => _worldState.GetInt(key);
        public float GetFloat(string key) => _worldState.GetFloat(key);
        public Transform GetTransform(string key) => _worldState.GetTransform(key);
        public Vector3 GetVector3(string key) => _worldState.GetVector3(key);

        // ───── Public methods ────────────────────────────────────────────────
        
        public WorldState GetWorldStateSnapshot() => _worldState.Clone();

        public void Set(string key, bool value)
        {
            bool changed = !_worldState.TryGet(key, out bool current) || current != value;
            _worldState.Set(key, value);
            if (changed) NotifyCallbacks(key, value);
        }
        
        public void Set(string key, int value)
        {
            bool changed = !_worldState.TryGet(key, out int current) || current != value;
            _worldState.Set(key, value);
            if (changed) NotifyCallbacks(key, value);
        }
        
        public void Set(string key, float value)
        {
            bool changed = !_worldState.TryGet(key, out float current) || !Mathf.Approximately(current, value);
            _worldState.Set(key, value);
            if (changed) NotifyCallbacks(key, value);
        }
        
        public void Set(string key, Transform value)
        {
            bool changed = !_worldState.TryGet(key, out Transform current) || current != value;
            _worldState.Set(key, value);
            if (changed) NotifyCallbacks(key, value);
        }
        
        public void Set(string key, Vector3 value)
        {
            bool changed = !_worldState.TryGet(key, out Vector3 current) || current != value;
            _worldState.Set(key, value);
            if (changed) NotifyCallbacks(key, value);
        }

        /// <summary>
        /// Returns the type of the value given a key or a default type if the key does not exist. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            if (typeof(T) == typeof(bool)) return (T)(object)_worldState.GetBool(key);
            if (typeof(T) == typeof(int)) return (T)(object)_worldState.GetInt(key); 
            if (typeof(T) == typeof(float)) return (T)(object)_worldState.GetFloat(key);                                                                                                                                        
            if (typeof(T) == typeof(Transform)) return (T)(object)_worldState.GetTransform(key);
            if (typeof(T) == typeof(Vector3)) return (T)(object)_worldState.GetVector3(key);                                                                                                                                      
            if (typeof(T).IsEnum) return (T)(object)_worldState.GetInt(key);                                                                                                                                          
            return default;  
        }

        /// <summary>
        /// Returns true/false depending on if the key exists on the blackboard.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(string key) => _worldState.Contains(key);

        /// <summary>
        /// Registers a callback whenever the value for a given key changes.
        /// Used by the GOAP agent to trigger immediate re-planning when the world state changes significantly.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        public void RegisterChangeCallback(string key, Action<object> callback)
        {
            if (!_changeCallbacks.TryGetValue(key, out List<Action<object>> callbacks))
            {
                callbacks = new List<Action<object>>();
                _changeCallbacks[key] = callbacks;
            }

            callbacks.Add(callback);
        }

        /// <summary>
        /// Removes a previously registered callback.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        /// <param name="callback"></param>
        public void UnregisterChangeCallback(string key, Action<object> callback)
        {
            if (_changeCallbacks.TryGetValue(key, out List<Action<object>> callbacks))
                callbacks.Remove(callback);
        }

        // -----Private methods-----

        private void NotifyCallbacks(string key, object newValue)
        {
            if (!_changeCallbacks.TryGetValue(key, out List<Action<object>> callbacks)) { return; }

            for (int i = 0; i < callbacks.Count; i++)
                callbacks[i]?.Invoke(newValue);
        }
    }
}
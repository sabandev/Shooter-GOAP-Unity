using System;
using System.Collections.Generic;

namespace GOAP
{
    /// <summary>
    /// Stores the WorldState facts for a single GOAP agent.
    /// Written to by sensors, read by the planner and action instances.
    /// </summary>
    public sealed class AgentBlackboard
    {   
        // -----Private properties-----
        private readonly WorldState _worldState = new();
        private readonly Dictionary<string, List<Action<object>>> _changeCallbacks = new();

        // -----Public methods-----
        public WorldState GetWorldStateSnapshot() => _worldState.Clone();

        /// <summary>
        /// Sets a fact on the blackboard.
        /// If any changes to the blackboard occur, notifies consumers.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, object value)
        {
            bool changed = !_worldState.TryGet(key, out object current) || !Equals(current, value);

            _worldState.Set(key, value);

            if (changed)
                NotifyCallbacks(key, value);
        }

        /// <summary>
        /// Returns raw value for a given key or null if the key does not exist.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key) => _worldState.Get(key);

        /// <summary>
        /// Returns the type of the value given a key or a default type if the key does not exist. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            object value = _worldState.Get(key);
            return value is T typed ? typed : default;
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
        public void UnregisterChangeCallback(string key, Action<object> callback)
        {
            if (!_changeCallbacks.TryGetValue(key, out List<Action<object>> callbacks))
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
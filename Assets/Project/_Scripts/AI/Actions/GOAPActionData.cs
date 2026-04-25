using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// The action data that specifies immutable states and information about given actions.
    /// 
    /// Authorised Use Instructions:
    ///     - Must not store any runtime or per-agent mutable state
    ///     - Always add [CreateAssetMenu] to subclasses
    ///     - Override CreateInstance() to link this data to a GOAPActionInstance
    /// </summary>
    public abstract class GOAPActionData : ScriptableObject
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [SerializeField] private string _actionName = "New Action";
        [SerializeField] [Min(0.0f)] private float _cost = 1.0f;
        [SerializeField] private List<WorldStatePair> _preconditions = new();
        [SerializeField] private List<WorldStatePair> _effects = new();

        // ───── Public properties ────────────────────────────────────────────────
        
        public string ActionName => _actionName;
        public float Cost => _cost;
        public IReadOnlyList<WorldStatePair> Preconditions => _preconditions;
        public IReadOnlyList<WorldStatePair> Effects => _effects;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void OnValidate()
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(_actionName), "[ActionData] ActionName is empty. Must assign a valid action name.", this);
            Debug.Assert(_effects != null && _effects.Count > 0, $"[ActionData] '{name}' has no effects. Must assign effects or the planner will never select this action.", this);
        }

        // ───── Public methods ────────────────────────────────────────────────

        /// <summary>
        /// Override to create and return the corresponding GOAPActionInstance and assign it this action's data.
        /// Specific GOAPActionInstance properties should be passed along through this method (e.g. CoverNode, searchRadius, etc)
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public abstract GOAPActionInstance CreateInstance(GOAPAgent agent);

        /// <summary>
        /// Builds and returns this action's preconditions as a WorldState for use by the planner
        /// to check if the current search node satisfies the action's preconditions.
        /// </summary>
        /// <returns></returns>
        // public WorldState GetPreconditionsAsWorldState()
        // {
        //     var state = new WorldState(_preconditions.Count);
        //
        //     foreach (WorldStatePair pair in _preconditions)
        //         state.Set(pair.Key, pair.GetValue());
        //
        //     return state;
        // }
        
        /// <summary>
        /// Builds and returns this action's preconditions as a WorldState for use by the planner
        /// to check what the WorldState will be after this action's effects are applied.
        /// </summary>
        /// <returns></returns>
        // public WorldState GetEffectsAsWorldState()
        // {
        //     var state = new WorldState(_effects.Count);
        //
        //     foreach (WorldStatePair pair in _effects)
        //         state.Set(pair.Key, pair.GetValue());
        //
        //     return state;
        // }
    }
}


using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// ScriptableObject that defines what a SmartObject advertises
    /// to agents — what action it enables, what world state it
    /// produces, and what preconditions must be met to use it.
    /// </summary>
    [CreateAssetMenu(fileName = "SMARTOBJECTDATA_New", menuName = "GOAP/Smart Objects/Smart Object Data")]
    public class SmartObjectData : ScriptableObject
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("Identity")]
        [SerializeField] private string _objectTag = "SmartObject";
        [SerializeField] private string _description = "";

        [Space(10.0f)]

        [Header("Advertisement")]
        [SerializeField] private List<WorldStatePair> _advertisedEffects = new List<WorldStatePair>();
        [SerializeField] private List<WorldStatePair> _requiredAgentState = new List<WorldStatePair>();

        [Space(10.0f)]

        [Header("Interaction")]
        [SerializeField] [Min(0.0f)] private float _baseCost = 1.0f;
        [SerializeField] [Min(0.01f)] private float _interactionRange = 1.2f;
        [SerializeField] private Vector3 _interactionOffset = new Vector3(0.0f, 0.0f, -1.0f);

        [Space(10.0f)]

        [Header("Availability")]
        [SerializeField] private bool _allowsSimultaneousUse = false;
        [SerializeField] [Min(0.0f)] private float _cooldownDuration = 0.0f;

        // ───── Public properties ────────────────────────────────────────────────
        
        public string ObjectTag => _objectTag;
        public string Description => _description;
        public IReadOnlyList<WorldStatePair> AdvertisedEffects => _advertisedEffects;
        public IReadOnlyList<WorldStatePair> RequiredAgentState => _requiredAgentState;
        public float BaseCost => _baseCost;
        public float InteractionRange => _interactionRange;
        public Vector3 InteractionOffset => _interactionOffset;
        public bool AllowsSimultaneousUse => _allowsSimultaneousUse;
        public float CooldownDuration => _cooldownDuration;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void OnValidate()
        {
            _baseCost = Mathf.Max(0.0f, _baseCost);
            _interactionRange = Mathf.Max(0.01f, _interactionRange);
            _cooldownDuration = Mathf.Max(0.0f, _cooldownDuration);
            
            Debug.Assert(!string.IsNullOrWhiteSpace(_objectTag), $"[SmartObjectData] '{name}' ObjectTag is empty. Must assign an ObjectTag.", this);
            Debug.Assert(_advertisedEffects != null && _advertisedEffects.Count > 0, $"[SmartObjectData] '{name}' AdvertisedEffects is empty. Must assign AdvertisedEffects.", this);
        }

        // ───── Public methods ────────────────────────────────────────────────
        
        public WorldState GetAdvertisedWorldState()
        {
            var state = new WorldState();
            foreach (WorldStatePair pair in _advertisedEffects)
                pair.ApplyTo(state);

            return state;
        }

        public WorldState GetRequiredAgentState()
        {
            var state = new WorldState();
            foreach (WorldStatePair pair in _requiredAgentState)
                pair.ApplyTo(state);
                
            return state;
        }
    }
}
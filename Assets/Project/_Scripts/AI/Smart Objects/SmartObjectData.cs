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
        // -----Serialized properties-----
        [Header("Identity")]
        [SerializeField] private string _objectTag = "SmartObject";
        [SerializeField] private string _description = "";

        [Space(10.0f)]

        [Header("Advertisement")]
        [SerializeField] private List<WorldStatePair> _advertisedEffects = new List<WorldStatePair>();
        [SerializeField] private List<WorldStatePair> _requiredAgentState = new List<WorldStatePair>();

        [Space(10.0f)]

        [Header("Interaction")]
        [SerializeField] private float _baseCost = 1.0f;
        [SerializeField] private float _interactionRange = 1.2f;
        [SerializeField] private Vector3 _interactionOffset = new Vector3(0.0f, 0.0f, -1.0f);

        [Space(10.0f)]

        [Header("Availability")]
        [SerializeField] private bool _allowsSimultaneousUse = false;
        [SerializeField] private float _cooldownDuration = 0.0f;

        // -----Public properties-----
        public string ObjectTag => _objectTag;
        public string Description => _description;
        public IReadOnlyList<WorldStatePair> AdvertisedEffects => _advertisedEffects;
        public IReadOnlyList<WorldStatePair> RequiredAgentState => _requiredAgentState;
        public float BaseCost => _baseCost;
        public float InteractionRange => _interactionRange;
        public Vector3 InteractionOffset => _interactionOffset;
        public bool AllowsSimultaneousUse => _allowsSimultaneousUse;
        public float CooldownDuration => _cooldownDuration;

        // -----Public methods------
        public WorldState GetAdvertisedWorldState()
        {
            var state = new WorldState();
            foreach (WorldStatePair pair in _advertisedEffects)
                state.Set(pair.Key, pair.GetValue());

            return state;
        }

        public WorldState GetRequiredAgentState()
        {
            var state = new WorldState();
            foreach (WorldStatePair pair in _requiredAgentState)
                state.Set(pair.Key, pair.GetValue());
                
            return state;
        }
    }
}
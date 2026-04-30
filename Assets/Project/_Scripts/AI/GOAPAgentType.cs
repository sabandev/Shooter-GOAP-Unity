using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Represents an agent type with assigned goals and actions a GOAP agent can use.
    /// </summary>
    [CreateAssetMenu(fileName = "AIType_New", menuName = "GOAP/Agent Type")]
    public sealed class GOAPAgentType : ScriptableObject
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [SerializeField] private string _typeName = "New Type";
        [SerializeField] private List<GOAPActionData> _actions = new();
        [SerializeField] private List<GOAPGoal> _goals = new();

        // ───── Public properties ────────────────────────────────────────────────
        
        public string TypeName => _typeName;
        public IReadOnlyList<GOAPActionData> Actions => _actions;
        public IReadOnlyList<GOAPGoal> Goals => _goals;
    }
}
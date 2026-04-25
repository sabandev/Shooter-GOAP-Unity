using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{    
    /// <summary>
    /// Defines a goal a GOAP agent can work towards.
    /// A goal is represented by a desired WorldState and is chosen on a priority system.
    /// 
    /// Authorised Use Instructions:
    ///     - Override EvaluatePriority methods in subclasses to make dynamic priority adjustments at runtime
    /// </summary>
    [CreateAssetMenu(fileName = "Goal_New", menuName = "GOAP/Goal")]
    public class GOAPGoal : ScriptableObject
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [SerializeField] private string _goalName = "New Goal";
        [SerializeField] [Range(0, 100)] private int _basePriority = 1;
        [SerializeField] private List<WorldStatePair> _desiredState = new();

        // ───── Public properties ────────────────────────────────────────────────
        
        public string GoalName => _goalName;
        public int BasePriority => _basePriority;

        // ───── Public methods ────────────────────────────────────────────────

        // Builds and returns a WorldState based on the list of WorldStatePairs we own (desired state)
        // Should only be called once per planning cycle and avoid calling in hot loops due to memory allocation when creating a new WorldState
        public WorldState GetDesiredState()
        {
            var desiredState = new WorldState(_desiredState.Count);

            foreach (WorldStatePair pair in _desiredState)
                pair.ApplyTo(desiredState);

            return desiredState;
        }

        // Override to change priority of this goal. Returns base priority by default
        public virtual int EvaluatePriority(WorldState agentState) => _basePriority;
    }
}


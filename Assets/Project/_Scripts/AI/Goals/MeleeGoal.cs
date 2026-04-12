using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Goal: Elimate the target using melee combat.
    /// </summary>
    [CreateAssetMenu(fileName = "GOAL_Melee", menuName = "GOAP/Goals/Melee")]
    public sealed class MeleeGoal : GOAPGoal
    {
        // -----Serialized properties-----
        [SerializeField] private int _alertPriority = 90;

        public override int EvaluatePriority(WorldState agentState)
        {
            bool targetDead = agentState.Get(BlackboardKeys.TARGET_IS_DEAD) is true;

            if (targetDead) { return 0; }

            bool targetVisible = agentState.Get(BlackboardKeys.TARGET_VISIBLE) is true;

            return targetVisible ? _alertPriority : BasePriority;
        }
    }
}


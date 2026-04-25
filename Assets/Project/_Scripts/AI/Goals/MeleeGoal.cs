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
            bool targetDead = agentState.GetBool(BlackboardKeys.TARGET_IS_DEAD);

            if (targetDead) { return 0; }

            bool targetVisible = agentState.GetBool(BlackboardKeys.TARGET_VISIBLE);

            return targetVisible ? _alertPriority : BasePriority;
        }
    }
}


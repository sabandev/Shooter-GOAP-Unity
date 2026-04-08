using UnityEngine;

namespace GOAP
{
    [CreateAssetMenu(fileName = "GOAL_Investigate", menuName = "GOAP/Goals/TEST0/Investigate")]
    public sealed class InvestigateGoal : GOAPGoal
    {
        // -----Serialized properties-----
        [SerializeField] private int _alertPriority = 80;

        // -----Implementation-----
        public override int EvaluatePriority(WorldState agentState)
        {
            bool targetVisible = agentState.Get(BlackboardKeys.TARGET_VISIBLE) is true;
            return targetVisible ? _alertPriority : BasePriority;
        }
    }
}

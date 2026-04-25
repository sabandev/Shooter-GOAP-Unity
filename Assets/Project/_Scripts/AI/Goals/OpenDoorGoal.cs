using UnityEngine;

namespace GOAP
{
    [CreateAssetMenu(fileName = "GOAL_OpenDoor", menuName = "GOAP/Goals/Open Door")]
    public sealed class OpenDoorGoal : GOAPGoal
    {
        [SerializeField] private int _alertPriority = 75;

        public override int EvaluatePriority(WorldState agentState)
        {
            bool inProgress = agentState.GetBool(BlackboardKeys.DOOR_IS_OPENING);
            if (inProgress) { return _alertPriority; }
            
            bool doorOpen = agentState.GetBool(BlackboardKeys.DOOR_IS_OPEN);
            if (doorOpen) { return 0; }
            
            bool doorAhead = agentState.GetBool(BlackboardKeys.DOOR_AHEAD);
            
            return doorAhead ? _alertPriority : BasePriority;
        }
    }
}

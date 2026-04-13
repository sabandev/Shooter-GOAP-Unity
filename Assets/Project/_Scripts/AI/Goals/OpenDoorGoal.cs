using UnityEngine;

namespace GOAP
{
    [CreateAssetMenu(fileName = "GOAL_OpenDoor", menuName = "GOAP/Goals/Open Door")]
    public sealed class OpenDoorGoal : GOAPGoal
    {
        [SerializeField] private int _alertPriority = 75;

        public override int EvaluatePriority(WorldState agentState)
        {
            bool inProgress = agentState.Get(BlackboardKeys.DOOR_IS_OPENING) is true;
            if (inProgress) { return _alertPriority; }
            
            bool doorOpen = agentState.Get(BlackboardKeys.DOOR_IS_OPEN) is true;
            if (doorOpen) { return 0; }
            
            bool doorAhead = agentState.Get(BlackboardKeys.DOOR_AHEAD) is true;
            
            return doorAhead ? _alertPriority : BasePriority;
        }
    }
}

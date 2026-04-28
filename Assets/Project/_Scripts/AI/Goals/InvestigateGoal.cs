using UnityEngine;
                                                                                                                                                                                                                                  
namespace GOAP  
{
    [CreateAssetMenu(fileName = "GOAL_Investigate", menuName = "GOAP/Goals/Investigate")]
    public sealed class InvestigateGoal : GOAPGoal                                                                                                                                                                              
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [SerializeField] private int _investigatePriority = 60;

        // ───── Implementation ────────────────────────────────────────────────
        
        public override int EvaluatePriority(WorldState agentState)                                                                                                                                                             
        {
            if (agentState.GetBool(BlackboardKeys.TARGET_IS_DEAD))   return 0;                                                                                                                                                  
            if (agentState.GetBool(BlackboardKeys.TARGET_VISIBLE))   return 0;
                                                                                                                                                                                                                              
            return agentState.GetBool(BlackboardKeys.IS_INVESTIGATING) ? _investigatePriority : BasePriority;                                                                                                                                                                                                 
        }       
    }
}
using UnityEngine;

namespace GOAP
{
    [CreateAssetMenu(fileName = "Goal_Pickup", menuName = "GOAP/Goals/Pickup")]
    public sealed class PickupGoal : GOAPGoal
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("Pickup Goal")]
        [SerializeField] private int _alertPriority = 60;
        [SerializeField] private string _hasItemKey = "HAS_ITEM";
        [SerializeField] private string _itemAvailableKey = "PICKUP_AVAILABLE";

        // ───── Implementation ────────────────────────────────────────────────
        
        public override int EvaluatePriority(WorldState agentState)
        {
            // Don't pursue if already have item
            if (agentState.GetBool(_hasItemKey)) { return 0; }
            
            return agentState.GetBool(_itemAvailableKey) ? _alertPriority : BasePriority;
        }
    }
}
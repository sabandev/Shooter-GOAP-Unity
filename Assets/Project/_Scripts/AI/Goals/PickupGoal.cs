using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Goal: acquire a nearby item.
    ///
    /// Priority activates when an item is available nearby and
    /// the agent doesn't already have one. Returns to base
    /// priority once the item is acquired.
    ///
    /// Desired state: HAS_ITEM = true
    /// </summary>
    [CreateAssetMenu(
        fileName = "Goal_Pickup",
        menuName = "GOAP/Goals/Pickup")]
    public sealed class PickupGoal : GOAPGoal
    {
        [Header("Pickup Goal")]
        [Tooltip("Priority when a pickup is available and agent " +
                 "doesn't have the item yet.")]
        [SerializeField] private int _alertPriority = 60;

        [Tooltip("Blackboard key checked to determine if agent " +
                 "already has the item. Matches PickupActionData " +
                 "AcquiredItemKey.")]
        [SerializeField] private string _hasItemKey = "HAS_ITEM";

        [Tooltip("Blackboard key written by SmartObjectSensor " +
                 "when a pickup is available. Should match " +
                 "'{TAG}_AVAILABLE' pattern.")]
        [SerializeField] private string _itemAvailableKey = "PICKUP_AVAILABLE";

        public override int EvaluatePriority(WorldState agentState)
        {
            // Don't pursue if already have item
            object hasItem = agentState.Get(_hasItemKey);
            if (hasItem is true) return 0;

            // Only activate if item is available
            object available = agentState.Get(_itemAvailableKey);
            return available is true ? _alertPriority : BasePriority;
        }
    }
}
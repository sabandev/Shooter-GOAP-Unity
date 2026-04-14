using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Detects closed doors within proximity roughly ahead of the GOAP agent.
    /// </summary>
    public class DoorSensor : Sensor
    {
        // -----Serialized properties-----
        [Header("Door Detection")]
        [SerializeField] private float _detectionRange = 4.0f;
        [SerializeField] private float _forwardAngle = 60.0f;
        [SerializeField] private string _doorTag = "Door";
        
        [Space(10.0f)]
        
        [Header("Gizmos")]
        [SerializeField] private bool _drawDoorDetectionGizmos = true;
        
        // -----Implementation-----
        protected override void Sense()
        {
            SmartObject nearest = SmartObjectRegistry.Instance.FindNearestForAgent(_doorTag, Agent.transform.position, Agent);
            
            if (nearest == null)
            {
                ClearDoorAhead();
                return;
            }
            
            float dist = Vector3.Distance(Agent.transform.position, nearest.transform.position);
            if (dist > _detectionRange)
            {
                ClearDoorAhead();
                return;
            }
            
            Vector3 toDoor = (nearest.transform.position - Agent.transform.position).normalized;
            float angle = Vector3.Angle(Agent.transform.forward, toDoor);
            
            if (angle >= _forwardAngle)
            {
                ClearDoorAhead();
                return;
            }
            
            Door door = nearest.GetComponent<Door>();
            if (door == null)
            {
                ClearDoorAhead();
                return;
            }
            
            switch (door.State)
            {
                case Door.DoorState.Open:
                    // Fully open — write fact, clear door ahead
                    // Agent can pass through freely
                    Agent.Blackboard.Set(BlackboardKeys.DOOR_IS_OPEN, true);
                    Agent.Blackboard.Set(BlackboardKeys.DOOR_AHEAD,   false);
                    break;

                case Door.DoorState.Opening:
                    // Mid-swing — keep DOOR_AHEAD true so OpenDoorGoal
                    // stays active and agent waits for animation to finish
                    // Do NOT write DOOR_IS_OPEN = true yet
                    Agent.Blackboard.Set(BlackboardKeys.DOOR_AHEAD,   true);
                    Agent.Blackboard.Set(BlackboardKeys.DOOR_IS_OPEN, false);
                    Agent.Blackboard.Set(BlackboardKeys.DOOR_TRANSFORM,
                        nearest.transform);
                    break;

                case Door.DoorState.Closed:
                    // Closed — agent needs to open it
                    Agent.Blackboard.Set(BlackboardKeys.DOOR_AHEAD,   true);
                    Agent.Blackboard.Set(BlackboardKeys.DOOR_IS_OPEN, false);
                    Agent.Blackboard.Set(BlackboardKeys.DOOR_TRANSFORM,
                        nearest.transform);
                    break;
            }
        }
        
        // -----Private methods-----
        private void ClearDoorAhead()
        {
            bool currentlyAhead = Agent.Blackboard.Get<bool>(BlackboardKeys.DOOR_AHEAD);
            
            if (!currentlyAhead) { return; }
            
            Agent.Blackboard.Set(BlackboardKeys.DOOR_AHEAD, false);
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_drawDoorDetectionGizmos) { return; }
            
            GetAgent();
            if (Agent == null) { return; }
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Agent.transform.position, _detectionRange);
            
            Vector3 leftBound = Quaternion.Euler(0.0f, -_forwardAngle, 0.0f) * transform.forward;
            Vector3 rightBound = Quaternion.Euler(0.0f, _forwardAngle, 0.0f) * transform.forward;
            
            Gizmos.color = Color.yellowGreen;
            Gizmos.DrawRay(Agent.transform.position, leftBound * _detectionRange);
            Gizmos.DrawRay(Agent.transform.position, rightBound* _detectionRange);
        }
        #endif
    }
}

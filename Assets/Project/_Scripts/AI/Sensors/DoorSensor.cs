using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Detects closed doors within proximity roughly ahead of the GOAP agent.
    /// </summary>
    public class DoorSensor : Sensor
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("Door Detection")]
        [SerializeField] [Min(0.01f)] private float _detectionRange = 4.0f;
        [SerializeField] [Min(0.01f)] private float _forwardAngle = 60.0f;
        [SerializeField] private string _doorTag = "Door";
        [SerializeField] [Min(0.01f)] private float _pathProximityThreshold = 1.5f;
        
        [Space(10.0f)]
        
        [Header("Gizmos")]
        [SerializeField] private bool _drawDoorDetectionGizmos = true;

        // ───── Implementation ────────────────────────────────────────────────
        
        protected override void Sense()
        {
            SmartObject nearest = SmartObjectRegistry.Instance.FindNearestForAgent(_doorTag, Agent.transform.position, Agent);
            
            if (nearest == null) { ClearDoorAhead(); return; }
            
            float dist = Vector3.Distance(Agent.transform.position, nearest.transform.position);
            if (dist > _detectionRange) { ClearDoorAhead(); return; }
            
            Vector3 toDoor = (nearest.transform.position - Agent.transform.position).normalized;
            float angle = Vector3.Angle(Agent.transform.forward, toDoor);
            if (angle >= _forwardAngle) { ClearDoorAhead(); return; }
            
            Door door = nearest.GetComponent<Door>();
            if (door == null) { ClearDoorAhead(); return; }
            
            switch (door.State)
            {
                case Door.DoorState.Open:
                    Agent.Blackboard.Set(BlackboardKeys.DOOR_IS_OPEN, true);
                    Agent.Blackboard.Set(BlackboardKeys.DOOR_AHEAD,   false);
                    break;

                case Door.DoorState.Opening:
                    Agent.Blackboard.Set(BlackboardKeys.DOOR_AHEAD,   true);
                    Agent.Blackboard.Set(BlackboardKeys.DOOR_IS_OPEN, false);
                    Agent.Blackboard.Set(BlackboardKeys.DOOR_TRANSFORM, nearest.transform);
                    break;

                case Door.DoorState.Closed:
                    if (!DoorAlongPath(nearest.transform.position)) { ClearDoorAhead(); return; }
                    
                    Agent.Blackboard.Set(BlackboardKeys.DOOR_AHEAD,   true);
                    Agent.Blackboard.Set(BlackboardKeys.DOOR_IS_OPEN, false);
                    Agent.Blackboard.Set(BlackboardKeys.DOOR_TRANSFORM, nearest.transform);
                    break;
            }
        }

        // ───── Private methods ────────────────────────────────────────────────
        
        /// <summary>
        /// Used to check if the found door is ALSO on the agent's path. Prevents opening
        /// irrelevant doors.
        /// </summary>
        /// <param name="doorPosition"></param>
        /// <returns></returns>
        private bool DoorAlongPath(Vector3 doorPosition)
        {
            if (!Agent.NavAgent.hasPath) { return false; }
            
            Vector3[] corners = Agent.NavAgent.path.corners;
            if (corners.Length < 2) { return false; }
            
            for (int i = 0; i < corners.Length - 1; i++)
            {
                if (DistanceToSegmetXZ(doorPosition, corners[i], corners[i + 1]) <= _pathProximityThreshold) { return true; }
            }
            
            return false;
        }
        
        private static float DistanceToSegmetXZ(Vector3 point, Vector3 a, Vector3 b)
        {
            Vector2 p = new Vector2(point.x, point.z);
            Vector2 sa = new Vector2(a.x, a.z);
            Vector2 sb = new Vector2(b.x, b.z);
            
            Vector2 ab = sb - sa;
            float sqrLen = ab.sqrMagnitude;
            if (sqrLen < 0.0001f) { return Vector2.Distance(p, sa); }
            
            float t = Mathf.Clamp01(Vector2.Dot(p - sa, ab) / sqrLen);
            Vector2 closest = sa + t * ab;
            return Vector2.Distance(p, closest);
        }
        
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

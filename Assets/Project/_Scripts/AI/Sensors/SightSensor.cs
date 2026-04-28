using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Sensor that a GOAP agent can own and have access to its results. Detects whether the agent has
    /// line-of-sight to a target.
    /// Writes vision information to the agent's blackboard.
    /// </summary>
    public sealed class SightSensor : Sensor
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [SerializeField] private Transform eyeLocation;
        [SerializeField] [Min(0.0f)] private float _sightRange = 20.0f;
        [SerializeField] [Range(1.0f, 180.0f)] private float _sightAngle = 90.0f;
        [SerializeField] [Min(0.0f)] private float _sightMemoryDuration = 2.0f;
        [SerializeField] private LayerMask _targetMask;
        [SerializeField] private LayerMask _occlusionMask;

        [Space(10.0f)]

        [Header("Gizmos")]
        [SerializeField] private bool _drawVisualiserGizmos = true;
        [SerializeField] private bool _drawSightSphereOnSelectGizmos = true;
        [SerializeField] private bool _drawVisionConeGizmos = true;
        [SerializeField] private bool _drawPathToLastKnownTargetPositionGizmos = true;

        // ───── Private properties ────────────────────────────────────────────────
        
        private readonly Collider[] _hits = new Collider[16];
        
        private Vector3 _prevTargetPos;
        
        private bool _wasLOS;
        private bool _wasEffectivelyVisible;
        private bool _prevTargetPosValid;
        
        private float _lostSightTime = float.NegativeInfinity;

        // ───── Implementation ────────────────────────────────────────────────

        /// <summary>
        /// Sends visual information to the agent's blackboard
        /// </summary>
        protected override void Sense()
        {
            bool hasLOS = false;
            Transform nearest = null;
            
            int count = Physics.OverlapSphereNonAlloc(transform.position, _sightRange, _hits, _targetMask);
            if (count > 0)
            {
                nearest = FindNearest(count);
                if (nearest != null)
                    hasLOS = CheckLineOfSight(nearest);
            }
            
            // When real LOS is first lost
            if (_wasLOS && !hasLOS)
                _lostSightTime = Time.time;
            else if (hasLOS)
                _lostSightTime = float.NegativeInfinity;
            
            bool withinMemory = !hasLOS && _lostSightTime != float.NegativeInfinity && Time.time - _lostSightTime < _sightMemoryDuration;
            bool nowVisible = hasLOS || withinMemory;

            if (_wasEffectivelyVisible && !nowVisible)
            {
                // Target left view and memory expired
                Agent.Blackboard.Set(BlackboardKeys.IS_INVESTIGATING, true);
                Agent.Blackboard.Set(BlackboardKeys.AT_INVESTIGATION_POINT, false);
            }
            else if (!_wasEffectivelyVisible && nowVisible)
                Agent.Blackboard.Set(BlackboardKeys.IS_INVESTIGATING, false);
            
            Agent.Blackboard.Set(BlackboardKeys.TARGET_VISIBLE, nowVisible);
            _wasLOS = hasLOS;
            _wasEffectivelyVisible = nowVisible;
            
            if (hasLOS)
            {
                Agent.Blackboard.Set(BlackboardKeys.TARGET_LAST_KNOWN_POS, nearest.position);
                Agent.Blackboard.Set(BlackboardKeys.TARGET_DISTANCE, Vector3.Distance(Agent.transform.position, nearest.position));
                Agent.Blackboard.Set(BlackboardKeys.TARGET_TRANSFORM, nearest);
                
                // Track movement direction of the target to later bias a search area in that direction
                if (_prevTargetPosValid)
                {
                    Vector3 delta = nearest.position - _prevTargetPos;
                    
                    if (delta.sqrMagnitude > 0.01f)
                        Agent.Blackboard.Set(BlackboardKeys.TARGET_LAST_KNOWN_DIRECTION, delta.normalized);
                }
                _prevTargetPos = nearest.position;
                _prevTargetPosValid = true;
            }
        }

        private Transform FindNearest(int count)
        {
            Transform nearest = null;
            float nearestDist = float.MaxValue;
            
            for (int i = 0; i < count; i++)
            {
                if (_hits[i].transform == transform) { continue; }
                
                float dist = (_hits[i].transform.position - Agent.transform.position).sqrMagnitude;
                
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = _hits[i].transform;
                }
            }

            return nearest;
        }

        private bool CheckLineOfSight(Transform target)
        {
            Vector3 origin = eyeLocation.position;
            Vector3 targetPos = target.position + (Vector3.up * 0.5f);
            Vector3 direction = (targetPos - origin).normalized;

            float angle = Vector3.Angle(Agent.transform.forward, direction);
            if (angle > _sightAngle) { return false; }

            float distance = Vector3.Distance(origin, targetPos);

            return !Physics.Raycast(origin, direction, distance, _occlusionMask);
        }

        // ───── Helper methods ────────────────────────────────────────────────
        
#if UNITY_EDITOR
        /// <summary>
        /// Draws vision cone for debugging
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!_drawVisualiserGizmos) { return; }

            GetAgent();
            if (Agent == null) { return; }

            if (_drawVisionConeGizmos)
            {
                bool targetVisible = Agent.Blackboard.Get<bool>(BlackboardKeys.TARGET_VISIBLE);

                Color coneColor = targetVisible ? new Color(1.0f, 0.3f, 0.3f, 0.12f) : new Color(0.0f, 1.0f, 0.0f, 0.06f);
                Color outlineColor = targetVisible ? new Color(1.0f, 0.3f, 0.3f, 0.6f) : new Color(0.0f, 0.8f, 0.0f, 0.4f);

                UnityEditor.Handles.color = coneColor;
                UnityEditor.Handles.DrawSolidArc(eyeLocation.position, Vector3.up, Quaternion.Euler(0.0f, -_sightAngle, 0.0f) * Agent.transform.forward, _sightAngle * 2.0f, _sightRange);
                UnityEditor.Handles.color = outlineColor;
                UnityEditor.Handles.DrawWireArc(eyeLocation.position, Vector3.up, Quaternion.Euler(0.0f, -_sightAngle, 0.0f) * Agent.transform.forward, _sightAngle * 2.0f, _sightRange);

                Gizmos.color = outlineColor;
                Gizmos.DrawRay(eyeLocation.position, Agent.transform.forward * _sightRange);
            }

            if (Agent.Blackboard.Contains(BlackboardKeys.TARGET_LAST_KNOWN_POS) && _drawPathToLastKnownTargetPositionGizmos)
            {
                Vector3 lastKnown = Agent.Blackboard.Get<Vector3>(BlackboardKeys.TARGET_LAST_KNOWN_POS);

                Gizmos.color = new Color(1.0f, 0.8f, 0.0f, 0.8f);
                Gizmos.DrawLine(Agent.transform.position, lastKnown);
                Gizmos.DrawWireSphere(lastKnown, 0.3f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_drawSightSphereOnSelectGizmos || !_drawVisualiserGizmos) { return; }
            
            GetAgent();
            if (Agent == null) { return; }

            Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.15f);
            Gizmos.DrawWireSphere(Agent.transform.position, _sightRange);
        }
#endif
    }
}


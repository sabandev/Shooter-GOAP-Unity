using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Sensor that a GOAP agent can own and have access to its results. Detects whether the agent has
    /// line-of-sight to a target.
    /// Writes vision information to the agent's blackboard.
    /// </summary>
    [RequireComponent(typeof(GOAPAgent))]
    public sealed class SightSensor : Sensor
    {
        // -----Serialized properties-----
        [SerializeField] private Transform eyeLocation;
        [SerializeField] private float _sightRange = 20.0f;
        [SerializeField] [Range(1.0f, 180.0f)] private float _sightAngle = 90.0f;
        [SerializeField] private LayerMask _targetMask;
        [SerializeField] private LayerMask _occlusionMask;

        [Space(10.0f)]

        [Header("Gizmos")]
        [SerializeField] private bool _drawVisualiserGizmos = true;
        [SerializeField] private bool _drawSightSphereOnSelectGizmos = true;
        [SerializeField] private bool _drawVisionConeGizmos = true;
        [SerializeField] private bool _drawPathToLastKnownTargetPositionGizmos = true;

        // -----Implementation-----

        /// <summary>
        /// Sends visual information to the agent's blackboard
        /// </summary>
        protected override void Sense()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, _sightRange, _targetMask);
            if (hits.Length == 0)
            {
                Agent.Blackboard.Set(BlackboardKeys.TARGET_VISIBLE, false);
                return;
            }

            Transform nearest = FindNearest(hits);
            if (nearest == null)
            {
                Agent.Blackboard.Set(BlackboardKeys.TARGET_VISIBLE, false);
                return;
            }

            bool hasLOS = CheckLineOfSight(nearest);
            Agent.Blackboard.Set(BlackboardKeys.TARGET_VISIBLE, hasLOS);

            if (hasLOS)
            {
                Agent.Blackboard.Set(BlackboardKeys.TARGET_LAST_KNOWN_POS, nearest.position);
                Agent.Blackboard.Set(BlackboardKeys.TARGET_DISTANCE, Vector3.Distance(transform.position, nearest.position));
                Agent.Blackboard.Set(BlackboardKeys.TARGET_TRANSFORM, nearest);
            }
        }

        private Transform FindNearest(Collider[] hits)
        {
            Transform nearest = null;
            float nearestDist = float.MaxValue;

            foreach (Collider hit in hits)
            {
                if (hit.transform == transform) { continue; }

                float dist = (hit.transform.position - transform.position).sqrMagnitude;

                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = hit.transform;
                }
            }

            return nearest;
        }

        private bool CheckLineOfSight(Transform target)
        {
            Vector3 origin = eyeLocation.position;
            Vector3 targetPos = target.position + (Vector3.up * 0.5f);
            Vector3 direction = (targetPos - origin).normalized;

            float angle = Vector3.Angle(transform.forward, direction);
            if (angle > _sightAngle) { return false; }

            float distance = Vector3.Distance(origin, targetPos);

            return !Physics.Raycast(origin, direction, distance, _occlusionMask);
        }

        // -----Editor Helper-----
#if UNITY_EDITOR
        /// <summary>
        /// Draws vision cone for debugging
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) { return; }
            if (!_drawVisualiserGizmos) { return; }

            GOAPAgent agent = GetComponent<GOAPAgent>();
            if (agent == null) { return; }

            if (_drawVisionConeGizmos)
            {
                bool targetVisible = agent.Blackboard.Get<bool>(BlackboardKeys.TARGET_VISIBLE);

                Color coneColor = targetVisible ? new Color(1.0f, 0.3f, 0.3f, 0.12f) : new Color(0.0f, 1.0f, 0.0f, 0.06f);
                Color outlineColor = targetVisible ? new Color(1.0f, 0.3f, 0.3f, 0.6f) : new Color(0.0f, 0.8f, 0.0f, 0.4f);

                UnityEditor.Handles.color = coneColor;
                UnityEditor.Handles.DrawSolidArc(eyeLocation.position, Vector3.up, Quaternion.Euler(0.0f, -_sightAngle, 0.0f) * transform.forward, _sightAngle * 2.0f, _sightRange);
                UnityEditor.Handles.color = outlineColor;
                UnityEditor.Handles.DrawWireArc(eyeLocation.position, Vector3.up, Quaternion.Euler(0.0f, -_sightAngle, 0.0f) * transform.forward, _sightAngle * 2.0f, _sightRange);

                Gizmos.color = outlineColor;
                Gizmos.DrawRay(eyeLocation.position, transform.forward * _sightRange);
            }

            if (agent.Blackboard.Contains(BlackboardKeys.TARGET_LAST_KNOWN_POS) && _drawPathToLastKnownTargetPositionGizmos)
            {
                Vector3 lastKnown = agent.Blackboard.Get<Vector3>(BlackboardKeys.TARGET_LAST_KNOWN_POS);

                Gizmos.color = new Color(1.0f, 0.8f, 0.0f, 0.8f);
                Gizmos.DrawLine(transform.position, lastKnown);
                Gizmos.DrawWireSphere(lastKnown, 0.3f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_drawSightSphereOnSelectGizmos || !_drawVisualiserGizmos) { return; }

            Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.15f);
            Gizmos.DrawWireSphere(transform.position, _sightRange);
        }
#endif
    }
}


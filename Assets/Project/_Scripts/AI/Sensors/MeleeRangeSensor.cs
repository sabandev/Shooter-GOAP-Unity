using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Detects whether the target is within the melee strike range.
    /// Writes melee-related facts to the Blackboard.
    /// 
    /// Authorised Use Instructions:
    ///     - Should run at a higher tick rate as melee range can change rapidly during a chase.
    /// </summary>
    public sealed class MeleeRangeSensor : Sensor
    {
        // -----Serialized properties-----
        [Header("Melee Range")]
        [SerializeField] private float _meleeRange = 1.5f;
        [SerializeField] private LayerMask _targetMask;

        [Space(10.0f)]

        [Header("Gizmos")]
        [SerializeField] private bool _drawMeleeRangeGizmos = true;

        // -----Implementation-----
        protected override void Sense()
        {
            Collider[] hits = Physics.OverlapSphere(Agent.transform.position, _meleeRange, _targetMask);

            if (hits.Length == 0)
            {
                Agent.Blackboard.Set(BlackboardKeys.TARGET_IN_MELEE_RANGE, false);
                return;
            }

            Transform nearest = null;
            float nearestDist = float.MaxValue;

            foreach (Collider hit in hits)
            {
                if (hit.transform == transform) { continue; }

                if (hit.TryGetComponent(out PlayerHealth health) && health.IsDead) { continue; }

                float dist = (hit.transform.position - Agent.transform.position).sqrMagnitude;
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = hit.transform;
                }
            }

            if (nearest == null)
            {
                Agent.Blackboard.Set(BlackboardKeys.TARGET_IN_MELEE_RANGE, false);
                return;
            }

            Agent.Blackboard.Set(BlackboardKeys.TARGET_IN_MELEE_RANGE, true);
            Agent.Blackboard.Set(BlackboardKeys.TARGET_TRANSFORM, nearest);
        }

        // -----Editor helper methods-----
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_drawMeleeRangeGizmos) { return; }
            
            GetAgent();
            if (Agent == null) { return; }
            
            bool inRange = Agent.Blackboard.Get<bool>(BlackboardKeys.TARGET_IN_MELEE_RANGE);

            Gizmos.color = inRange ? new Color(1.0f, 0.2f, 0.2f, 0.4f) : new Color(1.0f, 0.5f, 0.0f, 0.15f);

            Gizmos.DrawWireSphere(Agent.transform.position, _meleeRange);
        }
#endif
    }
}


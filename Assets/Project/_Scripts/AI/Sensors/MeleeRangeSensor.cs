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
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("Melee Range")]
        [SerializeField] private float _meleeRange = 1.5f;
        [SerializeField] private LayerMask _targetMask;

        [Space(10.0f)]

        [Header("Gizmos")]
        [SerializeField] private bool _drawMeleeRangeGizmos = true;

        // ───── Private properties ────────────────────────────────────────────────
        
        private readonly Collider[] _hits = new Collider[16];
        
        // ───── Implementation ────────────────────────────────────────────────
        
        protected override void Sense()
        {
            int count = Physics.OverlapSphereNonAlloc(Agent.transform.position, _meleeRange, _hits, _targetMask);

            if (count == 0)
            {
                Agent.Blackboard.Set(BlackboardKeys.TARGET_IN_MELEE_RANGE, false);
                return;
            }

            Transform nearest = null;
            float nearestDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (_hits[i].transform == transform) { continue; }
                
                if (_hits[i].TryGetComponent(out IHealth health) && health.IsDead) { continue; }
                
                float dist = (_hits[i].transform.position - Agent.transform.position).sqrMagnitude;
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = _hits[i].transform;
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

        // ───── Helper methods ────────────────────────────────────────────────
        
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


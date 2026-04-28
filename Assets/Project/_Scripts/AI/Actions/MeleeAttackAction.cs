using UnityEngine;

namespace GOAP
{
    [CreateAssetMenu(fileName = "ACTION_Melee", menuName = "GOAP/Actions/Melee")]
    public sealed class MeleeAttackActionData : GOAPActionData
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [SerializeField] private LayerMask  _targetMask;
        [SerializeField] [Min(0.0f)] private float _damage = 25.0f;
        [SerializeField] [Min(0.0f)] private float _hitRadius = 1.5f;
        [SerializeField] [Min(0.0f)] private float _attackDuration = 0.8f;
        [SerializeField] [Min(0.0f)] private float _applyDamageNormalisedFrame = 0.4f;
        [SerializeField] [Min(0.0f)] private float _attackCooldown = 1.2f;

        // ───── Implementation ────────────────────────────────────────────────
        
        public override GOAPActionInstance CreateInstance(GOAPAgent agent) => new MeleeAttackActionInstance(agent, this, _damage, _hitRadius, _targetMask, _attackDuration, _applyDamageNormalisedFrame, _attackCooldown);
    }

    /// <summary>
    /// Authorised Use Instructions
    ///     - Contains a static property (_lastAttackTime) that MUST be called as a subsystem
    ///       registration at the start of runtime to prevent stale states while playtesting.
    /// </summary>
    public sealed class MeleeAttackActionInstance : GOAPActionInstance
    {
        // ───── Hashes ────────────────────────────────────────────────
        
        private static readonly int _attackTriggerHash = Animator.StringToHash("MeleeAttack");

        // ───── Private properties────────────────────────────────────────────────
        
        private readonly LayerMask _targetMask;
        
        private readonly float _damage;
        private readonly float _hitRadius;
        private readonly float _attackDuration;
        private readonly float _applyDamageNormalisedFrame;
        private readonly float _attackCooldown;

        private Collider[] _hits = new Collider[16];
        
        private float _attackTimer;
        private bool _damageApplied;

        private static float _lastAttackTime = float.NegativeInfinity;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _lastAttackTime = float.NegativeInfinity;
        }

        // ───── Implementation ────────────────────────────────────────────────
        
        public MeleeAttackActionInstance(GOAPAgent agent, GOAPActionData data, float damage, float hitRadius, LayerMask targetMask, float attackDuration, float applyDamageNormalisedFrame, float attackCooldown) : base(agent, data)
        {
            _damage = damage;
            _hitRadius = hitRadius;
             _targetMask = targetMask;
            _attackDuration = attackDuration;
            _applyDamageNormalisedFrame = applyDamageNormalisedFrame;
            _attackCooldown = attackCooldown;
        }

        public override bool CheckProceduralPreconditions()
        {
            // Attack cooldown
            if (Time.time - _lastAttackTime < _attackCooldown) return false;

            Transform target = Agent.Blackboard.Get<Transform>(BlackboardKeys.TARGET_TRANSFORM);
            if (target == null) { return false; }

            float distSqr = (target.position - Agent.transform.position).sqrMagnitude;

            return distSqr <= (_hitRadius * _hitRadius * 1.5f);
        }

        public override void OnStart()
        {
            Agent.NavAgent.ResetPath();
            Agent.Blackboard.Set(BlackboardKeys.MOVEMENT_SPEED, (int) MovementSpeed.Walk);

            FaceTarget();

            Agent.Animator.SetTrigger(_attackTriggerHash);

            _attackTimer  = 0.0f;
            _damageApplied = false;
        }

        public override ActionStatus Perform()
        {
            _attackTimer += Time.deltaTime;

            float normalisedTime = _attackTimer / _attackDuration;

            // Apply damage at the impact frame
            if (!_damageApplied && normalisedTime >= _applyDamageNormalisedFrame)
            {
                ApplyDamage();
                _damageApplied = true;
            }

            if (_attackTimer >= _attackDuration)
                return ActionStatus.Succeeded;

            return ActionStatus.Running;
        }

        public override void OnEnd() { }

        public override void OnReset()
        {
            _attackTimer   = 0.0f;
            _damageApplied = false;
        }

        // ─── Private helpers ──────────────────────────────────────────

        /// <summary>
        /// Rotates the agent to face its target before striking.
        /// </summary>
        private void FaceTarget()
        {
            Transform targetTransform = Agent.Blackboard.Get<Transform>(BlackboardKeys.TARGET_TRANSFORM);

            if (targetTransform == null) return;

            Vector3 direction =
                (targetTransform.position - Agent.transform.position)
                .normalized;

            direction.y = 0.0f;

            if (direction == Vector3.zero) return;

            Agent.transform.rotation =
                Quaternion.LookRotation(direction);
        }

        /// <summary>
        /// Physics overlap sphere hit detection at the impact frame.
        /// Only fires once per attack via _damageApplied flag.
        /// Uses a throttled overlap sphere rather than running every
        /// frame for performance.
        /// </summary>
        private void ApplyDamage()
        {
            Vector3 agentCenter = Agent.transform.position + Vector3.up * 0.9f;
            Vector3 sphereOrigin = agentCenter + Agent.transform.forward * 0.5f;

            int count = Physics.OverlapSphereNonAlloc(sphereOrigin, _hitRadius, _hits, _targetMask);
            
            for (int i = 0; i < count; i++)
            {
                if (_hits[i].transform == Agent.transform) { continue; }
                
                if (_hits[i].TryGetComponent(out IHealth health))
                {
                    health.TakeDamage(_damage);
                    
                    if (health.IsDead)
                        Agent.Blackboard.Set(BlackboardKeys.TARGET_IS_DEAD, true);
                }
            }

            _lastAttackTime = Time.time;
        }
    }
}

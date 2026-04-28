using UnityEngine;

namespace GOAP
{
    [CreateAssetMenu(fileName = "ACTION_Melee", menuName = "GOAP/Actions/Melee")]
    public sealed class MeleeAttackActionData : GOAPActionData
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("Melee Settings")]
        [SerializeField] private LayerMask  _targetMask;
        [SerializeField] [Min(0.0f)] private float _damage = 25.0f;
        [SerializeField] [Min(0.0f)] private float _hitRadius = 1.5f;
        [SerializeField] [Min(0.0f)] private float _applyDamageNormalisedFrame = 0.4f;
        [SerializeField] [Min(0.0f)] private float _attackCooldown = 1.2f;
        
        [Space(10.0f)]
        
        [Header("Melee Animation Settings")]
        [SerializeField] private AnimationClip[] _meleeClips;
        [SerializeField] [Min(0.0f)] private float _preAttackDelay = 0.0f;
        [SerializeField] [Min(0.0f)] private float _postAttackDelay = 0.0f;

        // ───── Implementation ────────────────────────────────────────────────
        
        public override GOAPActionInstance CreateInstance(GOAPAgent agent) => new MeleeAttackActionInstance(agent, this, _damage, _hitRadius, _targetMask, _applyDamageNormalisedFrame, _attackCooldown, _meleeClips, _preAttackDelay, _postAttackDelay);
    }

    /// <summary>
    /// Authorised Use Instructions
    ///     - Melee Animations MUST be dragged into this agent's AnimatorController by their
    ///       DEFAULT names, or the state hashes will be inaccurate.
    /// </summary>
    public sealed class MeleeAttackActionInstance : GOAPActionInstance
    {
        // ───── Private properties────────────────────────────────────────────────
        
        private readonly LayerMask _targetMask;
        
        private readonly AnimationClip[] _meleeClips;
        private readonly int[] _meleeStateHashes;
            
        private readonly float _damage;
        private readonly float _hitRadius;
        private readonly float _applyDamageNormalisedFrame;
        private readonly float _attackCooldown;
        private readonly float _preAttackDelay;
        private readonly float _postAttackDelay;

        private Collider[] _hits = new Collider[16];
        
        private float _attackTimer;
        private float _attackDuration;
        private float _damageTime;
        
        private bool _damageApplied;

        private static float _lastAttackTime = float.NegativeInfinity;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _lastAttackTime = float.NegativeInfinity;
        }

        // ───── Implementation ────────────────────────────────────────────────
        
        public MeleeAttackActionInstance(GOAPAgent agent, GOAPActionData data, float damage, float hitRadius, LayerMask targetMask, float applyDamageNormalisedFrame, float attackCooldown, AnimationClip[] meleeClips, float preAttackDelay, float postAttackDelay) : base(agent, data)
        {
            _damage = damage;
            _hitRadius = hitRadius;
             _targetMask = targetMask;
            _applyDamageNormalisedFrame = applyDamageNormalisedFrame;
            _attackCooldown = attackCooldown;
            _meleeClips = meleeClips;
            _preAttackDelay = preAttackDelay;
            _postAttackDelay = postAttackDelay;
            
            _meleeStateHashes = new int[meleeClips.Length];
            for (int i = 0; i < meleeClips.Length; i++)
                _meleeStateHashes[i] = Animator.StringToHash(meleeClips[i].name);
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
            Agent.NavAgent.velocity = Vector3.zero;
            Agent.NavAgent.isStopped = true;
            Agent.Blackboard.Set(BlackboardKeys.MOVEMENT_SPEED, (int) MovementSpeed.Walk);
            FaceTarget();

            // Choose a melee animation
            int index = Random.Range(0, _meleeClips.Length);
            AnimationClip chosenClip = _meleeClips[index];
            
            Agent.Animator.Play(_meleeStateHashes[index], 0, 0.0f);
            
            _attackDuration = _preAttackDelay + chosenClip.length + _postAttackDelay;
            _damageTime = _preAttackDelay + chosenClip.length * _applyDamageNormalisedFrame;

            _attackTimer  = 0.0f;
            _damageApplied = false;
        }

        public override ActionStatus Perform()
        {
            _attackTimer += Time.deltaTime;
            
            // Apply damage at the impact frame
            if (!_damageApplied && _attackTimer >= _damageTime)
            {
                ApplyDamage();
                _damageApplied = true;
            }

            return _attackTimer >= _attackDuration ? ActionStatus.Succeeded : ActionStatus.Running;
        }

        public override void OnEnd()
        {
            Agent.NavAgent.isStopped = false;
        }

        public override void OnReset()
        {
            _attackTimer   = 0.0f;
            _damageApplied = false;
            Agent.NavAgent.isStopped = false;
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

using UnityEngine;
using UnityEngine.AI;

namespace GOAP
{
    /// <summary>
    /// Drives Animator parameters from GOAPAgent information - e.g. Speed from velocity.
    /// 
    /// Authorised Use Instructions:
    ///     - Must be placed on a GameObject WITH BOTH an Animator and NavMeshAgent. This may involve
    ///       changing the locations of these components in the hierarchy
    ///     - At present, this class can write to the NavMeshAgent component to change speed. This was done due to the
    ///       closely-connected nature of Animator speed and real speed. In future, could refactor this to separate the read/
    ///       write calls. Be CAUTIOUS of unchecked writing to the NavMeshAgent.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public sealed class AgentAnimator : MonoBehaviour
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("Movement Speeds")]
        [SerializeField] [Range(0.1f, 10.0f)] private float _walkSpeed = 2.0f;
        [SerializeField] [Range(0.1f, 10.0f)] private float _jogSpeed = 4.0f;
        [SerializeField] [Range(0.1f, 10.0f)] private float _sprintSpeed = 6.0f;

        [Space(10.0f)]

        [Header("Blending")]
        [SerializeField] [Range(0.01f, 1.0f)] private float _speedDampTime = 0.1f; // lower = smoother
        [SerializeField] [Range(0.01f, 1.0f)] private float _speedSmoothHalfLife = 0.1f;

        // ───── Private properties ────────────────────────────────────────────────
        
        private Animator _animator;
        private NavMeshAgent _navAgent;
        private GOAPAgent _goapAgent;

        // ───── Public properties ────────────────────────────────────────────────
        
        public float NormalizedSpeed { get; private set; }

        // ───── Hashes────────────────────────────────────────────────
        
        private static readonly int _speedHash = Animator.StringToHash("Speed");

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _navAgent = GetComponentInParent<NavMeshAgent>();
            _goapAgent = GetComponentInParent<GOAPAgent>();

            Debug.Assert(_animator != null, "[GOAPAgentAnimator] No Animator component found on this GameObject.", this);
            Debug.Assert(_navAgent != null, "[GOAPAgentAnimator] No NavMeshAgent component found in parent GameObject.", this);
            Debug.Assert(_goapAgent != null, "[GOAPAgentAnimator] No GOAPAgent component found in parent GameObject.", this);
        }
        
        private void Start()
        {
            _animator?.Rebind();
            _animator?.Update(0.0f);
            _navAgent.speed = _walkSpeed;
            _goapAgent.Blackboard.Set(BlackboardKeys.MOVEMENT_SPEED, (int) MovementSpeed.Walk); 
        }

        private void Update()
        {
            UpdateMovementSpeed();
            UpdateAnimator();
        }

        // -----Private methods-----
        private void UpdateMovementSpeed()
        {
            MovementSpeed speedState = _goapAgent.Blackboard.Get<MovementSpeed>(BlackboardKeys.MOVEMENT_SPEED);
            float targetSpeed = speedState switch
            {
                MovementSpeed.Walk => _walkSpeed,
                MovementSpeed.Jog => _jogSpeed,
                MovementSpeed.Sprint => _sprintSpeed,
                _ => _walkSpeed
            };

            _navAgent.speed = Mathf.Lerp(_navAgent.speed, targetSpeed, 1.0f - Mathf.Pow(_speedSmoothHalfLife, Time.deltaTime));
        }

        private void UpdateAnimator()
        {
            if (_sprintSpeed <= 0.0f) { return; }

            NormalizedSpeed = Mathf.Clamp01(_navAgent.velocity.magnitude / _sprintSpeed);

            // Speed
            _animator.SetFloat(_speedHash, NormalizedSpeed, _speedDampTime, Time.deltaTime);
        }
    }
}


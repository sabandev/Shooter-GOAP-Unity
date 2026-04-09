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
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(GOAPAgent))]
    public sealed class GOAPAgentAnimator : MonoBehaviour
    {
        // -----Serialized properties-----
        [SerializeField] [Range(0.1f, 10.0f)] private float _walkSpeed = 2.0f;
        [SerializeField] [Range(0.1f, 10.0f)] private float _jogSpeed = 4.0f;
        [SerializeField] [Range(0.1f, 10.0f)] private float _sprintSpeed = 6.0f;
        [SerializeField] [Range(1.0f, 20.0f)] private float _speedTransitionRate = 8.0f;
        [SerializeField] [Range(0.01f, 1.0f)] private float _dampTime = 0.1f; // lower = smoother

        // -----Private properties-----
        private Animator _animator;
        private NavMeshAgent _navAgent;
        private GOAPAgent _goapAgent;

        // -----Hashes-----
        private static readonly int _speedHash = Animator.StringToHash("Speed");

        // -----Lifecycle methods-----
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _navAgent = GetComponent<NavMeshAgent>();
            _goapAgent = GetComponent<GOAPAgent>();

            Debug.Assert(_animator != null, "[GOAPAgentAnimator] No Animator component found.", this);
            Debug.Assert(_navAgent != null, "[GOAPAgentAnimator] No NavMeshAgent component found.", this);
            Debug.Assert(_goapAgent != null, "[GOAPAgentAnimator] No GOAPAgent component found.", this);

            // Set walk as default speed
            _navAgent.speed = _walkSpeed;
            _goapAgent.Blackboard.Set(BlackboardKeys.MOVEMENT_SPEED, MovementSpeed.Walk); // MOVE TO START IF ENCOUNTERING NULLREFEX ERRORS.
        }

        private void Update()
        {
            UpdateMovementSpeed();
            UpdateLocomotion();
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

            _navAgent.speed = Mathf.Lerp(_navAgent.speed, targetSpeed, _speedTransitionRate * Time.deltaTime);
        }

        private void UpdateLocomotion()
        {
            if (_sprintSpeed <= 0.0f) { return; }

            float normalizedSpeed = Mathf.Clamp01(_navAgent.velocity.magnitude / _sprintSpeed);

            _animator.SetFloat(_speedHash, normalizedSpeed, _dampTime, Time.deltaTime);
        }
    }
}


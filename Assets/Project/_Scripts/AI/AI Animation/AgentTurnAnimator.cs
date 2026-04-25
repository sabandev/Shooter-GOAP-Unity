using UnityEngine;
using UnityEngine.AI;

namespace GOAP
{
    /// <summary>
    /// Manages rotation handoff between NavMeshAgent and animation root motion for in-place turn animations.
    /// Allows turning animations to drive agent rotation and prevents sliding.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AgentAnimator))]
    public sealed class AgentTurnAnimator : MonoBehaviour
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("Turn Detection")]
        [SerializeField] [Range(10.0f, 90.0f)] private float _turnTriggerAngle = 50.0f;
        [SerializeField] private float _maxSpeedToTurn = 0.15f;
        [SerializeField] [Range(0.05f, 0.5f)] private float _turnCheckInterval = 0.1f;

        [Space(10.0f)]

        [Header("Root Motion")]
        [SerializeField] [Range(0.0f, 1.0f)] private float _rootMotionWeight = 1.0f;

        // ───── Private properties ────────────────────────────────────────────────
        
        private Animator        _animator;
        private NavMeshAgent    _navAgent;
        private AgentAnimator _agentAnimator;

        private bool  _isTurning;
        private float _turnCheckTimer;

        // ───── Hashes ────────────────────────────────────────────────
        
        private static readonly int _turnLeftHash  = Animator.StringToHash("TurnLeft");
        private static readonly int _turnRightHash = Animator.StringToHash("TurnRight");
        private static readonly int _isTurningHash = Animator.StringToHash("IsTurning");

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _navAgent = GetComponentInParent<NavMeshAgent>();
            _agentAnimator = GetComponent<AgentAnimator>();

            Debug.Assert(_animator != null, "[TurnController] Missing Animator.", this);
            Debug.Assert(_navAgent != null, "[TurnController] Missing NavMeshAgent.", this);
            Debug.Assert(_agentAnimator != null, "[TurnController] Missing GOAPAgentAnimator.", this);
        }

        private void Update()
        {
            if (_isTurning)
            {
                TickTurnInProgress();
            }
            else
            {
                TickTurnDetection();
            }
        }

        /// <summary>
        /// Called by Unity when root motion is applied.
        /// We use this to apply the animation's rotation to the
        /// agent while turning, ignoring the position delta.
        /// </summary>
        private void OnAnimatorMove()
        {
            if (!_isTurning) return;

            Quaternion rootRotation = Quaternion.Slerp(_navAgent.transform.rotation, _navAgent.transform.rotation * _animator.deltaRotation, _rootMotionWeight);

            _navAgent.transform.rotation = rootRotation;

            _navAgent.nextPosition = _navAgent.transform.position;
        }

        // ───── Private methods ────────────────────────────────────────────────
        
        private void TickTurnDetection()
        {
            _turnCheckTimer -= Time.deltaTime;

            if (_turnCheckTimer > 0.0f) { return; }

            _turnCheckTimer = _turnCheckInterval;

            // Only turn in place when nearly stationary
            if (_agentAnimator.NormalizedSpeed > _maxSpeedToTurn) { return; }

            // No path — nothing to turn toward
            if (!_navAgent.hasPath) { return; }

            Vector3 toDestination = (_navAgent.steeringTarget - _navAgent.transform.position).normalized;

            toDestination.y = 0.0f;

            if (toDestination == Vector3.zero) { return; }

            float angle = Vector3.SignedAngle(_navAgent.transform.forward, toDestination, Vector3.up);

            // Only trigger if angle exceeds threshold
            if (Mathf.Abs(angle) < _turnTriggerAngle) { return; }

            StartTurn(angle);
        }

        private void TickTurnInProgress()
        {
            // Check how far we still need to turn
            if (!_navAgent.hasPath)
            {
                EndTurn();
                return;
            }

            Vector3 toDestination = (_navAgent.steeringTarget - _navAgent.transform.position).normalized;

            toDestination.y = 0.0f;

            if (toDestination == Vector3.zero)
            {
                EndTurn();
                return;
            }

            float remainingAngle = Vector3.SignedAngle(_navAgent.transform.forward, toDestination, Vector3.up);

            // Use a smaller threshold than the trigger to prevent flickering
            if (Mathf.Abs(remainingAngle) < _turnTriggerAngle * 0.3f)
                EndTurn();
        }

        private void StartTurn(float angle)
        {
            _isTurning   = true;

            _navAgent.updateRotation = false;
            _navAgent.isStopped = true;

            _animator.SetBool(_isTurningHash, true);

            if (angle < 0.0f)
                _animator.SetTrigger(_turnLeftHash);
            else
                _animator.SetTrigger(_turnRightHash);
        }

        private void EndTurn()
        {
            _isTurning = false;

            _navAgent.updateRotation = true;
            _navAgent.isStopped = false;

            _animator.SetBool(_isTurningHash, false);
        }
    }
}
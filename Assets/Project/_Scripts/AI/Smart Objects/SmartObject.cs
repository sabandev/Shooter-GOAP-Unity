using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Marks a GameObject as an interactable SmartObject for use by GOAP Agents
    /// </summary>
    public sealed class SmartObject : MonoBehaviour
    {
        // ----- -----

        public enum ReservationState
        {
            Available,
            Reserved,
            InUse,
            Cooldown,
            PermanentlyUnavailable    // e.g. consumed pickups, destroyed objects
        }

        // -----Serialized properties-----
        [SerializeField] private SmartObjectData _data;
        [SerializeField] private Transform _interactionPointOverride;

        [Space(10.0f)]

        [Header("Debug")]
        [SerializeField] private bool _drawGizmos = true;

        // -----Private properties-----
        private ReservationState _reservationState = ReservationState.Available;
        private GOAPAgent _reservingAgent;
        private float _cooldownTimer;

        // -----Public properties-----
        public SmartObjectData Data => _data;
        public ReservationState State => _reservationState;
        public GOAPAgent ReservingAgent => _reservingAgent;

        /// <summary>
        /// World position where the agent should stand to interact.
        /// </summary>
        public Vector3 InteractionPoint
        {
            get
            {
                if (_interactionPointOverride != null)
                    return _interactionPointOverride.position;

                return transform.position +
                       transform.TransformDirection(_data != null ? _data.InteractionOffset : Vector3.forward * -1.0f);
            }
        }

        public bool IsAvailable =>
            _reservationState == ReservationState.Available && (_data == null || _data.AllowsSimultaneousUse ||  _reservingAgent == null);

        // -----Lifecycle methods-----

        private void Awake()
        {
            Debug.Assert(_data != null,
                $"[SmartObject] '{name}' has no SmartObjectData assigned.", this);
        }

        private void OnEnable()
        {
            SmartObjectRegistry.Instance?.Register(this);
        }

        private void OnDisable()
        {
            SmartObjectRegistry.Instance?.Unregister(this);
        }

        private void Update()
        {
            if (_reservationState != ReservationState.Cooldown) return;

            _cooldownTimer -= Time.deltaTime;

            if (_cooldownTimer <= 0.0f)
            {
                _reservationState = ReservationState.Available;
                _reservingAgent   = null;
            }
        }

        // -----Public methods-----

        /// <summary>
        /// Claims this object for the given agent.
        /// Returns false if already claimed by another agent.
        /// </summary>
        public bool TryReserve(GOAPAgent agent)
        {
            if (!IsAvailable) { return false; }

            _reservationState = ReservationState.Reserved;
            _reservingAgent   = agent;
            return true;
        }

        /// <summary>
        /// Transitions from Reserved to InUse when the agent
        /// arrives at the interaction point.
        /// </summary>
        public void BeginInteraction(GOAPAgent agent)
        {
            if (_reservingAgent != agent) { return; }

            _reservationState = ReservationState.InUse;
        }

        /// <summary>
        /// Called when the interaction completes successfully.
        /// Starts cooldown if configured, otherwise returns to Available.
        /// </summary>
        public void CompleteInteraction(GOAPAgent agent)
        {
            if (_reservingAgent != agent) return;

            if (_data != null && _data.CooldownDuration > 0.0f)
            {
                _reservationState = ReservationState.Cooldown;
                _cooldownTimer    = _data.CooldownDuration;
            }
            else
            {
                _reservationState = ReservationState.Available;
                _reservingAgent   = null;
            }
        }

        /// <summary>
        /// Releases the reservation — called when an agent's plan
        /// is aborted before the interaction completes.
        /// </summary>
        public void ReleaseReservation(GOAPAgent agent)
        {
            if (_reservingAgent != agent) return;

            _reservationState = ReservationState.Available;
            _reservingAgent   = null;
        }

        public void SetPermanentlyUnavailable()
        {
            _reservationState = ReservationState.PermanentlyUnavailable;
            _reservingAgent   = null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_drawGizmos || _data == null) return;

            Color colour = _reservationState switch
            {
                ReservationState.Available => new Color(0.0f, 1.0f, 0.0f, 0.4f),
                ReservationState.Reserved => new Color(1.0f, 1.0f, 0.0f, 0.4f),
                ReservationState.InUse => new Color(1.0f, 0.5f, 0.0f, 0.4f),
                ReservationState.Cooldown => new Color(0.5f, 0.5f, 0.5f, 0.4f),
                _ => new Color(1.0f, 0.0f, 0.0f, 0.4f)
            };

            Gizmos.color = colour;
            Gizmos.DrawWireSphere(InteractionPoint, _data.InteractionRange);
            Gizmos.DrawLine(transform.position, InteractionPoint);
            Gizmos.DrawWireSphere(InteractionPoint, 0.1f);
        }
#endif
    }
}
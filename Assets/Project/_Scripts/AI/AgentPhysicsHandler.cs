using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Applies physics forces to rigidbodies the agent walks through.
    /// </summary>
    public sealed class AgentPhysicsHandler : MonoBehaviour
    {
        // -----Serialized properties-----

        [Header("Push Settings")]
        [SerializeField] private float _pushForce = 4.0f;
        [SerializeField] private float _speedForceMultiplier = 1.5f;
        [SerializeField] private float _maxForce = 8.0f;
        [SerializeField] private float _maxPushMass = 10.0f;

        [Space(10.0f)]

        [Header("Detection")]
        [SerializeField] private float _pushRadius = 0.5f;
        [SerializeField] private float[] _pushHeights = { 0.3f, 0.9f, 1.5f };
        [SerializeField] private LayerMask _pushableLayers;
        [SerializeField] [Range(0.016f, 1.0f)] private float _pushCheckInterval = 0.05f; // 60Hz - 1Hz

        [Space(10.0f)]

        [Header("Gizmos")]
        [SerializeField] private bool _drawPushDetectionGizmos = true;

        // -----Private properties-----
        private AgentAnimator _agentAnimator;
        private float _pushCheckTimer;

        // -----Lifecycle methods-----

        private void Awake()
        {
            _agentAnimator = GetComponent<AgentAnimator>();
        }

        private void Update()
        {
            if (_agentAnimator?.NormalizedSpeed < 0.1f) { return; }

            _pushCheckTimer -= Time.deltaTime;
            if (_pushCheckTimer > 0.0f) { return; }
            _pushCheckTimer = _pushCheckInterval;

            CheckAndPush();
        }

        // -----Private methods-----

        /// <summary>
        /// Checks all configured heights for overlapping pushable
        /// rigidbodies and applies force to each one found.
        /// </summary>
        private void CheckAndPush()
        {
            foreach (float height in _pushHeights)
            {
                Vector3 origin = transform.position + Vector3.up * height;

                Collider[] hits = Physics.OverlapSphere(origin, _pushRadius, _pushableLayers);

                foreach (Collider hit in hits)
                {
                    if (hit.attachedRigidbody == null)              continue;
                    if (hit.attachedRigidbody.mass > _maxPushMass)  continue;

                    ApplyPushForce(hit.attachedRigidbody, origin);
                }
            }
        }

        private void ApplyPushForce(Rigidbody rb, Vector3 origin)
        {
            rb.WakeUp();

            Vector3 pushDirection = rb.position - origin;

            // Slight upward component prevents objects being pushed into the floor
            pushDirection.Normalize();

            float speedMultiplier = _agentAnimator != null ? 1.0f + (_agentAnimator.NormalizedSpeed * _speedForceMultiplier) : 1.0f;

            float finalForce = Mathf.Min(_pushForce * speedMultiplier, _maxForce);

            rb.AddForce(pushDirection * finalForce, ForceMode.Impulse);
        }

        // -----Editor helper methods-----

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_pushHeights == null || !_drawPushDetectionGizmos) { return; }

            foreach (float height in _pushHeights)
            {
                Vector3 origin = transform.position + Vector3.up * height;

                Gizmos.color = new Color(1.0f, 0.5f, 0.0f, 0.3f);
                Gizmos.DrawWireSphere(origin, _pushRadius);
            }
        }
#endif
    }
}
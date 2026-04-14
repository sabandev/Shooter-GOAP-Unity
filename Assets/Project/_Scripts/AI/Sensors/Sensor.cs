using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Base class for agent sensors. Holds common properties (tick rate, GOAPAgent reference) and
    /// common methods - Sense().
    /// 
    /// Authorised Use Instructions:
    ///     - MUST respect individual tick rate and decouple sensing from the Unity loop.
    ///       If sensors run at 60Hz this can be overwhelming for builds and destroy performance. Run
    ///       at 5-10Hz for safety.
    ///     - All editor helper methods (DrawGizmos) must call GetAgent() if they are to be run while the
    ///       application is not playing. Two solutions to null reference exceptions:
    ///         - Add !Application.isPlaying { return; } at the top to prevent gizmos from rendering while
    ///           not in Play Mode. This stop the calls to an Agent that is not referenced yet.
    ///         - Add GetAgent() and a null check { return; } to ensure an Agent reference is obtained,
    ///           or the method won't execute.
    /// </summary>
    public abstract class Sensor : MonoBehaviour
    {
        // -----Serialized properties-----
        [Header("Sensor")]
        [SerializeField] [Range(1.0f, 30.0f)] private float _tickRate = 5.0f;

        // -----Protected properties-----
        protected GOAPAgent Agent { get; private set; }

        // -----Private properties-----
        private float _tickTimer;

        // -----Lifecycle methods-----
        protected virtual void Awake()
        {
            GetAgent();
        }

        private void Update()
        {
            _tickTimer -= Time.deltaTime;

            if (_tickTimer > 0.0f) { return; }

            _tickTimer = 1.0f / _tickRate;

            Sense();
        }

        // -----Implementation-----
        protected abstract void Sense();
        
        protected virtual void GetAgent()
        {
            Agent = GetComponentInParent<GOAPAgent>();
            Debug.Assert(Agent != null, $"[{GetType().Name}] Requires GOAPAgent on the parent GameObject.", this);
        }
    }
}


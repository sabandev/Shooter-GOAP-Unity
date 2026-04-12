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
            Agent = GetComponent<GOAPAgent>();
            Debug.Assert(Agent != null, $"[{GetType().Name}] Requires GOAPAgent on the same GameObject.", this);
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
    }
}


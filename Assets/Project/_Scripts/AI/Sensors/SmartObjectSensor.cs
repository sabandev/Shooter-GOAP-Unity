using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Sensor that queries SmartObjectRegistry each tick and writes
    /// relevant availability facts to the agent's blackboard.
    ///
    /// Each sensor instance monitors one object tag. Add multiple
    /// sensors to an agent to monitor multiple object types.
    ///
    /// Written facts:
    ///   {Tag}_AVAILABLE  (bool)      — any available object of this type exists
    ///   {Tag}_TRANSFORM  (Transform) — nearest available object's transform
    ///   {Tag}_DISTANCE   (float)     — distance to nearest available object
    ///
    /// Example: tag "Pickup" produces keys:
    ///   PICKUP_AVAILABLE, PICKUP_TRANSFORM, PICKUP_DISTANCE
    /// </summary>
    public sealed class SmartObjectSensor : Sensor
    {
        // ─── Serialized fields ────────────────────────────────────────

        [Header("Smart Object Sensor")]
        [Tooltip("The SmartObjectData tag to monitor. Must match " +
                 "SmartObjectData.ObjectTag exactly.")]
        [SerializeField] private string _targetTag = "";

        [Tooltip("Maximum distance to consider objects. Objects " +
                 "beyond this range are ignored.")]
        [SerializeField] private float _detectionRange = 20.0f;

        // ─── Cached blackboard keys ───────────────────────────────────

        private string _availableKey;
        private string _transformKey;
        private string _distanceKey;

        // ─── SensorBase implementation ────────────────────────────────

        protected override void Awake()
        {
            base.Awake();

            // Build blackboard keys from tag at init time
            // so we're not doing string concatenation every tick
            string tag      = _targetTag.ToUpper();
            _availableKey  = $"{tag}_AVAILABLE";
            _transformKey  = $"{tag}_TRANSFORM";
            _distanceKey   = $"{tag}_DISTANCE";
        }

        protected override void Sense()
        {
            if (string.IsNullOrEmpty(_targetTag)) return;

            SmartObject nearest = SmartObjectRegistry.Instance.FindNearestForAgent(_targetTag, transform.position, Agent);

            if (nearest == null)
            {
                Agent.Blackboard.Set(_availableKey, false);
                return;
            }

            float dist = Vector3.Distance(
                transform.position,
                nearest.transform.position);

            if (dist > _detectionRange)
            {
                Agent.Blackboard.Set(_availableKey, false);
                return;
            }

            Agent.Blackboard.Set(_availableKey, true);
            Agent.Blackboard.Set(_transformKey,  nearest.transform);
            Agent.Blackboard.Set(_distanceKey,   dist);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Singleton registry of all active SmartObjects in the scene.
    ///
    /// Authorised Use Instructions:
    ///     - MUST use RuntimeInitializeOnLoadMethod to reset static state
    ///       between play sessions when domain reload is disabled.
    /// </summary>
    public sealed class SmartObjectRegistry
    {
        // -----Singleton-----

        private static SmartObjectRegistry _instance;

        public static SmartObjectRegistry Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SmartObjectRegistry();
                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _instance = null;
        }

        // -----Private properties -----
        private readonly Dictionary<string, List<SmartObject>> _registry = new Dictionary<string, List<SmartObject>>();

        // -----Public methods-----

        /// <summary>
        /// Registers a SmartObject. Called by SmartObject.OnEnable.
        /// </summary>
        public void Register(SmartObject smartObject)
        {
            if (smartObject?.Data == null) { return; }

            string tag = smartObject.Data.ObjectTag;

            if (!_registry.TryGetValue(tag, out List<SmartObject> list))
            {
                list = new List<SmartObject>();
                _registry[tag] = list;
            }

            if (!list.Contains(smartObject))
                list.Add(smartObject);
        }

        /// <summary>
        /// Unregisters a SmartObject. Called by SmartObject.OnDisable.
        /// </summary>
        public void Unregister(SmartObject smartObject)
        {
            if (smartObject?.Data == null) { return; }

            string tag = smartObject.Data.ObjectTag;

            if (_registry.TryGetValue(tag, out List<SmartObject> list))
                list.Remove(smartObject);
        }

        /// <summary>
        /// Returns the nearest available SmartObject with the given tag
        /// to the given world position. Returns null if none found.
        /// </summary>
        public SmartObject FindNearest(string tag, Vector3 position)
        {
            if (!_registry.TryGetValue(tag, out List<SmartObject> list))
                return null;

            SmartObject nearest    = null;
            float       nearestSqr = float.MaxValue;

            foreach (SmartObject obj in list)
            {
                if (obj == null || !obj.IsAvailable) continue;

                float distSqr = (obj.transform.position - position)
                                .sqrMagnitude;

                if (distSqr < nearestSqr)
                {
                    nearestSqr = distSqr;
                    nearest    = obj;
                }
            }

            return nearest;
        }

        public SmartObject FindNearestForAgent(string tag, Vector3 position, GOAPAgent agent)
        {
            if (!_registry.TryGetValue(tag, out List<SmartObject> list)) { return null; }

            SmartObject nearest = null;
            float nearestSqr = float.MaxValue;

            foreach (SmartObject obj in list)
            {
                if (obj == null) { continue; }

                bool usable = obj.IsAvailable || obj.ReservingAgent == agent;

                if (!usable) { continue; }

                if (obj.State == SmartObject.ReservationState.PermanentlyUnavailable) { continue; }

                float distSqr = (obj.transform.position - position).sqrMagnitude;

                if (distSqr < nearestSqr)
                {
                    nearestSqr = distSqr;
                    nearest = obj;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Returns all available SmartObjects with the given tag.
        /// Returns empty list if none found.
        /// </summary>
        public List<SmartObject> FindAll(string tag)
        {
            if (!_registry.TryGetValue(tag, out List<SmartObject> list))
                return new List<SmartObject>();

            var available = new List<SmartObject>();

            foreach (SmartObject obj in list)
            {
                if (obj != null && obj.IsAvailable)
                    available.Add(obj);
            }

            return available;
        }

        /// <summary>
        /// Returns true if any available SmartObject with the given
        /// tag exists within range of the given position.
        /// </summary>
        public bool AnyAvailableInRange(
            string tag, Vector3 position, float range)
        {
            if (!_registry.TryGetValue(tag, out List<SmartObject> list))
                return false;

            float rangeSqr = range * range;

            foreach (SmartObject obj in list)
            {
                if (obj == null || !obj.IsAvailable) continue;

                if ((obj.transform.position - position).sqrMagnitude
                    <= rangeSqr)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if any SmartObject with the given tag exists
        /// in the scene regardless of availability.
        /// </summary>
        public bool AnyExists(string tag)
        {
            return _registry.TryGetValue(tag, out List<SmartObject> list)
                   && list.Count > 0;
        }

        /// <summary>
        /// Returns the count of available objects with the given tag.
        /// </summary>
        public int CountAvailable(string tag)
        {
            if (!_registry.TryGetValue(tag, out List<SmartObject> list))
                return 0;

            int count = 0;
            foreach (SmartObject obj in list)
                if (obj != null && obj.IsAvailable)
                    count++;

            return count;
        }
    }
}
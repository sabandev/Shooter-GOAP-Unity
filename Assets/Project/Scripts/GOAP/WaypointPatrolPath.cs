using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Defines an ordered sequence of waypoint Transforms for the AI patrol actions.
    /// 
    /// Authorised Use Instructions:
    ///     - Attach to an empty GameObject and assign waypoint Transforms in the inspector
    /// </summary>
    public sealed class WaypointPatrolPath : MonoBehaviour
    {
        // -----Serialized properties-----
        [SerializeField] private Transform[] _waypoints;
        [SerializeField] private float _gizmoSphereRadius = 0.3f;

        // -----Public properties-----
        public int Count => _waypoints?.Length ?? 0;

        // -----Public methods-----
        public Vector3 GetWaypoint(int index)
        {
            if (_waypoints == null || _waypoints.Length == 0)
            {
                Debug.LogWarning($"[WaypointPatrolPath] No waypoints assigned. AI cannot patrol this waypoint path. Assign waypoints in the inspector of {name} GameObject.", this);
                return transform.position;
            }

            return _waypoints[index % _waypoints.Length].position;
        }

        public Vector3 GetRandomWaypoint()
        {
            if (_waypoints == null || _waypoints.Length == 0) { return transform.position; }

            return _waypoints[Random.Range(0, _waypoints.Length)].position;
        }

        // -----Editor Helper-----
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_waypoints == null || _waypoints.Length == 0) { return; }

            Gizmos.color = Color.green;

            for (int i = 0; i < _waypoints.Length; i++)
            {
                if (_waypoints[i] == null) { continue; }

                Gizmos.DrawWireSphere(_waypoints[i].position, _gizmoSphereRadius);

                int next = (i + 1) % _waypoints.Length;

                if (_waypoints[next] != null)
                    Gizmos.DrawLine(_waypoints[i].position, _waypoints[next].position);
            }
        }
#endif
    }
}

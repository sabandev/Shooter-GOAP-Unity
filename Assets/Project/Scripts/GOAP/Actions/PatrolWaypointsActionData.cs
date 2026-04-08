using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// First-pass test.
/// Creating some basic actions to test the inital implementation of the GOAP system.
/// </summary>

namespace GOAP
{
    // -----PATROL WAYPOINTS-----
    [CreateAssetMenu(fileName = "ACTION_PatrolWaypoints", menuName = "GOAP/Actions/TEST0/PatrolWaypoints")]
    public sealed class PatrolWaypointsActionData : GOAPActionData
    {
        // -----Serialized properties-----
        [SerializeField] private float _arrivalTolerance = 0.5f;

        public override GOAPActionInstance CreateInstance(GOAPAgent agent) => new PatrolWaypointsActionInstance(agent, this, _arrivalTolerance);
    }

    public sealed class PatrolWaypointsActionInstance : GOAPActionInstance
    {
        // -----Private properties-----
        private readonly float _arrivalTolerance;
        private WaypointPatrolPath _waypointNetwork;
        private int _currentWaypointIndex;

        public PatrolWaypointsActionInstance(GOAPAgent agent, GOAPActionData data, float arrivalTolerance) : base(agent, data)
        {
            _arrivalTolerance = arrivalTolerance;
        }

        public override bool CheckProceduralPreconditions()
        {
            if (_waypointNetwork == null)
                _waypointNetwork = Agent.WaypointNetwork;

            if (_waypointNetwork == null)
            {
                Debug.LogWarning($"[PatrolWaypointActionInstance] No WaypointPatrolPath component found on '{Agent.name}'. Add one to use waypoint patrol action.", Agent);
                return false;
            }

            return _waypointNetwork.Count > 0;
        }

        public override void OnStart()
        {
            Vector3 destination = _waypointNetwork.GetWaypoint(_currentWaypointIndex);
            Agent.NavAgent.SetDestination(destination);
            Agent.Blackboard.Set(BlackboardKeys.IS_PATROLLING, true);
        }

        public override ActionStatus Perform()
        {
            if (Agent.NavAgent.pathPending) { return ActionStatus.Running; }
            if (Agent.NavAgent.pathStatus == NavMeshPathStatus.PathInvalid) { return ActionStatus.Failed; }
            if (Agent.NavAgent.remainingDistance > _arrivalTolerance) { return ActionStatus.Running; }

            _currentWaypointIndex++;
            return ActionStatus.Succeeded;
        }

        public override void OnEnd()
        {
            Agent.Blackboard.Set(BlackboardKeys.IS_PATROLLING, false);
        }

        /// <summary>
        /// NOTE: Can make a no-op if we want the current waypoint index to persist across plans (resuming waypoint behaviour)
        /// </summary>
        public override void OnReset()
        {
        }
    }
}
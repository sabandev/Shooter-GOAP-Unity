using UnityEngine;
using UnityEngine.AI;

namespace GOAP
{
    [CreateAssetMenu(fileName = "ACTION_PatrolRandom", menuName = "GOAP/Actions/PatrolRandom")]
    public sealed class PatrolRandomActionData : GOAPActionData
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [SerializeField] [Min(0.0f)] private float _wanderRadius = 10.0f;
        [SerializeField] [Min(0.0f)] private float _arrivalTolerance = 0.5f;

        // ───── Implementation ────────────────────────────────────────────────
        
        public override GOAPActionInstance CreateInstance(GOAPAgent agent) => new PatrolRandomActionInstance(agent, this, _wanderRadius, _arrivalTolerance);
    }

    public sealed class PatrolRandomActionInstance : GOAPActionInstance
    {
        // ───── Private properties ────────────────────────────────────────────────
        
        private readonly float _wanderRadius;
        private readonly float _arrivalTolerance;
        private bool _destinationSet;

        // ───── Implementation ────────────────────────────────────────────────
        
        public PatrolRandomActionInstance(GOAPAgent agent, GOAPActionData data, float wanderRadius, float arrivalTolerance) : base(agent, data)
        {
            _wanderRadius = wanderRadius;
            _arrivalTolerance = arrivalTolerance;
        }

        public override bool CheckProceduralPreconditions() => NavMesh.SamplePosition(RandomPointInRadius(), out _, _wanderRadius, NavMesh.AllAreas);

        public override void OnStart()
        {
            base.OnStart();
            
            if (NavMesh.SamplePosition(RandomPointInRadius(), out NavMeshHit hit, _wanderRadius, NavMesh.AllAreas))
            {
                Agent.NavAgent.SetDestination(hit.position);
                _destinationSet = true;
            }

            Agent.Blackboard.Set(BlackboardKeys.IS_PATROLLING, true);
        }

        public override ActionStatus Perform()
        {
            if (!_destinationSet) { return ActionStatus.Failed; }
            if (Agent.NavAgent.pathPending) { return ActionStatus.Running; }
            if (Agent.NavAgent.pathStatus == NavMeshPathStatus.PathInvalid) { return ActionStatus.Failed; }
            if (Agent.NavAgent.remainingDistance > _arrivalTolerance) { return ActionStatus.Running; }

            return ActionStatus.Succeeded;
        }

        public override void OnEnd()
        {
            Agent.Blackboard.Set(BlackboardKeys.IS_PATROLLING, false);
        }

        public override void OnReset()
        {
            _destinationSet = false;
        }

        private Vector3 RandomPointInRadius()
        {
            Vector2 circle = Random.insideUnitCircle * _wanderRadius;
            return Agent.transform.position + new Vector3(circle.x, 0.0f, circle.y);
        }
    }
}
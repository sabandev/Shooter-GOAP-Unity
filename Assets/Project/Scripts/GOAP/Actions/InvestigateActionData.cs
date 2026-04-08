using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// First-pass test.
/// Creating some basic actions to test the inital implementation of the GOAP system.
/// </summary>

namespace GOAP
{
    [CreateAssetMenu(fileName = "ACTION_Investigate", menuName = "GOAP/Actions/TEST0/Investigate")]
    public sealed class InvestigateActionData : GOAPActionData
    {
        [SerializeField] private float _arrivalTolerance = 1.5f;

        public override GOAPActionInstance CreateInstance(GOAPAgent agent) => new InvestigateActionInstance(agent, this, _arrivalTolerance);
    }

    public sealed class InvestigateActionInstance : GOAPActionInstance
    {
        private readonly float _arrivalTolerance;
        private bool _destinationSet;

        public InvestigateActionInstance(GOAPAgent agent, GOAPActionData data, float arrivalTolerance) : base(agent, data)
        {
            _arrivalTolerance = arrivalTolerance;
        }

        public override bool CheckProceduralPreconditions() => Agent.Blackboard.Contains(BlackboardKeys.TARGET_LAST_KNOWN_POS);

        public override void OnStart()
        {
            var destination = Agent.Blackboard.Get<Vector3>(BlackboardKeys.TARGET_LAST_KNOWN_POS);

            Agent.NavAgent.SetDestination(destination);
            Agent.Blackboard.Set(BlackboardKeys.IS_INVESTIGATING, true);
            _destinationSet = true;
        }

        public override ActionStatus Perform()
        {
            if (!_destinationSet) { return ActionStatus.Failed; }
            if (Agent.NavAgent.pathPending) { return ActionStatus.Running; }
            if (Agent.NavAgent.pathStatus == NavMeshPathStatus.PathInvalid) { return ActionStatus.Failed; }

            if (Agent.Blackboard.Contains(BlackboardKeys.TARGET_LAST_KNOWN_POS))
            {
                var updatedPos = Agent.Blackboard.Get<Vector3>(BlackboardKeys.TARGET_LAST_KNOWN_POS);
                Agent.NavAgent.SetDestination(updatedPos);
            }

            if (Agent.NavAgent.remainingDistance > _arrivalTolerance) { return ActionStatus.Running; }

            return ActionStatus.Succeeded;
        }

        public override void OnEnd()
        {
            Agent.Blackboard.Set(BlackboardKeys.IS_INVESTIGATING, false);
        }

        public override void OnReset()
        {
            _destinationSet = false;
        }
    }
}
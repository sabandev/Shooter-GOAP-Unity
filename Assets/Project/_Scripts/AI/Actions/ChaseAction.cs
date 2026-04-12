using UnityEngine;

namespace GOAP
{
    [CreateAssetMenu(fileName = "ACTION_Chase", menuName = "GOAP/Actions/Chase")]
    public sealed class ChaseActionData : GOAPActionData
    {
        // -----Serialized properties-----
        [SerializeField] private float _arrivalRange = 1.5f;
        [SerializeField] [Range(1.0f, 30.0f)] private float _destinationUpdateRate = 10.0f;

        public override GOAPActionInstance CreateInstance(GOAPAgent agent) => new ChaseActionInstance(agent, this, _arrivalRange, _destinationUpdateRate);
    }

    public sealed class ChaseActionInstance : GOAPActionInstance
    {
        // -----Private properties-----
        private readonly float _arrivalRange;
        private readonly float _destinationUpdateRate;

        private float _destinationUpdateTimer;

        // -----Implementation-----
        public ChaseActionInstance(GOAPAgent agent, GOAPActionData data, float arrivalRange, float destinationUpdateRate) : base(agent, data)
        {
            _arrivalRange = arrivalRange;
            _destinationUpdateRate = destinationUpdateRate;
        }

        public override bool CheckProceduralPreconditions()
        {
            return Agent.Blackboard.Get<bool>(BlackboardKeys.TARGET_VISIBLE);
        }

        public override void OnStart()
        {
            Agent.Blackboard.Set(BlackboardKeys.MOVEMENT_SPEED, MovementSpeed.Sprint);

            _destinationUpdateTimer = 0.0f;

            UpdateDestination();
        }

        public override ActionStatus Perform()
        {
            bool targetVisisble = Agent.Blackboard.Get<bool>(BlackboardKeys.TARGET_VISIBLE);
            if (!targetVisisble) { return ActionStatus.Failed; }

            if (Agent.Blackboard.Get<bool>(BlackboardKeys.TARGET_IN_MELEE_RANGE)) { return ActionStatus.Succeeded; }

            _destinationUpdateTimer -= Time.deltaTime;

            if (_destinationUpdateTimer <= 0.0f)
            {
                UpdateDestination();
                _destinationUpdateTimer = 1.0f / _destinationUpdateRate;
            }

            if (Agent.NavAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid) { return ActionStatus.Failed; }

            return ActionStatus.Running;
        }

        public override void OnEnd()
        {
            Agent.NavAgent.ResetPath();
            Agent.Blackboard.Set(BlackboardKeys.MOVEMENT_SPEED, MovementSpeed.Walk);
        }

        public override void OnReset()
        {
            _destinationUpdateTimer = 0.0f;

            Agent.Blackboard.Set(BlackboardKeys.MOVEMENT_SPEED, MovementSpeed.Walk);
        }

        // -----Private methods-----
        private void UpdateDestination()
        {
            Transform targetTransform = Agent.Blackboard.Get<Transform>(BlackboardKeys.TARGET_TRANSFORM);

            Vector3 destination = targetTransform != null ? targetTransform.position : Agent.Blackboard.Get<Vector3>(BlackboardKeys.TARGET_LAST_KNOWN_POS);

            Agent.NavAgent.SetDestination(destination);
        }
    }
}


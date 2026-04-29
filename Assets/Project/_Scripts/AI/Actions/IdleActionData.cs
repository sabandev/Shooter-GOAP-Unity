using UnityEngine;

namespace GOAP
{
    [CreateAssetMenu(fileName = "ACTION_Idle", menuName = "GOAP/Actions/Idle")]
    public sealed class IdleActionData : GOAPActionData
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [SerializeField] [Min(0.0f)] private float _idleDuration = 3.0f;

        // ───── Implementation ────────────────────────────────────────────────
        
        public override GOAPActionInstance CreateInstance(GOAPAgent agent) => new IdleActionInstance(agent, this, _idleDuration);
    }

    public sealed class IdleActionInstance : GOAPActionInstance
    {
        // ───── Private properties ────────────────────────────────────────────────
        
        private readonly float _idleDuration;
        private float _timer;

        // ───── Implementation ────────────────────────────────────────────────
        
        public IdleActionInstance(GOAPAgent agent, GOAPActionData data, float idleDuration) : base(agent, data)
        {
            _idleDuration = idleDuration;
        }

        public override bool CheckProceduralPreconditions() => true;

        public override void OnStart()
        {
            Agent.NavAgent.ResetPath();
            Agent.Blackboard.Set(BlackboardKeys.IS_IDLE, true);
            _timer = 0.0f;
        }

        public override ActionStatus Perform()
        {
            _timer += Time.deltaTime;

            return _timer >= _idleDuration ? ActionStatus.Succeeded : ActionStatus.Running;
        }

        public override void OnEnd()
        {
            Agent.Blackboard.Set(BlackboardKeys.IS_IDLE, false);
        }

        public override void OnReset()
        {
            _timer = 0.0f;
        }
    }
}
using UnityEngine;

namespace GOAP
{
    [CreateAssetMenu(fileName = "ACTION_GoToLastKnownPosition", menuName = "GOAP/Actions/Go To Last Known Position")]
    public sealed class GoToLastKnownPositionActionData : GOAPActionData
    {
        // ───── Serialized propertie ────────────────────────────────────────────────
        
        [SerializeField] [Min(0.1f)] private float _arrivalDistance = 1.2f;                                                                                                                                                     
        [SerializeField] [Min(1.0f)] private float _timeout = 12.0f;                                                                                                                                                    
   
        public override GOAPActionInstance CreateInstance(GOAPAgent agent) => new GoToLastKnownPositionActionInstance(agent, this, _arrivalDistance, _timeout);
    }
    
    public sealed class GoToLastKnownPositionActionInstance : GOAPActionInstance
    {
        // ───── Private propertie ────────────────────────────────────────────────
        
        private readonly float _arrivalDistance;
        private readonly float _timeout;
        
        private float _timer;

        // ───── Implementation ────────────────────────────────────────────────
        
        public GoToLastKnownPositionActionInstance(GOAPAgent agent, GOAPActionData data, float arrivalDistance, float timeout) : base(agent, data)                                                                                                                                                           
        {       
            _arrivalDistance = arrivalDistance;
            _timeout = timeout;                                                                                                                                                                                         
        }
                                                                                                                                                                                                                              
        public override bool CheckProceduralPreconditions() => Agent.Blackboard.Get<bool>(BlackboardKeys.IS_INVESTIGATING) && Agent.Blackboard.Contains(BlackboardKeys.TARGET_LAST_KNOWN_POS);                                                                                                                                                    

        public override void OnStart()                                                                                                                                                                                          
        {       
            base.OnStart();
            
            _timer = 0;
            Agent.NavAgent.SetDestination(Agent.Blackboard.Get<Vector3>(BlackboardKeys.TARGET_LAST_KNOWN_POS));                                                                                                                 
        }
                                                                                                                                                                                                                              
        public override ActionStatus Perform()                                                                                                                                                                                  
        {
            _timer += Time.deltaTime;                                                                                                                                                                                           
            if (_timer >= _timeout) return ActionStatus.Failed;

            if (!Agent.NavAgent.pathPending && Agent.NavAgent.remainingDistance <= _arrivalDistance)
            {
              Agent.Blackboard.Set(BlackboardKeys.AT_INVESTIGATION_POINT, true);
              return ActionStatus.Succeeded;                                                                                                                                                                                  
            }
                                                                                                                                                                                                                              
            return ActionStatus.Running;
        }

        public override void OnEnd()   { }
        public override void OnReset() => _timer = 0;
    }
}
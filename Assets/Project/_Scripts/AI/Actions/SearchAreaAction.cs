using UnityEngine;
using UnityEngine.AI;

namespace GOAP
{
    [CreateAssetMenu(fileName = "ACTION_SearchArea", menuName = "GOAP/Actions/Search Area")]
    public sealed class SearchAreaActionData : GOAPActionData 
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [SerializeField] [Min(1)] private int _searchPointCount = 3;                                                                                                                                                       
        [SerializeField] [Min(1.0f)] private float _searchRadius = 5.0f;                                                                                                                                                    
        [SerializeField] [Min(0.0f)] private float _waitTimeAtPoint = 1.2f;
        [SerializeField] [Range(0.0f, 1.0f)] private float _directionBiasStrength = 0.6f;

        // ───── Implementation ────────────────────────────────────────────────
        
        public override GOAPActionInstance CreateInstance(GOAPAgent agent) => new SearchAreaActionInstance(agent, this, _searchPointCount, _searchRadius, _waitTimeAtPoint, _directionBiasStrength);
    }
    
    public sealed class SearchAreaActionInstance : GOAPActionInstance
    {
        // ───── Private properties ────────────────────────────────────────────────
        
        private readonly int _searchPointCount;
        private readonly float _searchRadius;                                                                                                                                                                                   
        private readonly float _waitTimeAtPoint;
        private readonly float _directionBiasStrength;
                                                                                                                                                                                                                              
        private Vector3 _searchCenter;
        private Vector3 _searchBiasDir;
        
        private int _remainingPoints;
        
        private float _waitTimer;
        private bool  _waiting;

        // ───── Implementation ────────────────────────────────────────────────   
        
        public SearchAreaActionInstance(GOAPAgent agent, GOAPActionData data, int searchPointCount, float searchRadius, float waitTimeAtPoint, float directionBiasStrength) : base(agent, data)                                                                                                                                
        {       
            _searchPointCount = searchPointCount;
            _searchRadius = searchRadius;                                                                                                                                                                                   
            _waitTimeAtPoint  = waitTimeAtPoint;
            _directionBiasStrength = directionBiasStrength;
        }                                                                                                                                                                                                                       
              
        public override bool CheckProceduralPreconditions() => Agent.Blackboard.Get<bool>(BlackboardKeys.AT_INVESTIGATION_POINT);
                                                                                                                                                                                                                              
        public override void OnStart()
        {                                                                                                                                                                                                                       
            _searchCenter = Agent.Blackboard.Get<Vector3>(BlackboardKeys.TARGET_LAST_KNOWN_POS);
            Vector3 dir = Agent.Blackboard.Get<Vector3>(BlackboardKeys.TARGET_LAST_KNOWN_DIRECTION);
            _searchBiasDir = dir.sqrMagnitude > 0.01f ? dir : Vector3.zero;
            
            _remainingPoints = _searchPointCount;
            _waitTimer = 0;                                                                                                                                                                                               
            _waiting = false;
                                                                                                                                                                                                                              
            Agent.Blackboard.Set(BlackboardKeys.MOVEMENT_SPEED, (int)MovementSpeed.Walk);                                                                                                                                       
            MoveToNextPoint();
        }                                                                                                                                                                                                                       
              
        public override ActionStatus Perform()
        { 
            if (_waiting)
            {
                _waitTimer += Time.deltaTime;
                if (_waitTimer < _waitTimeAtPoint) return ActionStatus.Running;
                                                                                                                                                                                                                              
                _waiting = false;
                _remainingPoints--;                                                                                                                                                                                             

                if (_remainingPoints <= 0)
                {
                    Agent.Blackboard.Set(BlackboardKeys.IS_INVESTIGATING, false);
                    Agent.Blackboard.Set(BlackboardKeys.AT_INVESTIGATION_POINT, false);
                    return ActionStatus.Succeeded;                                                                                                                                                                              
                }
                                                                                                                                                                                                                              
                MoveToNextPoint();
                return ActionStatus.Running;
            }

            if (!Agent.NavAgent.pathPending && Agent.NavAgent.remainingDistance <= Agent.NavAgent.stoppingDistance + 0.1f)
            {                                                                                                                                                                                                                   
                _waiting   = true;
                _waitTimer = 0;                                                                                                                                                                                                 
            }   

            return ActionStatus.Running;
        }

        public override void OnEnd()                                                                                                                                                                                            
        {
            Agent.Blackboard.Set(BlackboardKeys.AT_INVESTIGATION_POINT, false);                                                                                                                                                           
        }       

        public override void OnReset()
        {
            _remainingPoints = 0;
            _waitTimer       = 0;
            _waiting         = false;
        }

        // ───── Private methods ────────────────────────────────────────────────

        private void MoveToNextPoint()                                                                                                                                                                                          
        {       
            // Bias decays from 1 on the first point to 0 on the last
            float biasLerp = _searchPointCount > 1 ? (_remainingPoints - 1.0f) / (_searchPointCount - 1.0f) : 0.0f;                                                 
                                                                                                                                                                                                                                  
            Vector3 biasedCenter = _searchCenter + _searchBiasDir * (_searchRadius * _directionBiasStrength * biasLerp);                                                                                                                         
                                                                                                         
            for (int attempt = 0; attempt < 10; attempt++)                                                                                                                                                                              
            {                                             
                Vector2 circle = Random.insideUnitCircle * _searchRadius;
                Vector3 candidate = biasedCenter + new Vector3(circle.x, 0.0f, circle.y);
                                                                                                                                                                                                                                      
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, _searchRadius, NavMesh.AllAreas))
                {                                                                                                                                                                                                                       
                    Agent.NavAgent.SetDestination(hit.position);
                    return;                                                                                                                                                                                                             
                }          
            }    

            Agent.NavAgent.SetDestination(_searchCenter);                                                                                                                                                                     
        }
    }
}
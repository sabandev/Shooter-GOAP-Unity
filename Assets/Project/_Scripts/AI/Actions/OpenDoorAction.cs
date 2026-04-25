using UnityEngine;

namespace GOAP
{
    [CreateAssetMenu(fileName = "ACTION_OpenDoor", menuName = "GOAP/Actions/Open Door")]
    public sealed class OpenDoorActionData : SmartObjectActionData
    {
        [SerializeField] private bool _faceBeforeOpening = true;
        [SerializeField] [Min(0.0f)] private float _waitForOpenDuration = 0.7f;
        
        public bool FaceBeforeOpening => _faceBeforeOpening;
        public float WaitForOpenDuration => _waitForOpenDuration;

        public override GOAPActionInstance CreateInstance(GOAPAgent agent) => new OpenDoorActionInstance(agent, this);
    }
    
    public sealed class OpenDoorActionInstance : SmartObjectActionInstance
    {
        // -----Hashes-----
        private static readonly int _openDoorHash = Animator.StringToHash("OpenDoor");
        
        // -----Private properties-----
        private readonly OpenDoorActionData _doorData;
        
        private Door _door;
        private float _waitTimer;
        private bool _doorOpenTriggered;
        
        // -----Implementation-----
        public OpenDoorActionInstance(GOAPAgent agent, OpenDoorActionData actionData) : base(agent, actionData)
        {
            _doorData = actionData;
        }

        public override bool CheckProceduralPreconditions()
        {
            Transform doorTransform = Agent.Blackboard.Get<Transform>(BlackboardKeys.DOOR_TRANSFORM);
            return doorTransform != null && doorTransform.GetComponent<Door>() != null && !doorTransform.GetComponent<Door>().IsOpen;
        }

        public override void OnStart()
        {
            base.OnStart();
            _waitTimer = 0.0f;
            _doorOpenTriggered = false;
            
            Agent.Blackboard.Set(BlackboardKeys.MOVEMENT_SPEED, (int) MovementSpeed.Walk);
        }

        protected override void OnArrival()
        {
            Agent.NavAgent.ResetPath();
            _door = TargetObject.GetComponent<Door>();
            
            Agent.Blackboard.Set(BlackboardKeys.DOOR_IS_OPENING, true);
            
            if (_door == null)
            {
                Debug.LogWarning($"[OpenDoorAction] SmartObject '{TargetObject.name}' has no Door component.");
                Agent.Blackboard.Set(BlackboardKeys.DOOR_IS_OPENING, false);
                FailInteraction();
                return;
            }
            
            if (_door.IsOpen)
            {
                Agent.Blackboard.Set(BlackboardKeys.DOOR_IS_OPENING, false);
                WriteDoorOpenState();
                CompleteInteraction();
                return;
            }
            
            if (_doorData.FaceBeforeOpening)
            {
                Vector3 direction = (_door.transform.position - Agent.transform.position).normalized;
                direction.y = 0.0f;
                
                if (direction != Vector3.zero)
                    Agent.transform.rotation = Quaternion.LookRotation(direction);
            }
            
            if (Agent.Animator != null)
                Agent.Animator.SetTrigger(_openDoorHash);
            
            _door.Open(Agent.transform.position);
            _doorOpenTriggered = true;
            _waitTimer = 0.0f;
        }

        protected override ActionStatus TickInteraction()
        {
            if (!_doorOpenTriggered) { return ActionStatus.Failed; }
            
            _waitTimer += Time.deltaTime;
            
            if (_waitTimer < _doorData.WaitForOpenDuration) { return ActionStatus.Running; }
            
            Agent.Blackboard.Set(BlackboardKeys.DOOR_IS_OPENING, false);
            
            WriteDoorOpenState();
            CompleteInteraction();
            return ActionStatus.Succeeded;
        }

        public override void OnEnd()
        {
            Agent.Blackboard.Set(BlackboardKeys.DOOR_IS_OPENING, false);
            base.OnEnd();
            Agent.Blackboard.Set(BlackboardKeys.MOVEMENT_SPEED, (int) MovementSpeed.Walk);
        }

        public override void OnReset()
        {
            Agent.Blackboard.Set(BlackboardKeys.DOOR_IS_OPENING, false);
            base.OnReset();
            _door = null;
            _waitTimer = 0.0f;
            _doorOpenTriggered = false;
        }
        
        // -----Private methods-----
        private void WriteDoorOpenState()
        {
            Agent.Blackboard.Set(BlackboardKeys.DOOR_AHEAD, false);
            Agent.Blackboard.Set(BlackboardKeys.DOOR_IS_OPEN, true);
            
            // Debug.Log($"[OpenDoorAction] '{Agent.name}' opened '{TargetObject?.name}'.");
        }
    }
}


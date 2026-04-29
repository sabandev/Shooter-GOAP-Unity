using UnityEngine;

namespace GOAP
{
    [CreateAssetMenu(fileName = "ACTION_Pickup", menuName = "GOAP/Actions/Pickup")]
    public sealed class PickupActionData : SmartObjectActionData
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("Pickup")]
        [SerializeField] private string _acquiredItemKey = "HAS_ITEM";
        [SerializeField] [Min(0.0f)] private float _pickupDuration = 0.8f;
        [SerializeField] [Range(0.0f, 1.0f)] private float _itemDisappearNormalizedFrame = 0.45f;
        [SerializeField] private bool _disableOnPickup = true;

        // ───── Public properties ────────────────────────────────────────────────
        
        public string AcquiredItemKey => _acquiredItemKey;
        public float  PickupDuration  => _pickupDuration;
        public float ItemDisappearNormalizedFrame => _itemDisappearNormalizedFrame;
        public bool   DisableOnPickup => _disableOnPickup;

        public override GOAPActionInstance CreateInstance(GOAPAgent agent) => new PickupActionInstance(agent, this);
    }

    public sealed class PickupActionInstance : SmartObjectActionInstance
    {
        // ─── Hashes  ──────────────────────────
        
        private static readonly int _pickupTriggerHash = Animator.StringToHash("Pickup"); //TODO: Static check

        // ─── Private properties ────────────────────────────────────────────
        
        private readonly PickupActionData _pickupData;
        
        private float _pickupTimer;
        private bool _itemDisappeared;

        public PickupActionInstance(
            GOAPAgent      agent,
            PickupActionData data) : base(agent, data)
        {
            _pickupData = data;
        }

        // ─── Implementation ─────────────────

        public override void OnStart()
        {
            base.OnStart();
            
            _pickupTimer = 0.0f;
        }

        protected override void OnArrival()
        {
            // Stop movement
            Agent.NavAgent.ResetPath();
            Agent.NavAgent.velocity = Vector3.zero;

            // Face the object
            Vector3 direction =
                (TargetObject.transform.position -
                 Agent.transform.position).normalized;

            direction.y = 0.0f;

            if (direction != Vector3.zero)
                Agent.transform.rotation =
                    Quaternion.LookRotation(direction);

            // Trigger pickup animation if available
            if (Agent.Animator != null)
            {
                Agent.Animator.ResetTrigger(_pickupTriggerHash);
                Agent.Animator.SetTrigger(_pickupTriggerHash);
            }

            _pickupTimer = 0.0f;
            _itemDisappeared = false;
        }

        protected override ActionStatus TickInteraction()
        {
            _pickupTimer += Time.deltaTime;

            float normalizedTime = _pickupTimer / _pickupData.PickupDuration;
            if (!_itemDisappeared && normalizedTime >= _pickupData.ItemDisappearNormalizedFrame)
            {
                DisappearItem();
                _itemDisappeared = true;
            }

            if (_pickupTimer < _pickupData.PickupDuration)
                return ActionStatus.Running;

            // Pickup complete — apply effects
            ApplyPickup();
            CompleteInteraction();
            return ActionStatus.Succeeded;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            Agent.Blackboard.Set(BlackboardKeys.MOVEMENT_SPEED, (int) MovementSpeed.Walk);
        }

        public override void OnReset()
        {
            base.OnReset();
            _pickupTimer = 0.0f;
            _itemDisappeared = false;
        }

        // ─── Private helpers ──────────────────────────────────────────

        private void ApplyPickup()
        {
            if (TargetObject == null) return;

            // Write acquired item fact to blackboard
            Agent.Blackboard.Set(_pickupData.AcquiredItemKey, true);

            // Also apply any advertised effects from the smart object
            foreach (WorldStatePair effect in TargetObject.Data.AdvertisedEffects)
            {
                effect.ApplyTo(Agent.Blackboard);
            }
        }

        private void DisappearItem()
        {
            if (TargetObject == null) { return; }

            if (_pickupData.DisableOnPickup)
            {
                TargetObject.SetPermanentlyUnavailable();

                foreach (Renderer r in TargetObject.GetComponentsInChildren<Renderer>())
                    r.enabled = false;
            }
        }
    }
}
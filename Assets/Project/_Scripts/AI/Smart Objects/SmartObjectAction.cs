using UnityEngine;
using UnityEngine.AI;

namespace GOAP
{
    // ══════════════════════════════════════════════════════════════════
    // DATA ASSET
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Abstract ScriptableObject base for all SmartObject action data.
    ///
    /// Concrete subclasses specify which SmartObjectData tag they
    /// target and any additional configuration.
    ///
    /// Does not create assets directly — subclasses define their
    /// own CreateAssetMenu attributes.
    /// </summary>
    public abstract class SmartObjectActionData : GOAPActionData
    {
        [Header("Smart Object Targeting")]
        [Tooltip("The SmartObjectData tag this action searches for. " +
                 "Must match SmartObjectData.ObjectTag exactly.")]
        [SerializeField] private string _targetObjectTag = "";

        [Tooltip("Maximum distance to search for a target object. " +
                 "Objects beyond this range are ignored.")]
        [SerializeField] private float _searchRange = 20.0f;

        public string TargetObjectTag => _targetObjectTag;
        public float  SearchRange     => _searchRange;
    }


    // ══════════════════════════════════════════════════════════════════
    // ACTION INSTANCE
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Abstract base for all SmartObject action instances.
    ///
    /// Handles the common navigation + reservation lifecycle:
    ///   1. Find nearest available target object
    ///   2. Reserve it
    ///   3. Navigate to interaction point
    ///   4. Execute interaction (subclass)
    ///   5. Complete or release reservation
    ///
    /// Subclasses implement OnArrival() — called once when the agent
    /// reaches the interaction point — and optionally override
    /// TickInteraction() for multi-frame interactions.
    ///
    /// Subclasses must call CompleteInteraction() or FailInteraction()
    /// to terminate the action.
    /// </summary>
    public abstract class SmartObjectActionInstance : GOAPActionInstance
    {
        // ─── State machine ────────────────────────────────────────────

        protected enum InteractionState
        {
            FindingTarget,
            Navigating,
            Interacting,
            Complete,
            Failed
        }

        // ─── Private/Protected state ──────────────────────────────────

        protected SmartObject       TargetObject  { get; private set; }
        protected InteractionState  CurrentState  { get; private set; }
        protected SmartObjectActionData SmartData =>
            (SmartObjectActionData)Data;

        // ─── Constructor ──────────────────────────────────────────────

        protected SmartObjectActionInstance(
            GOAPAgent      agent,
            GOAPActionData data) : base(agent, data) { }

        // ─── GOAPActionInstance overrides ─────────────────────────────

        public override bool CheckProceduralPreconditions()
        {
            // Check a target exists and is reachable
            SmartObject target = SmartObjectRegistry.Instance.FindNearestForAgent(SmartData.TargetObjectTag, Agent.transform.position, Agent);

            if (target == null) return false;

            // Check agent meets required state for this object
            WorldState required = target.Data.GetRequiredAgentState();
            WorldState snapshot = Agent.Blackboard.GetWorldStateSnapshot();

            return snapshot.Satisfies(required);
        }

        public override void OnStart()
        {
            CurrentState = InteractionState.FindingTarget;
            TargetObject = null;
        }

        public override ActionStatus Perform()
        {
            return CurrentState switch
            {
                InteractionState.FindingTarget => TickFindTarget(),
                InteractionState.Navigating    => TickNavigate(),
                InteractionState.Interacting   => TickInteraction(),
                InteractionState.Complete      => ActionStatus.Succeeded,
                InteractionState.Failed        => ActionStatus.Failed,
                _                              => ActionStatus.Failed
            };
        }

        public override void OnEnd()
        {
            // Release reservation if action ends without completing
            if (TargetObject != null &&
                CurrentState != InteractionState.Complete)
            {
                TargetObject.ReleaseReservation(Agent);
            }
        }

        public override void OnReset()
        {
            if (TargetObject != null)
                TargetObject.ReleaseReservation(Agent);

            TargetObject  = null;
            CurrentState  = InteractionState.FindingTarget;
        }

        // ─── Abstract interface ───────────────────────────────────────

        /// <summary>
        /// Called once when the agent arrives at the interaction point.
        /// Subclasses begin their interaction here — play animation,
        /// apply effects, etc.
        /// Set CurrentState to Complete or Failed when done.
        /// </summary>
        protected abstract void OnArrival();

        /// <summary>
        /// Called every frame while in Interacting state.
        /// Default implementation returns Succeeded immediately —
        /// override for multi-frame interactions.
        /// </summary>
        protected virtual ActionStatus TickInteraction()
        {
            return ActionStatus.Succeeded;
        }

        // ─── Protected helpers ────────────────────────────────────────

        /// <summary>
        /// Marks interaction as complete. Apply world state changes
        /// before calling this.
        /// </summary>
        protected void CompleteInteraction()
        {
            TargetObject?.CompleteInteraction(Agent);
            CurrentState = InteractionState.Complete;
        }

        /// <summary>
        /// Marks interaction as failed and releases reservation.
        /// </summary>
        protected void FailInteraction()
        {
            TargetObject?.ReleaseReservation(Agent);
            CurrentState = InteractionState.Failed;
        }

        // ─── Private tick methods ─────────────────────────────────────

        private ActionStatus TickFindTarget()
        {
            SmartObject target = SmartObjectRegistry.Instance.FindNearest(
                SmartData.TargetObjectTag,
                Agent.transform.position);

            if (target == null)
                return ActionStatus.Failed;

            // Check range
            float distSqr = (target.transform.position -
                             Agent.transform.position).sqrMagnitude;

            if (distSqr > SmartData.SearchRange * SmartData.SearchRange)
                return ActionStatus.Failed;

            // Try to reserve
            if (!target.TryReserve(Agent))
                return ActionStatus.Failed;

            TargetObject = target;
            Agent.NavAgent.SetDestination(target.InteractionPoint);
            CurrentState = InteractionState.Navigating;

            return ActionStatus.Running;
        }

        private ActionStatus TickNavigate()
        {
            if (TargetObject == null)
                return ActionStatus.Failed;

            // Check object hasn't been taken while navigating
            if (!TargetObject.IsAvailable &&
                TargetObject.ReservingAgent != Agent)
            {
                TargetObject = null;
                CurrentState = InteractionState.FindingTarget;
                return ActionStatus.Running;
            }

            // Check path validity
            if (Agent.NavAgent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                FailInteraction();
                return ActionStatus.Failed;
            }

            // Check arrival
            float distToPoint = Vector3.Distance(
                Agent.transform.position,
                TargetObject.InteractionPoint);

            if (!Agent.NavAgent.pathPending &&
                distToPoint <= TargetObject.Data.InteractionRange)
            {
                Agent.NavAgent.ResetPath();
                TargetObject.BeginInteraction(Agent);
                CurrentState = InteractionState.Interacting;
                OnArrival();
            }

            return ActionStatus.Running;
        }
    }
}
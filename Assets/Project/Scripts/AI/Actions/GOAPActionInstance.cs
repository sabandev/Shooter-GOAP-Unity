using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// The runtime action owned by a GOAP agent.
    /// 
    /// Authorised Use Instructions
    ///     - One instance should be created per-agent and it should be re-used
    ///       for multiple plans. This means it should always be reset after
    ///       finihing (succeeded/failed)
    ///     - Subclasses should validate real-world preconditions that cannot be symbolically
    ///       represented and reasoned by the GOAP planner
    ///     - Subclasses should execute the action each tick and return progress as ActionStatus
    ///     - Subclasses should clean up all mutable state when the action is finished
    ///     - CheckProceduralPreconditions must be kept fast and contain no expensive method calls (e.g. SetDestination()) as it runs
    ///       in a hot loop
    /// </summary>
    public abstract class GOAPActionInstance
    {
        // -----Protected properties-----
        protected GOAPAgent Agent { get; }

        // -----Public properties-----
        public GOAPActionData Data { get; }
        public float Cost => Data.Cost;

        // -----Constructor-----
        protected GOAPActionInstance(GOAPAgent agent, GOAPActionData data)
        {
            Debug.Assert(agent != null, "[GOAPActionInstance] Agent must not be null.");
            Debug.Assert(data != null, "[GOAPActionInstance] GOAPActionData must not be null.");

            Agent = agent;
            Data = data;
        }

        // -----Public methods-----

        /// <summary>
        /// Checks REAL-WORLD preconditions that the GOAP planner cannot reason. The planner does all symbolic checks.
        /// </summary>
        /// <returns></returns>
        public abstract bool CheckProceduralPreconditions();

        /// <summary>
        /// Executes one tick of action behaviour, called every frame to drive behaviour.
        /// </summary>
        /// <returns></returns>
        public abstract ActionStatus Perform();

        /// <summary>
        /// Resets all mutable states. Must be used to reset action to default because action instances are re-used
        /// across multiple plans.
        /// </summary>
        public abstract void OnReset();

        public virtual void OnStart() {}
        public virtual void OnEnd() {}

        // -----Diagnostics-----
        public override string ToString() => $"[{Data.ActionName}] on {Agent.name}";
    }
}


using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// The "brain" of the GOAP AI. Produces an ordered list of actions to satisfy
    /// a GOAP agent's goal state. Uses a regressive A* search to find a plan of actions.
    /// </summary>
    public sealed class GOAPPlanner
    {
        // -----Nested types-----
        private sealed class PlanNode
        {
            // -----Public properties-----
            public WorldState RequiredState { get; }
            public float RunningCost { get; }
            public GOAPActionInstance Action { get; }
            public PlanNode Parent { get; }

            // -----Constructor-----
            public PlanNode(WorldState requiredState, float runningCost, GOAPActionInstance action, PlanNode parent)
            {
                RequiredState = requiredState;
                RunningCost = runningCost;
                Action = action;
                Parent = parent;
            }
        }

        // -----Constants-----
        private const int MAX_SEARCH_NODES = 1000;

        // -----Public methods-----

        /// <summary>
        /// Returns the plan as a list of GOAP actions.
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="goal"></param>
        /// <param name="availableActions"></param>
        /// <returns></returns>
        public List<GOAPActionInstance> Plan(WorldState currentState, WorldState goal, IReadOnlyList<GOAPActionInstance> availableActions)
        {
            // LogPlannerDiagnostics(currentState, goal, availableActions);

            // Nodes that are waiting to be expanded upon. Ordered by A* heuristic
            // calculation f(n) = g(n) + h(n).
            // If action sets have > 30 actions, use a priority queue for better performance
            var openSet = new List<PlanNode>();

            var root = new PlanNode(goal.Clone(), 0.0f, null, null);

            openSet.Add(root);

            int nodesExpanded = 0;

            while (openSet.Count > 0)
            {
                if (nodesExpanded >= MAX_SEARCH_NODES)
                {
                    Debug.LogWarning($"[GOAPPlanner] Search exceed {MAX_SEARCH_NODES} nodes. No plan found. Must reduce action set complexity or increase search node ceiling.");
                    return null;
                }

                PlanNode current = PopLowestCostNode(openSet);
                nodesExpanded++;

                // Plan has successfully been found
                if (currentState.Satisfies(current.RequiredState))
                    return ReconstructPlan(current);
                
                foreach (GOAPActionInstance action in availableActions)
                {
                    if (!ActionSatisfiesAnyRequirement(action, current.RequiredState)) { continue; }

                    WorldState nextRequired = BuildNextRequiredState(current.RequiredState, action);

                    if (!action.CheckProceduralPreconditions()) { continue; }

                    float nextCost = current.RunningCost + action.Cost;

                    var child = new PlanNode(nextRequired, nextCost, action, current);
                    InsertByPriority(openSet, child, currentState);
                }
            }

            // No valid plan exists given the list of available actions
            return null;
        }

        // -----Private methods-----

        /// <summary>
        /// Walks the chain of nodes from the leaf to the root. Then creates a new reversed version of the list
        /// that can get returned to the GOAP agent.
        /// </summary>
        /// <param name="leafNode"></param>
        /// <returns></returns>
        private static List<GOAPActionInstance> ReconstructPlan(PlanNode leafNode)
        {
            var reversedPlan = new List<GOAPActionInstance>();

            PlanNode current = leafNode;
            while (current.Parent != null)
            {
                reversedPlan.Add(current.Action);
                current = current.Parent;
            }

            reversedPlan.Reverse();
            return reversedPlan;
        }

        /// <summary>
        /// Returns true if any of the action's effects satisfy at least one of the currently required state.
        /// Filters out irrelevant actions.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="requiredState"></param>
        /// <returns></returns>
        private static bool ActionSatisfiesAnyRequirement(GOAPActionInstance action, WorldState requiredState)
        {
            foreach (WorldStatePair effect in action.Data.Effects)
            {
                if (requiredState.Contains(effect.Key) && effect.ValueEquals(requiredState.Get(effect.Key))) { return true; }
            }

            return false;
        }

        /// <summary>
        /// Builds and returns the required state for the next node to satisfy.
        /// </summary>
        /// <param name="currentRequired"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private static WorldState BuildNextRequiredState(WorldState currentRequired, GOAPActionInstance action)
        {
            WorldState next = currentRequired.Clone();

            // Remove facts that are already satisfied by the action's effects
            foreach (WorldStatePair effect in action.Data.Effects)
                next.Remove(effect.Key);

            // Add this action's preconditions as new requirements
            foreach (WorldStatePair precondition in action.Data.Preconditions)
                next.Set(precondition.Key, precondition.GetValue());
            
            return next;
        }

        /// <summary>
        /// A* quirk that calculates the number of unsatisfied facts remaining in the required state that
        /// are not present in the current state.
        /// </summary>
        /// <param name="requiredState"></param>
        /// <param name="currentState"></param>
        /// <returns></returns>
        private static float Heuristic(WorldState requiredState, WorldState currentState)
        {
            int unsatisfiedCount = 0;

            foreach (KeyValuePair<string, object> fact in requiredState.GetFacts())
            {
                if (!currentState.TryGet(fact.Key, out object value) || !Equals(value, fact.Value))
                    unsatisfiedCount++;
            }

            return unsatisfiedCount;
        }

        /// <summary>
        /// Removes and returns the PlanNode with the lowest heuristic.
        /// </summary>
        /// <param name="openSet"></param>
        /// <returns></returns>
        private static PlanNode PopLowestCostNode(List<PlanNode> openSet)
        {
            // Lowest cost node is always the first element in the list because of InsertByPriority method
            PlanNode best = openSet[0];
            openSet.RemoveAt(0);
            return best;
        }

        /// <summary>
        /// Inserts a node into the open set in ascending order so the lowest cost node is always at index 0
        /// </summary>
        /// <param name="openSet"></param>
        /// <param name="node"></param>
        /// <param name="currentState"></param>
        private static void InsertByPriority(List<PlanNode> openSet, PlanNode node, WorldState currentState)
        {
            float f = node.RunningCost + Heuristic(node.RequiredState, currentState);

            for (int i = 0; i < openSet.Count; i++)
            {
                float existingF = openSet[i].RunningCost + Heuristic(openSet[i].RequiredState, currentState);

                if (f < existingF)
                {
                    openSet.Insert(i, node);
                    return;
                }
            }

            openSet.Add(node);
        }

        private void LogPlannerDiagnostics(WorldState currentState, WorldState goal, IReadOnlyList<GOAPActionInstance> availableActions)
        {
            System.Text.StringBuilder sb = new();

            sb.AppendLine("─── Planner Diagnostic ───────────────────");
            sb.AppendLine($"Goal state:");

            foreach (var fact in goal.GetFacts())
                sb.AppendLine($"  {fact.Key} = {fact.Value}");

            sb.AppendLine($"Current state:");

            foreach (var fact in currentState.GetFacts())
                sb.AppendLine($"  {fact.Key} = {fact.Value}");

            sb.AppendLine($"Available actions ({availableActions.Count}):");

            foreach (GOAPActionInstance action in availableActions)
            {
                sb.AppendLine($"  {action.Data.ActionName}");
                sb.AppendLine($"    Preconditions:");

                foreach (WorldStatePair pair in action.Data.Preconditions)
                    sb.AppendLine($"      {pair.Key} = {pair.GetValue()}");

                sb.AppendLine($"    Effects:");

                foreach (WorldStatePair pair in action.Data.Effects)
                    sb.AppendLine($"      {pair.Key} = {pair.GetValue()}");

                bool proceduralPass = action.CheckProceduralPreconditions();
                sb.AppendLine($"    Procedural preconditions: " +
                            $"{(proceduralPass ? "PASS" : "FAIL")}");
            }

            sb.AppendLine("──────────────────────────────────────────");
            Debug.Log(sb.ToString());
        }
    }
}


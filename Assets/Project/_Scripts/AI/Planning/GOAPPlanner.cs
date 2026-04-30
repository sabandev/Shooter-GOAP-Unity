using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;

namespace GOAP
{
    /// <summary>
    /// The "brain" of the GOAP AI. Produces an ordered list of actions to satisfy
    /// a GOAP agent's goal state. Uses a regressive A* search to find a plan of actions.
    /// </summary>
    public sealed class GOAPPlanner
    {
        // ───── Nested type ────────────────────────────────────────────────
        
        private sealed class PlanNode
        {
            // ───── Public properties ────────────────────────────────────────────────
            
            public WorldState RequiredState { get; }
            public GOAPActionInstance Action { get; }
            public PlanNode Parent { get; }
            
            public float RunningCost { get; }

            // ───── Constructor ────────────────────────────────────────────────
            
            public PlanNode(WorldState requiredState, float runningCost, GOAPActionInstance action, PlanNode parent)
            {
                RequiredState = requiredState;
                RunningCost = runningCost;
                Action = action;
                Parent = parent;
            }
        }

        // ───── Private properties ────────────────────────────────────────────────
        
        private static ProfilerMarker _planMarker = new("GOAPPlanner.Plan");
        
        private readonly Stack<WorldState> _statePool = new(32);
        private readonly List<WorldState> _rentedThisSearch = new(32);

        // ───── Constants ────────────────────────────────────────────────
        
        private const int MAX_SEARCH_NODES = 1000;

        // ───── Public methods ────────────────────────────────────────────────
        
        /// <summary>
        /// Returns the plan as a list of GOAP actions.
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="goal"></param>
        /// <param name="availableActions"></param>
        /// <param name="nodesExpanded"></param>
        /// <returns></returns>
        public List<GOAPActionInstance> Plan(
            WorldState currentState, 
            WorldState goal, 
            IReadOnlyList<GOAPActionInstance> availableActions, 
            out int nodesExpanded 
#if UNITY_EDITOR 
        , PlanSearchTrace trace = null
#endif
        )
        {
            using (_planMarker.Auto())
            {
                // LogPlannerDiagnostics(currentState, goal, availableActions);

                // Nodes that are waiting to be expanded upon. Ordered by A* heuristic
                // calculation f(n) = g(n) + h(n).
                // If action sets have > 30 actions, use a priority queue for better performance
                var openSet = new List<PlanNode>();
                
                #if UNITY_EDITOR
                var nodeIndex = trace != null ? new Dictionary<PlanNode, int>() : null;
                #endif

                var root = new PlanNode(RentState(goal), 0.0f, null, null);

                openSet.Add(root);

                nodesExpanded = 0;
                
                #if UNITY_EDITOR
                if (trace != null)
                {
                    SnapshotState(goal, trace.GoalStateFacts);
                    SnapshotState(currentState, trace.CurrentStateFacts);
                    
                    var rootRecord = new TraceNode
                    {
                        ActionName = null,
                        ParentIndex = -1,
                        Depth = 0,
                        GCost = 0,
                        HCost = Heuristic(root.RequiredState, currentState),
                    };
                    
                    SnapshotState(root.RequiredState, rootRecord.RequiredFacts);
                    trace.Nodes.Add(rootRecord);
                    nodeIndex![root] = 0;
                }
                #endif

                while (openSet.Count > 0)
                {
                    if (nodesExpanded >= MAX_SEARCH_NODES)
                    {
                        Debug.LogWarning($"[GOAPPlanner] Search exceed {MAX_SEARCH_NODES} nodes. No plan found. Must reduce action set complexity or increase search node ceiling.");
                        ReturnAllRented();
                        return null;
                    }

                    PlanNode current = PopLowestCostNode(openSet);
                    
                    #if UNITY_EDITOR
                    int curIdx = trace != null ? nodeIndex![current] : -1;
                    #endif
                    
                    nodesExpanded++;

                    // Plan has successfully been found
                    if (currentState.Satisfies(current.RequiredState))
                    {
                        List<GOAPActionInstance> plan = ReconstructPlan(current);
                        
                        #if UNITY_EDITOR
                        if (trace != null)
                        {
                            PlanNode n = current;
                            while (n != null && nodeIndex!.TryGetValue(n, out int idx))
                            {
                                trace.Nodes[idx].IsOnWinningPath = true;
                                n = n.Parent;
                            }
                            trace.PlanFound = true;
                        }
                        #endif
                        
                        ReturnAllRented();
                        return plan;
                    }
                    
                    foreach (GOAPActionInstance action in availableActions)
                    {
                        if (!ActionSatisfiesAnyRequirement(action, current.RequiredState))
                        {
                            #if UNITY_EDITOR
                            trace?.Nodes[curIdx].Rejected.Add(new RejectedAction { ActionName = action.Data.ActionName });
                            #endif
                            continue;
                        }

                        WorldState nextRequired = BuildNextRequiredState(current.RequiredState, action);
                        float nextCost = current.RunningCost + action.Cost;
                        var child = new PlanNode(nextRequired, nextCost, action, current);
                        InsertByPriority(openSet, child, currentState);
                        
                        #if UNITY_EDITOR
                        if (trace != null)
                        {
                            var record = new TraceNode
                            {
                                ActionName = action.Data.ActionName,
                                ParentIndex = curIdx,
                                Depth = trace.Nodes[curIdx].Depth + 1,
                                GCost = nextCost,
                                HCost = Heuristic(nextRequired, currentState),
                            };
                            
                            SnapshotState(nextRequired, record.RequiredFacts);
                            nodeIndex![child] = trace.Nodes.Count;
                            trace.Nodes.Add(record);
                        }
                        #endif
                    }
                }

                // No valid plan exists given the list of available actions
                ReturnAllRented();
                return null;
            }
        }

        // ───── Private methods ────────────────────────────────────────────────
        
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
                if (effect.ValueEquals(requiredState)) { return true; }
            }

            return false;
        }

        /// <summary>
        /// Builds and returns the required state for the next node to satisfy.
        /// </summary>
        /// <param name="currentRequired"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private WorldState BuildNextRequiredState(WorldState currentRequired, GOAPActionInstance action)
        {
            WorldState next = RentState(currentRequired);

            // Remove facts that are already satisfied by the action's effects
            foreach (WorldStatePair effect in action.Data.Effects)
                next.Remove(effect.Key);

            // Add this action's preconditions as new requirements
            foreach (WorldStatePair precondition in action.Data.Preconditions)
                precondition.ApplyTo(next);
            
            return next;
        }

        private static float Heuristic(WorldState requiredState, WorldState currentState) => requiredState.CountUnsatisfied(currentState);

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
        
        #if UNITY_EDITOR
        private static void SnapshotState(WorldState state, List<(string, string)> into)
        {
            foreach (KeyValuePair<string, object> fact in state.GetFacts())
                into.Add((fact.Key, fact.Value?.ToString() ?? "null"));
        }
        #endif
        
        private WorldState RentState(WorldState copyFrom)
        {
            WorldState state = _statePool.Count > 0 ? _statePool.Pop() : new WorldState();
            state.CopyFrom(copyFrom);
            _rentedThisSearch.Add(state);
            return state;
        }
        
        private void ReturnAllRented()
        {
            foreach (WorldState state in _rentedThisSearch)
            {
                state.Clear();
                _statePool.Push(state);
            }
            
            _rentedThisSearch.Clear();
        }

        private void LogPlannerDiagnostics(WorldState currentState, WorldState goal, IReadOnlyList<GOAPActionInstance> availableActions)
        {
            System.Text.StringBuilder sb = new();

            sb.AppendLine("─── Planner Diagnostic ───────────────────");
            sb.AppendLine($"Goal state:");
            sb.AppendLine($"  {goal}");

            sb.AppendLine($"Current state:");
            sb.AppendLine($"  {currentState}");

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
                sb.AppendLine($"    Procedural preconditions: {(proceduralPass ? "PASS" : "FAIL")}");
            }

            sb.AppendLine("──────────────────────────────────────────");
            Debug.Log(sb.ToString());
        }
    }
}


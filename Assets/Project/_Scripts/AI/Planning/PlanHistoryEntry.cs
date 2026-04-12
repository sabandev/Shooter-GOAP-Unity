using System.Collections.Generic;

namespace GOAP
{
        /// <summary>
        /// Represents an entry in the agen't planning history log.
        /// Records information from a planning event for the debugger window.
        /// </summary>
        public sealed class PlanHistoryEntry
        {
            // -----Public properties-----
            public float Timestamp { get; }
            public string GoalName { get; }
            public string TriggerReason { get; }
            public List<string> PlanActionNames { get; }
            public int NodesExpanded { get; }
            public bool PlanFound { get; }
            // public IReadOnlyList<PlanHistoryEntry> PlanHistory => _planHistory;

            // -----Constants-----

            // -----Private properties-----
            // private readonly List<PlanHistoryEntry> _planHistory = new();

            // -----Constructor-----
            public PlanHistoryEntry(float timestamp, string goalName, string triggerReason, List<string> planActionNames, int nodesExpanded, bool planFound)
            {
                Timestamp = timestamp;
                GoalName = goalName;
                TriggerReason = triggerReason;
                PlanActionNames = planActionNames;
                NodesExpanded = nodesExpanded;
                PlanFound = planFound;
            }
        }
}


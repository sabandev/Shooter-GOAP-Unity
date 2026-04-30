#if UNITY_EDITOR
using System.Collections.Generic;

namespace GOAP
{
    /// <summary>
    /// Represents a trace of the planner's search graph.
    /// </summary>
    public sealed class PlanSearchTrace
    {
        // ───── Public properties ────────────────────────────────────────────────
        
        public string GoalName { get; set; }
        public string TriggerReason { get; set; }
        
        public float Timestamp { get; set; }
        
        public bool PlanFound  { get; set; }
        
        public List<(string key, string value)> GoalStateFacts { get; } = new();
        public List<(string key, string value)> CurrentStateFacts { get; } = new();
        public List<TraceNode> Nodes { get; } = new();
    }
    
    /// <summary>
    /// Represents a node (action) in the search trace.
    /// </summary>
    public sealed class TraceNode
    {
        // ───── Public properties ────────────────────────────────────────────────
        
        public string ActionName { get; set; }
        
        public int ParentIndex { get; set; }
        public int Depth { get; set; }
        
        public float GCost { get; set; }
        public float HCost { get; set; }
        public float FCost => GCost + HCost;
        
        public bool IsOnWinningPath { get; set; }
        
        public List<(string key, string value)> RequiredFacts { get; } = new();
        public List<RejectedAction> Rejected { get; } = new();
    }
    
    /// <summary>
    /// Stores the name of a rejcted action from the search trace.
    /// </summary>
    public sealed class RejectedAction
    {
        // ───── Public properties ────────────────────────────────────────────────
        
        public string ActionName { get; set; }
    }
}
#endif
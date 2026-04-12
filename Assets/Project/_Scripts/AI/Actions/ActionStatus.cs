using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Represents the execution state of a GOAP action on any given frame.
    /// </summary>
    public enum ActionStatus
    {
        Running = 0,
        Succeeded = 1,
        Failed = 2,
    }
}


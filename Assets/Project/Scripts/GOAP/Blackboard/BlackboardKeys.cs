using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// Centralised data for string constants for all blackboard key facts. Used to prevent string dependency/mis-types.
    /// 
    /// Authorised Use Instructions:
    ///     - Add new keys as and when sensors/actions require them
    /// </summary>
    public static class BlackboardKeys
    {
        // -----Agent States-----
        public const string IS_IDLE = "IS_IDLE";
        public const string IS_PATROLLING = "IS_PATROLLING";
        public const string IS_INVESTIGATING = "IS_INVESTIGATING";

        // -----Vision-----
        public const string TARGET_VISIBLE = "TARGET_VISIBLE";
        public const string TARGET_LAST_KNOWN_POS = "TARGET_LAST_KNOWN_POS";
        public const string TARGET_DISTANCE = "TARGET_DISTANCE";
    }
}


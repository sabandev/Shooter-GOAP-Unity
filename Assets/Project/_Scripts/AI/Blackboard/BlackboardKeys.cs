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
        public const string TARGET_IN_MELEE_RANGE = "TARGET_IN_MELEE_RANGE";
        public const string TARGET_IS_DEAD = "TARGET_IS_DEAD";
        public const string TARGET_TRANSFORM = "TARGET_TRANSFORM";

        // -----Locomotion States-----
        public const string MOVEMENT_SPEED = "MOVEMENT_SPEED";

        // -----Vision-----
        public const string TARGET_VISIBLE = "TARGET_VISIBLE";
        public const string TARGET_LAST_KNOWN_POS = "TARGET_LAST_KNOWN_POS";
        public const string TARGET_DISTANCE = "TARGET_DISTANCE";
    }
}


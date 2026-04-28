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
        // ───── Agent states ────────────────────────────────────────────────
        public const string IS_IDLE = "IS_IDLE";
        public const string IS_PATROLLING = "IS_PATROLLING";
        public const string IS_INVESTIGATING = "IS_INVESTIGATING";
        public const string TARGET_IN_MELEE_RANGE = "TARGET_IN_MELEE_RANGE";
        public const string TARGET_IS_DEAD = "TARGET_IS_DEAD";
        public const string TARGET_TRANSFORM = "TARGET_TRANSFORM";
        public const string TARGET_LAST_KNOWN_DIRECTION = "TARGET_LAST_KNOWN_DIRECTION";
        public const string AT_INVESTIGATION_POINT = "AT_INVESTIGATION_POINT";

        // ───── Agent inventory ────────────────────────────────────────────────
        public const string HAS_ITEM = "HAS_ITEM";

        // ───── Locomotion states ────────────────────────────────────────────────
        public const string MOVEMENT_SPEED = "MOVEMENT_SPEED";

        // ───── Vision ────────────────────────────────────────────────
        public const string TARGET_VISIBLE = "TARGET_VISIBLE";
        public const string TARGET_LAST_KNOWN_POS = "TARGET_LAST_KNOWN_POS";
        public const string TARGET_DISTANCE = "TARGET_DISTANCE";

        // ───── SmartObject states ────────────────────────────────────────────────
        public const string PICKUP_AVAILABLE = "PICKUP_AVAILABLE";
        public const string PICKUP_TRANSFORM = "PICKUP_TRANSFORM";
        public const string PICKUP_DISTANCE  = "PICKUP_DISTANCE";
        public const string DOOR_AHEAD = "DOOR_AHEAD";
        public const string DOOR_IS_OPEN = "DOOR_IS_OPEN";
        public const string DOOR_TRANSFORM = "DOOR_TRANSFORM";
        public const string DOOR_IS_OPENING = "DOOR_IS_OPENING";
    }
}


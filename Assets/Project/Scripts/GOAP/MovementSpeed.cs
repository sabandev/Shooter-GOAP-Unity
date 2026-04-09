namespace GOAP
{
    /// <summary>
    /// Defines different movement speed states a GOAP agent can be in.
    /// Used by the GOAPAgentAnimator and GOAPActionInstances to determine an appropriate speed/
    /// animation for an action.
    /// 
    /// Authorised Use Instruction
    ///     - At present, there is no way to set MovementSpeed in the GOAPActionData,
    ///       so this must be done in GOAPActionInstances. ALWAYS reset the speed in OnReset() and OnEnd()
    ///       to avoid stale behaviour. Consider changing in future.
    /// </summary>
    public enum MovementSpeed
    {
        Walk = 0,
        Jog = 1,
        Sprint = 2,
    }
}

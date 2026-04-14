using UnityEngine;

namespace Player
{
    /// <summary>
    /// ScriptableObject configuration for crouch behaviour.
    /// </summary>
    [CreateAssetMenu(fileName = "CrouchSettings", menuName = "FPSController/Crouch Settings")]
    public sealed class PlayerCrouchSettings : ScriptableObject
    {
        // ───── Public properties ────────────────────────────────────────────────
        public CrouchMode Mode = CrouchMode.Toggle;
        public float StandingHeight = 1.8f;
        public float CrouchingHeight = 1.0f;
        [Range(1.0f, 20.0f)] public float CrouchSpeed = 10.0f;
        public float StandingEyeHeight = 1.6f;
        public float CrouchingEyeHeight = 0.8f;

        // ───── Nested type ────────────────────────────────────────────────
        public enum CrouchMode
        {
            Toggle,
            Hold
        }
    }
}


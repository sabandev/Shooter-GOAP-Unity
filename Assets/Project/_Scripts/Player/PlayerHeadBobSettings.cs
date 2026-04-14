using UnityEngine;

namespace Player
{
    /// <summary>
    /// ScriptableObject configuration for camera head bob.
    /// </summary>
    [CreateAssetMenu(fileName = "HeadBobSettings", menuName = "FPSController/Head Bob Settings")]
    public sealed class PlayerHeadBobSettings : ScriptableObject
    {
        // ───── Public properties ────────────────────────────────────────────────
        [Range(0.0f, 10.0f)] public float Frequency = 1.8f; // Bobs per second at full speed
        [Range(0.0f, 0.1f)] public float VerticalAmplitude = 0.02f; // Vertical height of bob in units
        [Range(0.0f, 0.1f)] public float HorizontalAmplitude = 0.01f; // Horizantal movement of bob in units
        [Range(1.0f, 20.0f)] public float BlendSpeed = 5.0f;
        [Range(0.0f, 1.0f)] public float MinSpeedThreshold = 0.1f;
    }
}
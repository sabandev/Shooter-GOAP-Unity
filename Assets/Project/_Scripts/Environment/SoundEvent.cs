using UnityEngine;

/// <summary>
/// Represents a sound event that can occur in the game world, allowing listeners
/// to determine the sound's world position if they are within the listening radius.
/// </summary>
public readonly struct SoundEvent
{
    // ───── Public properties ────────────────────────────────────────────────
    
    public readonly Vector3 Position;
    
    public readonly float Radius;
    public readonly float Timestamp;

    // ───── Constructor ────────────────────────────────────────────────
    
    public SoundEvent(Vector3 position, float radius, float timestamp)
    {
        Position = position;
        Radius = radius;
        Timestamp = timestamp;
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Registrar for all sound events that occur in the game world.
/// Responsible for instantiating sound events and providing access to them.
/// </summary>
public static class SoundEventRegistry
{
    // ───── Constants ────────────────────────────────────────────────
    
    private const float EVENT_LIFETIME = 5.0f;

    // ───── Private properties ────────────────────────────────────────────────
    
    private static readonly List<SoundEvent> _events = new ();
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => _events.Clear();

    // ───── Public methods ────────────────────────────────────────────────
    
    public static void Register(Vector3 position, float radius)
    {
        _events.Add(new SoundEvent(position, radius, Time.time));
    }
    
    public static void GetRecent(List<SoundEvent> results)
    {
        float now =  Time.time;
        results.Clear();
        
        for (int i = _events.Count - 1; i >= 0; i--)
        {
            if (now - _events[i].Timestamp > EVENT_LIFETIME)
                _events.RemoveAt(i);
            else
                results.Add(_events[i]);
        }
    }
}
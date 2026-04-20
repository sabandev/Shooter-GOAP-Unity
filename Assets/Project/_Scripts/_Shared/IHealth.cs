using UnityEngine;

/// <summary>
/// Interface for any entity with health in the game world.
/// </summary>
public interface IHealth
{
    float CurrentHealth { get; }
    float MaxHealth { get; }
    bool IsDead { get; }
    void TakeDamage(float amount);
}

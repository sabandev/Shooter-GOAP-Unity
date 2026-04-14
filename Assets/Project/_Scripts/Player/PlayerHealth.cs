using System;
using UnityEngine;

/// <summary>
/// Simple health class for the test player. Receives damage and broadcasts death.
/// TODO: Replace with more robust system when a player is implemented.
/// </summary>
public sealed class PlayerHealth : MonoBehaviour
{
    // -----Serialized properties-----
    [SerializeField] private float _maxHealth = 100.0f;

    // -----Public properties-----
    public float CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0.0f;

    public event Action<float> OnDamageReceived;
    public event Action OnDeath;

    // -----Lifecycle methods-----
    private void Awake()
    {
        CurrentHealth = _maxHealth;
    }

    // -----Public methods-----
    public void TakeDamage(float amount)
    {
        if (IsDead) { return; }

        CurrentHealth = Mathf.Max(0.0f, CurrentHealth - amount);
        OnDamageReceived?.Invoke(amount);

        Debug.Log($"[PlayerHealth] '{name}' took {amount} damage.\nHealth: {CurrentHealth}/{_maxHealth}");

        if (IsDead)
        {
            Debug.Log($"[PlayerHealth] '{name}' died.");
            OnDeath?.Invoke();
        }
    }

    public void Respawn()
    {
        CurrentHealth = _maxHealth;
    }
}

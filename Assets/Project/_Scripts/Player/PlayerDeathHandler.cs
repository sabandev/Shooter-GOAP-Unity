using UnityEngine;

namespace Player
{
    /// <summary>
    /// Responsible for handling the player's death.
    /// </summary>
    public sealed class PlayerDeathHandler : MonoBehaviour
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private MonoBehaviour[] _componentsToDisable;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void OnEnable() => _playerHealth.OnDeath += HandleDeath;
        private void OnDisable() => _playerHealth.OnDeath -= HandleDeath;

        // ───── Private methods ────────────────────────────────────────────────
        
        private void HandleDeath()
        {
            foreach (MonoBehaviour c in _componentsToDisable)
                if (c != null) { c.enabled = false; }
        }
    }
}
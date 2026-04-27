using UnityEngine;
using UnityEngine.AI;
                       
namespace GOAP                                                                                                                                                                                                                    
{           
    /// <summary>
    /// Responsible for handling the agent's death.
    /// </summary>
    [RequireComponent(typeof(AIHealth))]                                                                                                                                                                                        
    public sealed class AgentDeathHandler : MonoBehaviour
    {
        // ───── Private properties ────────────────────────────────────────────────
        
        private AIHealth _health;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void Awake() => _health = GetComponent<AIHealth>();
        private void OnEnable()  => _health.OnDeath += HandleDeath;
        private void OnDisable() => _health.OnDeath -= HandleDeath;

        // ───── Private methods ────────────────────────────────────────────────
        
        private void HandleDeath()                                                                                                                                                                                              
        {
            foreach (Sensor sensor in GetComponentsInChildren<Sensor>())
                sensor.enabled = false;
            
            if (TryGetComponent(out GOAPAgent agent))                                                                                                                                                                           
              agent.enabled = false;
                                                                                                                                                                                                                              
            if (TryGetComponent(out NavMeshAgent nav))
              nav.enabled = false;                                                                                                                                                                                            
        }       
    }
}
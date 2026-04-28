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
        
        private void HandleDeath(Vector3 hitPoint, Vector3 hitDirection, float force)                                                                                                                                                                                              
        {
            foreach (Sensor sensor in GetComponentsInChildren<Sensor>())
                sensor.enabled = false;
            
            if (TryGetComponent(out GOAPAgent agent))                                                                                                                                                                           
              agent.enabled = false;
                                  
            Vector3 deathVelocity = Vector3.zero;
            if (TryGetComponent(out NavMeshAgent nav))
            {
                deathVelocity = nav.velocity;
                nav.enabled = false;      
            }
            
            if (TryGetComponent(out AIRagdoll ragdoll))
                ragdoll.EnableRagdoll(deathVelocity, hitPoint, hitDirection, force);
        }       
    }
}
using UnityEngine;

namespace GOAP
{               
    /// <summary>
    /// Responsible for managing the agent's ragdoll.
    /// </summary>
    public sealed class AIRagdoll : MonoBehaviour
    {
        // ───── Serialized properties ────────────────────────────────────────────────

        [Header("References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _hipsRoot; // Top of ragdoll hierarchy

        // ───── Private properties ────────────────────────────────────────────────
        
        private Rigidbody[] _bones;
        private Collider[] _boneColliders;                                                                                                                                                                                     
        private Collider _mainCollider; // Root Capsule Collider

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void Awake()
        {                                                                                                                                                                                                                       
            _bones = _hipsRoot.GetComponentsInChildren<Rigidbody>(true);
            _boneColliders = _hipsRoot.GetComponentsInChildren<Collider>(true);
            _mainCollider = GetComponent<Collider>();                                                                                                                                                                          

            SetPhysicsEnabled(false);                                                                                                                                                                                           
        }

        // ───── Public methods ────────────────────────────────────────────────

        public void EnableRagdoll(Vector3 inheritedVelocity, Vector3 hitPoint, Vector3 hitDirection, float force)                                                                                                                                                          
        {
            if (_mainCollider != null)                                                                                                                                                                                          
              _mainCollider.enabled = false;

            _animator.enabled = false;
            SetPhysicsEnabled(true);

            foreach (Rigidbody bone in _bones)
              bone.linearVelocity =  inheritedVelocity;

            Rigidbody hitBone = GetNearestBone(hitPoint);
            if (hitBone != null)
              hitBone.AddForceAtPosition(hitDirection.normalized * force, hitPoint, ForceMode.Impulse);
        }

        // ───── Private methods ────────────────────────────────────────────────

        private Rigidbody GetNearestBone(Vector3 point)
        {
            Rigidbody nearest = null;
            float minDist =  float.MaxValue;
            
            foreach (Rigidbody bone in _bones)
            {
                float dist = Vector3.Distance(bone.position, point);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = bone;
                }
            }
            
            return nearest;
        }
        
        private void SetPhysicsEnabled(bool value)
        {
            foreach (Rigidbody bone in _bones)                                                                                                                                                                                  
              bone.isKinematic = !value;
                                                                                                                                                                                                                              
            foreach (Collider col in _boneColliders) // only disable non-trigger bone colliders so hitboxes stay active
                if (!col.isTrigger) { col.enabled = value; }
        }
    }
}

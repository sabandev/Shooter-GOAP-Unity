using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons
{
    /// <summary>
    /// Manages behaviour for spawned bullet debris.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PooledObject))]
    public sealed class BulletDebris : MonoBehaviour
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("Pooling")]
        [SerializeField] [Min(1)] private int _maxCount = 20;
        
        [Space(10.0f)]
        
        [Header("Debris Properties")]
        [SerializeField] [Min(0.01f)] private float _outwardForce = 3.0f;
        [SerializeField] [Min(0.0f)] private float _upwardBias = 1.5f;
        [SerializeField] [Min(0.0f)] private float _randomness = 1.5f;
        [SerializeField] [Min(0.0f)] private float _torque = 4.0f;

        // ───── Private properties ────────────────────────────────────────────────
        
        private static readonly List<BulletDebris> _activeBulletDebris = new List<BulletDebris>();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState() => _activeBulletDebris.Clear();
        
        private Rigidbody _rigidbody;
        private PooledObject _pooledObject;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _pooledObject = GetComponent<PooledObject>();
            
            Debug.Assert(_rigidbody != null, "[BulletDebris] Rigidbody is null.", this);
            Debug.Assert(_pooledObject != null, "[BulletDebris] PooledObject is null.", this);
        }
        
        private void OnEnable()
        {
            _activeBulletDebris.Add(this);
        }
            
        private void OnDisable()
        {
            _activeBulletDebris.Remove(this);
        }

        // ───── Public methods ────────────────────────────────────────────────
        
        public void Launch(Vector3 surfaceNormal)
        {
            EnforceMaxCount();
            
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            
            Vector3 force = surfaceNormal * _outwardForce + Vector3.up * _upwardBias + Random.insideUnitSphere * _randomness;
            
            _rigidbody.AddForce(force, ForceMode.Impulse);
            _rigidbody.AddTorque(Random.insideUnitSphere * _torque, ForceMode.Impulse);
        }

        // ───── Private methods ────────────────────────────────────────────────
        
        private void EnforceMaxCount()
        {
            while (_activeBulletDebris.Count > _maxCount)
            {
                BulletDebris oldest = _activeBulletDebris[0];
                
                if (oldest._pooledObject != null)
                    oldest._pooledObject.ReturnNow();
                else
                    _activeBulletDebris.RemoveAt(0);
            }
        }
    }
}

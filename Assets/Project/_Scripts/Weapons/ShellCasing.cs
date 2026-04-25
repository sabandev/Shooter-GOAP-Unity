using UnityEngine;

namespace Weapons
{
    /// <summary>
    /// Shell casing ejected on fire.
    /// Spawned at ShellEjectPoint with random impulse.
    /// Stays in world until PooledObject lifetime expires.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PooledObject))]
    public sealed class ShellCasing : MonoBehaviour
    {
        [SerializeField] private float _ejectForce  = 3.0f;
        [SerializeField] private float _ejectTorque = 5.0f;
        [SerializeField] private float _upwardBias  = 0.5f;

        private Rigidbody _rigidbody;

        private void Awake() => _rigidbody = GetComponent<Rigidbody>();

        public void Eject(Vector3 ejectDirection)
        {
            _rigidbody.linearVelocity  = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            Vector3 force =
                (ejectDirection + Vector3.up * _upwardBias).normalized * _ejectForce + Random.insideUnitSphere * 0.5f;

            _rigidbody.AddForce(force, ForceMode.Impulse);
            _rigidbody.AddTorque(Random.insideUnitSphere * _ejectTorque, ForceMode.Impulse);
            // AUDIO: shell casing bounce sound here
        }
    }
}
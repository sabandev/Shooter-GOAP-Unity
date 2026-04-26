using UnityEngine;

/// <summary>
/// Used for any 3D prop that reacts to impacts.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public sealed class PropPhysics : MonoBehaviour, IImpactReceiver
{
    // ───── Serialized properties ────────────────────────────────────────────────
    
    [SerializeField] [Min(0.0f)] private float _impactMultiplier = 1.0f;

    // ───── Private properties ────────────────────────────────────────────────
    
    private Rigidbody _rb;

    // ───── Lifecycle methods ────────────────────────────────────────────────
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
    }

    // ───── Implementation ────────────────────────────────────────────────
    
    public void ReceiveImpact(Vector3 point, Vector3 direction, float force)
    {
        _rb.isKinematic = false;
        _rb.AddForceAtPosition(direction * (force * _impactMultiplier), point, ForceMode.Impulse);
    }
}

using UnityEngine;

/// <summary>
/// Universal impact receiver contract.
/// Will apply a force at a given point in a given direction.
/// </summary>
public interface IImpactReceiver
{
    void ReceiveImpact(Vector3 point, Vector3 direction, float force);
}

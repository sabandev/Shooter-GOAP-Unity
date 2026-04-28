using UnityEngine;

/// <summary>
/// Global interface that can provide context to a given damage unit.
/// Allows direction and force to passed into damage as information to be
/// interpreted by the recipient (e.g. AI)
/// </summary>
public interface IDamageContext
{
    void SetHitContext(Vector3 hitPoint, Vector3 hitDirection, float force);
}

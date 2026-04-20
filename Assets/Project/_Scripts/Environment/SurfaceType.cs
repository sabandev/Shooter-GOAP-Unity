using UnityEngine;

/// <summary>
/// Tags a surface for impact particle and audio selection.
/// Add to any environment object weapons can hit.
/// </summary>
public sealed class SurfaceType : MonoBehaviour
{
    public enum Surface
    {
        Default,
        Metal,
        Wood,
        Concrete,
        Flesh,
        Glass
    }

    [SerializeField] private Surface _surface = Surface.Default;
    public Surface Type => _surface;
}
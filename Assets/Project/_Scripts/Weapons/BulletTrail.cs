using UnityEngine;

/// <summary>
/// Animates a bullet trail from muzzle to impact.
/// </summary>
[RequireComponent(typeof(TrailRenderer))]
[RequireComponent(typeof(PooledObject))]
public sealed class BulletTrail : MonoBehaviour
{
    // ───── Serialized properties ────────────────────────────────────────────────
    
    [SerializeField] private float _travelSpeed = 80.0f;

    // ───── Private properties ────────────────────────────────────────────────
    
    private TrailRenderer _trailRenderer;
    private PooledObject _pooled;
    private Vector3      _start;
    private Vector3      _end;
    private float        _totalDistance;
    private float        _travelled;
    private bool _travelling;

    private void Awake()
    {
        _trailRenderer = GetComponent<TrailRenderer>();
        _pooled       = GetComponent<PooledObject>();
        
        Debug.Assert(_pooled != null, "[BulletTrail]: Pooled object is null.", this);
        Debug.Assert(_trailRenderer != null, "[BulletTrail]: TrailRenderer is null.", this);
    }

    public void Initialise(Vector3 start, Vector3 end)
    {
        _start = start;
        _end = end;
        _totalDistance = Vector3.Distance(start, end);
        _travelled = 0.0f;
        _travelling = true;
        
        _trailRenderer.Clear();

        _trailRenderer.emitting = false;
        transform.position = start;
        _trailRenderer.emitting = true;
    }

    private void Update()
    {
        if (!_travelling) { return; }

        _travelled += _travelSpeed * Time.deltaTime;

        float t = Mathf.Clamp01(_travelled / _totalDistance);
        
        transform.position = Vector3.Lerp(_start, _end, t);
        
        if (t >= 1.0f)
        {
            _travelling = false;
            _trailRenderer.emitting = false;
            
            Invoke(nameof(ReturnToPool), _trailRenderer.time + 0.05f);
        }
    }
    
    private void ReturnToPool()
    {
        _trailRenderer.Clear();
        _pooled.ReturnNow();
    }
}
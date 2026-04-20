using UnityEngine;

/// <summary>
/// Required on every pooled prefab.
/// Handles automatic return after lifetime.
/// </summary>
public sealed class PooledObject : MonoBehaviour
{
    [SerializeField] private float _lifetime = 3.0f;

    public ObjectPool Pool  { get; set; }

    private float _timer;

    public void OnSpawn()    => _timer = 0.0f;
    public void ReturnNow()  => Pool?.Return(this);

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _lifetime) Pool?.Return(this);
    }
}
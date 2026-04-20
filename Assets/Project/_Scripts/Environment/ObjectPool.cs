using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool for frequent spawn/despawn.
/// Used for bullet trails, shell casings, muzzle flash, and impact particles.
/// </summary>
public sealed class ObjectPool : MonoBehaviour
{
    // ───── Serialized properties ────────────────────────────────────────────────
    
    [SerializeField] private GameObject _prefab;
    [SerializeField] private int        _initialSize = 10;

    // ───── Private properties ────────────────────────────────────────────────
    
    private readonly Queue<PooledObject> _available = new Queue<PooledObject>();
    private static readonly Dictionary<GameObject, ObjectPool> _pools = new Dictionary<GameObject, ObjectPool>();

    // ───── Static Runtime Initalization ────────────────────────────────────────────────
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState() => _pools.Clear();

    // ───── Lifecycle methods ────────────────────────────────────────────────
    
    private void Awake()
    {
        if (_prefab == null) { return; }
        
        for (int i = 0; i < _initialSize; i++)
            CreateInstance();
    }

    private void OnEnable()
    {
        if (_prefab != null) _pools[_prefab] = this;
    }

    private void OnDisable()
    {
        if (_prefab != null) _pools.Remove(_prefab);
    }

    // ───── Public methods ────────────────────────────────────────────────
    
    public static PooledObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) { return null; }
        
        if (!_pools.TryGetValue(prefab, out ObjectPool pool))
        {
            GameObject go = new GameObject($"Pool_{prefab.name}");
            pool          = go.AddComponent<ObjectPool>();
            pool._prefab  = prefab;
            pool.Initialize(prefab, 10);
            _pools[prefab] = pool;
        }

        return pool.GetInstance(position, rotation);
    }

    public void Return(PooledObject obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform);
        _available.Enqueue(obj);
    }

    // ───── Private methods ────────────────────────────────────────────────
    
    private void Initialize(GameObject prefab, int initialSize)
    {
        _prefab = prefab;
        for (int i = 0; i < initialSize; i++)
            CreateInstance();
    }
    
    private PooledObject GetInstance(Vector3 pos, Quaternion rot)
    {
        PooledObject obj = _available.Count > 0 ? _available.Dequeue() : CreateInstance();

        obj.transform.SetParent(null);
        obj.transform.SetPositionAndRotation(pos, rot);
        obj.gameObject.SetActive(true);
        obj.OnSpawn();
        return obj;
    }


    private PooledObject CreateInstance()
    {
        GameObject go = Instantiate(_prefab, transform);
        go.SetActive(false);

        PooledObject pooled = go.GetComponent<PooledObject>() ?? go.AddComponent<PooledObject>();

        pooled.Pool = this;
        return pooled;
    }
}
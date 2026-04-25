using UnityEngine;
using Unity.Profiling;

/// <summary>
/// A diagnostic tool used to warn if the GC Allocation budget is exceeded during play time.
/// Used to spot memory leaks and poor programming practice resulting in high memory spikes.
/// </summary>
public sealed class MemoryBudgetMonitor : MonoBehaviour
{
    // ───── Serialized properties ────────────────────────────────────────────────
    
    [SerializeField] [Min(64)] private long _gcAllocBudgetBytes = 8192;
    [SerializeField] [Min(4)] private long _totalMemoryBudgetMB = 256;
    [SerializeField] [Min(1)] private int _logIntervalFrames = 60; // Debug.Log the heap size every x frames
    [SerializeField] private bool _monitor = false;

    // ───── Private properties ────────────────────────────────────────────────
    
    private ProfilerRecorder _gcAllocRecorder;
    private ProfilerRecorder _totalMemoryRecorder;
    private int _frameCounter;

    // ───── Lifecycle methods ────────────────────────────────────────────────
    
    #if DEVELOPMENT_BUILD || UNITY_EDITOR
    private void OnEnable()
    {
        _gcAllocRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC.Alloc");
        _totalMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
    }
    
    private void OnDisable()
    {
        _gcAllocRecorder.Dispose();
        _totalMemoryRecorder.Dispose();
    }
    
    private void Update()
    {
        // Prevent from running during editor play-tests, clogs up console
        if (!_monitor) { return; }
        
        long gcAlloc = _gcAllocRecorder.LastValue;
        if (gcAlloc > _gcAllocBudgetBytes)
            Debug.LogWarning($"[MemoryBudgetMonitor] GC.Alloc: {gcAlloc}B this frame exceeds limit: {_gcAllocBudgetBytes}B.");
        
        _frameCounter++;
        
        if (_frameCounter % _logIntervalFrames == 0)
        {
            long totalMB = _totalMemoryRecorder.LastValue / (1024 * 1024);
            if (totalMB > _totalMemoryBudgetMB)
                Debug.LogWarning($"[MemoryBudgetMonitor] Memory Budget: {totalMB}MB exceeds limit: {_totalMemoryBudgetMB}MB.");
        }
    }
    #endif
}

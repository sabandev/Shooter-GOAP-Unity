using TMPro;
using UnityEngine;

/// <summary>
/// Displays an FPS counter. Updates on an interval to prevent flickering.
/// </summary>
public sealed class FPSCounter : MonoBehaviour
{
    // ───── Serialized properties ────────────────────────────────────────────────
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _updateInterval = 0.5f;

    // ───── Private properties ────────────────────────────────────────────────
    private float _timer;
    private int _frameCount;
    private float _fps;

    private void Update()
    {
        _frameCount++;
        _timer += Time.unscaledDeltaTime;
        
        if (_timer < _updateInterval) { return; }
        
        _fps = _frameCount / _timer;
        _frameCount = 0;
        _timer = 0.0f;
        
        _text.text = $"FPS: {_fps:F0}";
    }
}

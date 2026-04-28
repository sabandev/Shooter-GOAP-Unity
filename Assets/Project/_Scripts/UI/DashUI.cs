using UnityEngine;
using UnityEngine.UI;
using Player;

namespace UI
{
    /// <summary>
    /// Responsible for displaying player dash progress
    /// </summary>
    public sealed class DashUI : MonoBehaviour
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("References")]
        [SerializeField] private PlayerDashController _playerDashController;
        [SerializeField] private Image _barImage;
        
        [Space(10.0f)]
        
        [Header("Colors")]
        [SerializeField] private Material _chargingColor;
        [SerializeField] private Material _readyColor;

        // ───── Lifecycle methods ────────────────────────────────────────────────

        private void Awake()
        {
            Debug.Assert(_playerDashController != null, "[DashUI] PlayerDashController not assigned", this);
            Debug.Assert(_barImage != null, "[DashUI] Bar image not assigned", this);
            Debug.Assert(_chargingColor != null, "[DashUI] Charging color not assigned", this);
            Debug.Assert(_readyColor != null, "[DashUI] Ready color not assigned", this);
        }
        
        private void Update()
        {
            _barImage.fillAmount = _playerDashController.CooldownProgress;
            _barImage.material = _playerDashController.DashReady ? _readyColor : _chargingColor;
        }
    }
}

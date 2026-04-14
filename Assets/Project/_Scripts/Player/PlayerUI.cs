using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Player
{
    /// <summary>
    /// Player UI display.
    /// </summary>
    public sealed class HealthUI : MonoBehaviour
    {
        // ─── Serialized fields ────────────────────────────────────────

        [Header("References")]
        [SerializeField] private PlayerHealth    _health;
        [SerializeField] private PlayerInventory _inventory;

        [Header("Health Display")]
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private Image           _panelBorder;
        [SerializeField] private Image           _panelBackground;
        [SerializeField] private Image           _medicalIcon;

        [Header("Syringe Display")]
        [SerializeField] private TextMeshProUGUI _syringeCountText;
        [SerializeField] private Image           _syringeIcon;

        [Header("Colours")]
        [SerializeField] private Color _normalBorderColour =
            new Color(0.0f, 0.8f, 1.0f, 0.8f);

        [SerializeField] private Color _lowHealthBorderColour =
            new Color(1.0f, 0.1f, 0.1f, 0.8f);

        [SerializeField] private Color _damageFlashColour =
            new Color(1.0f, 0.0f, 0.0f, 0.5f);

        [Header("Animation")]
        [Tooltip("Speed of the damage flash fade.")]
        [SerializeField] private float _flashFadeSpeed = 4.0f;

        [Tooltip("Speed of the low health pulse.")]
        [SerializeField] private float _pulseSpeed = 2.0f;

        [Tooltip("Pulse intensity — how much the border alpha varies.")]
        [SerializeField] [Range(0.0f, 1.0f)]
        private float _pulseIntensity = 0.4f;

        // ─── Private state ────────────────────────────────────────────

        private Color _currentFlashColour;
        private bool  _isLowHealth;
        private float _pulseTimer;

        // ─── Unity lifecycle ──────────────────────────────────────────

        private void Awake()
        {
            Debug.Assert(_health    != null,
                "[HealthUI] Missing PlayerHealth.",    this);
            Debug.Assert(_inventory != null,
                "[HealthUI] Missing PlayerInventory.", this);

            _currentFlashColour = Color.clear;
        }

        private void OnEnable()
        {
            _health.OnHealthChanged        += UpdateHealthDisplay;
            _health.OnDamageReceived       += OnDamageReceived;
            _health.OnLowHealthStateChanged += OnLowHealthStateChanged;
            _inventory.OnHealthKitCountChanged += UpdateSyringeDisplay;
        }

        private void OnDisable()
        {
            _health.OnHealthChanged         -= UpdateHealthDisplay;
            _health.OnDamageReceived        -= OnDamageReceived;
            _health.OnLowHealthStateChanged -= OnLowHealthStateChanged;
            _inventory.OnHealthKitCountChanged -= UpdateSyringeDisplay;
        }

        private void Start()
        {
            // Initialise displays
            UpdateHealthDisplay(_health.CurrentHealth);
            UpdateSyringeDisplay(_inventory.HealthKitCount);
        }

        private void Update()
        {
            UpdateDamageFlash();
            UpdateLowHealthPulse();
        }

        // ─── Private methods ──────────────────────────────────────────

        private void UpdateHealthDisplay(float health)
        {
            if (_healthText != null)
                _healthText.text = Mathf.CeilToInt(health).ToString();
        }

        private void UpdateSyringeDisplay(int count)
        {
            if (_syringeCountText != null)
                _syringeCountText.text = count.ToString();

            // Dim syringe icon when empty
            if (_syringeIcon != null)
            {
                Color c = _syringeIcon.color;
                c.a = count > 0 ? 1.0f : 0.3f;
                _syringeIcon.color = c;
            }
        }

        private void OnDamageReceived(float amount)
        {
            // Trigger red flash
            _currentFlashColour = _damageFlashColour;

            if (_panelBackground != null)
                _panelBackground.color = _currentFlashColour;
        }

        private void OnLowHealthStateChanged(bool isLow)
        {
            _isLowHealth = isLow;

            if (!isLow && _panelBorder != null)
                _panelBorder.color = _normalBorderColour;
        }

        private void UpdateDamageFlash()
        {
            if (_currentFlashColour.a <= 0.0f) { return; }

            _currentFlashColour.a = Mathf.MoveTowards(
                _currentFlashColour.a,
                0.0f,
                Time.deltaTime * _flashFadeSpeed);

            if (_panelBackground != null)
                _panelBackground.color = _currentFlashColour;
        }

        private void UpdateLowHealthPulse()
        {
            if (!_isLowHealth || _panelBorder == null) { return; }

            _pulseTimer += Time.deltaTime * _pulseSpeed;

            float pulse = (Mathf.Sin(_pulseTimer * Mathf.PI * 2.0f)
                          * 0.5f + 0.5f) * _pulseIntensity;

            Color pulseColour = _lowHealthBorderColour;
            pulseColour.a     = _lowHealthBorderColour.a - pulse;

            _panelBorder.color = pulseColour;
        }
    }
}
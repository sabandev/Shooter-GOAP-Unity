using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Player
{
    /// <summary>
    /// Screen-space interaction prompt. Shows a hand icon and
    /// optional text when the player looks at an interactable.
    ///
    /// Subscribes to PlayerInteraction events rather than
    /// polling — only updates when interactable state changes.
    /// </summary>
    public sealed class InteractionPrompt : MonoBehaviour
    {
        // ─── Serialized fields ────────────────────────────────────────

        [Header("References")]
        [SerializeField] private PlayerInteraction _interaction;

        [Header("UI Elements")]
        [SerializeField] private GameObject      _promptRoot;
        [SerializeField] private Image           _handIcon;
        [SerializeField] private TextMeshProUGUI _promptText;

        [Header("Animation")]
        [Tooltip("Speed at which the prompt fades in and out.")]
        [SerializeField] private float _fadeSpeed = 8.0f;

        // ─── Private state ────────────────────────────────────────────

        private CanvasGroup _canvasGroup;
        private float       _targetAlpha;

        // ─── Unity lifecycle ──────────────────────────────────────────

        private void Awake()
        {
            Debug.Assert(_interaction != null,
                "[InteractionPrompt] Missing PlayerInteraction.", this);

            _canvasGroup = _promptRoot.GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
                _canvasGroup = _promptRoot.AddComponent<CanvasGroup>();

            _canvasGroup.alpha = 0.0f;
            _targetAlpha       = 0.0f;
        }

        private void OnEnable()
        {
            _interaction.OnInteractableFound += ShowPrompt;
            _interaction.OnInteractableLost  += HidePrompt;
        }

        private void OnDisable()
        {
            _interaction.OnInteractableFound -= ShowPrompt;
            _interaction.OnInteractableLost  -= HidePrompt;
        }

        private void Update()
        {
            if (Mathf.Approximately(_canvasGroup.alpha, _targetAlpha))
                { return; }

            _canvasGroup.alpha = Mathf.MoveTowards(
                _canvasGroup.alpha,
                _targetAlpha,
                Time.deltaTime * _fadeSpeed);
        }

        // ─── Private methods ──────────────────────────────────────────

        private void ShowPrompt(string promptText)
        {
            if (_promptText != null)
                _promptText.text = promptText;

            _targetAlpha = 1.0f;
        }

        private void HidePrompt()
        {
            _targetAlpha = 0.0f;
        }
    }
}
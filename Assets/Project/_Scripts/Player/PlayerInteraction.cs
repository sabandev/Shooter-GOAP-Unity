using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    /// <summary>
    /// Handles player interaction with any GameObject of type IInteractable.
    /// </summary>
    public sealed class PlayerInteraction : MonoBehaviour
    {
        // ───── Serialized properties ────────────────────────────────────────────────
        [Header("Detection")]
        [SerializeField] private Transform _camera;
        [SerializeField] private float _interactionRange = 2.5f;
        [SerializeField] private LayerMask _interactionMask;

        // ───── Public properties ────────────────────────────────────────────────
        public event Action<string> OnInteractableFound;
        public event Action OnInteractableLost;

        // ───── Private properties ────────────────────────────────────────────────
        private PlayerInputActions _input;
        private IInteractable _currentInteractable;
        private GameObject _currentInteractableGameObject;

        // ───── Lifecycle methods ────────────────────────────────────────────────
        private void Awake()
        {
            Debug.Assert(_camera != null, "[PlayerInteraction] Camera not assigned.", this);
            _input = new PlayerInputActions();
        }
        
        private void OnEnable()
        {
            _input.Player.Enable();
            _input.Player.Interact.performed += OnInteractPerformed;
        }
        
        private void OnDisable()
        {
            _input.Player.Interact.performed -= OnInteractPerformed;
            _input.Player.Disable();
        }
        
        private void Update()
        {
            DetectInteractable();
        }

        // ───── Private methods ────────────────────────────────────────────────
        private void DetectInteractable()
        {
            if (Physics.Raycast(_camera.position, _camera.forward, out RaycastHit hit, _interactionRange, _interactionMask))
            {
                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null && interactable.CanInteract)
                {
                    if (interactable != _currentInteractable)
                    {
                        _currentInteractable = interactable;
                        _currentInteractableGameObject = hit.collider.gameObject;
                        
                        OnInteractableFound?.Invoke(interactable.InteractionPrompt);
                    }
                    return;
                }
            }
            
            // Nothing found
            if (_currentInteractable != null)
            {
                _currentInteractable = null;
                _currentInteractableGameObject = null;
                OnInteractableLost?.Invoke();
            }
        }
        
        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (_currentInteractable == null) { return; }
            if (!_currentInteractable.CanInteract) { return; }
            
            _currentInteractable.Interact(gameObject);
        }
    }
}


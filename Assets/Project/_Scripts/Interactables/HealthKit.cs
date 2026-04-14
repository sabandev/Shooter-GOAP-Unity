using GOAP;
using Player;
using UnityEngine;

/// <summary>
/// HealthKit pickup. Implements IInteractable for player use and can be interacted with
/// through SmartObject for AI use.
/// </summary>
[RequireComponent(typeof(SmartObject))]
public sealed class HealthKit : MonoBehaviour, IInteractable
{
    // ───── Serialized properties ────────────────────────────────────────────────
    [SerializeField] private GameObject _visualMesh; // Hides when consumed

    // ───── Private properties ────────────────────────────────────────────────
    private SmartObject _smartObject;
    private bool _consumed;

    // ───── Implementation ────────────────────────────────────────────────
    public string InteractionPrompt => "Take Health Kit";
    public bool CanInteract => !_consumed;
    
    public void Interact(GameObject interactor)
    {
        if (_consumed) { return; }
        
        PlayerInventory inventory = interactor.GetComponent<PlayerInventory>();
        
        if (inventory != null)
        {
            inventory.AddHealthKit();
            Consume();
            return;
        }
        
        // TODO: Add AI interaction here
    }

    // ───── Public methods ────────────────────────────────────────────────
    public void Consume()
    {
        if (_consumed) { return; }
        
        _consumed = true;
        
        if (_visualMesh != null)
            _visualMesh.SetActive(false);
        
        _smartObject?.SetPermanentlyUnavailable();
        
        Debug.Log("[HealthKit] Consumed.");
    }

    // ───── Lifecycle methods ────────────────────────────────────────────────
    private void Awake()
    {
        _smartObject = GetComponent<SmartObject>();
    }
}

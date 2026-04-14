using UnityEngine;

/// <summary>
/// Interface implemented by all interactable GameObjects for the player and AI.
/// </summary>
public interface IInteractable
{
    // ───── Properties ────────────────────────────────────────────────
    string InteractionPrompt { get; }
    bool CanInteract { get; }

    // ───── Methods ────────────────────────────────────────────────
    void Interact(GameObject interactor);
}

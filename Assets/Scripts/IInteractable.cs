public interface IInteractable
{
    void Interact();  // Method to define what happens when interacted with
    void ShowUI();    // Method to show any specific UI for interaction
    void HideUI();    // Method to hide the UI
    bool IsInteractable();  // Determine if the object is currently interactable
}
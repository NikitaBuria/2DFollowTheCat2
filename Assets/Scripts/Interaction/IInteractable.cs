// Anything the player can use with the interact key: NPCs, doors, the merchant, buildings.
// Gets the Player so each interactable takes what it needs (Wallet, Inventory, Health...).
// Ported from MetrooDvania (which passed only the Inventory).
public interface IInteractable
{
    // False when the interaction is refused right now (e.g. not enough coins); the prompt still shows why.
    bool CanInteract(Player player);

    // Text for the prompt above the object, e.g. "Talk" or "Restore (50 coins)".
    string GetPrompt(Player player);

    void Interact(Player player);
}

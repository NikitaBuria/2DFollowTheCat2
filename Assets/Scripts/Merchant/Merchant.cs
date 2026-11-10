using UnityEngine;

// The merchant NPC. Interacting only asks GameStateManager for the Shop state; it never touches
// the UI. UIManager shows the merchant and inventory panels, and the menu or inventory key closes the shop.
public class Merchant : MonoBehaviour, IInteractable
{
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private string tradePrompt = "Trade";

    public bool CanInteract(Player player) => true;

    public string GetPrompt(Player player) => tradePrompt;

    public void Interact(Player player)
    {
        gameStateManager.ChangeState(GameState.Shop);
    }
}

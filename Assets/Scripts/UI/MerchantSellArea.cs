using UnityEngine;
using UnityEngine.EventSystems;

// The drop zone on the merchant panel: drop an inventory slot here to sell the whole stack.
// Only reports to the Merchant; Inventory.OnChanged and Wallet.OnMoneyChanged redraw the UI.
// Needs a Graphic with Raycast Target on (e.g. an Image) or it never receives drops.
public class MerchantSellArea : MonoBehaviour, IDropHandler
{
    [SerializeField] private Merchant merchant;

    // Runs before the dragged slot's OnEndDrag, so InventoryUI still cancels the drag afterwards.
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return; // right-drag = split, not a sale
        if (eventData.pointerDrag == null) return;
        if (!eventData.pointerDrag.TryGetComponent(out InventorySlotUI slotView)) return;

        merchant.TrySell(slotView.index);
    }
}

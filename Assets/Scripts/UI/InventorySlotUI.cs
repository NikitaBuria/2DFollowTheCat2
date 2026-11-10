using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Draws one inventory slot. In the inventory grid it also reports drags to its owner
// (InventoryUI), which does the actual move. Views without an owner (hotbar) ignore drags.
public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField, Range(0f, 1f)] private float draggingAlpha = 0.25f;

    private InventoryUI owner;

    public int index { get; private set; } = -1;

    public void Setup(InventoryUI newOwner, int newIndex)
    {
        owner = newOwner;
        index = newIndex;
    }

    public void Show(InventorySlot slot)
    {
        icon.color = Color.white;

        if (slot == null || slot.isEmpty)
        {
            amountText.SetText("");
            icon.enabled = false;
        }
        else
        {
            icon.sprite = slot.item.icon;
            icon.enabled = true;

            if (slot.amount > 1)
            {
                amountText.SetText("{0}", slot.amount);
            }
            else
            {
                amountText.SetText("");
            }
        }
    }

    // Fades the icon left behind while its item is being dragged.
    public void SetDragging(bool dragging)
    {
        icon.color = dragging ? new Color(1f, 1f, 1f, draggingAlpha) : Color.white;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (owner == null) return;

        bool left = eventData.button == PointerEventData.InputButton.Left;
        bool right = eventData.button == PointerEventData.InputButton.Right;
        if (!left && !right) return;

        owner.BeginDrag(index, right, eventData.position); // right-drag splits the stack
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (owner == null) return;

        owner.UpdateDrag(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (owner == null) return;

        GameObject hovered = eventData.pointerCurrentRaycast.gameObject;
        InventorySlotUI target = hovered == null ? null : hovered.GetComponentInParent<InventorySlotUI>();

        owner.EndDrag(target == null ? -1 : target.index);
    }
}

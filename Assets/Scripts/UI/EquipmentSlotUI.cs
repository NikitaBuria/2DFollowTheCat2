using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// One equipment box (Helmet, Armor, Weapon, Boots) in the inventory panel. Shows what Equipment
// wears in its slot and reports drags and drops to its owner (InventoryUI), which does the move.
// Needs the root Image with Raycast Target on, or it never receives drops.
public class EquipmentSlotUI : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private EquipmentSlotType slotType;
    [SerializeField] private Equipment equipment;
    [SerializeField] private InventoryUI owner;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject placeholder;
    [SerializeField, Range(0f, 1f)] private float draggingAlpha = 0.25f;

    public EquipmentSlotType slot => slotType;

    private void OnEnable()
    {
        equipment.OnEquipmentChanged += HandleEquipmentChanged;
        Show(equipment.GetEquipped(slotType));
    }

    private void OnDisable()
    {
        equipment.OnEquipmentChanged -= HandleEquipmentChanged;
    }

    private void HandleEquipmentChanged(EquipmentSlotType changedSlot, EquipmentData item)
    {
        if (changedSlot == slotType)
            Show(item);
    }

    private void Show(EquipmentData item)
    {
        bool hasItem = item != null;

        icon.gameObject.SetActive(hasItem);
        placeholder.SetActive(!hasItem);
        icon.sprite = hasItem ? item.icon : null;
        icon.enabled = hasItem && item.icon != null;
        icon.color = Color.white;
    }

    // Fades the icon left behind while it is being dragged.
    public void SetDragging(bool dragging)
    {
        icon.color = dragging ? new Color(1f, 1f, 1f, draggingAlpha) : Color.white;
    }

    // An inventory item dropped here. Runs before the dragged slot's OnEndDrag.
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        owner.DropOnEquipment(slotType);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        owner.BeginEquipmentDrag(this, eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        owner.UpdateDrag(eventData.position);
    }

    // Dropped on an inventory slot = take it off into that slot; anywhere else = stays worn.
    public void OnEndDrag(PointerEventData eventData)
    {
        GameObject hovered = eventData.pointerCurrentRaycast.gameObject;
        InventorySlotUI target = hovered == null ? null : hovered.GetComponentInParent<InventorySlotUI>();

        owner.EndEquipmentDrag(target == null ? -1 : target.index);
    }
}

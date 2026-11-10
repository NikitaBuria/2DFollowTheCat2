using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Draws the inventory grid slots from Inventory.OnChanged and runs drag and drop between them.
// Visibility is UIManager's job.
public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private Transform slotsParent;

    [Header("Drag Preview")]
    [SerializeField] private RectTransform dragPreview;
    [SerializeField] private Image dragIcon;
    [SerializeField] private TextMeshProUGUI dragAmountText;

    private readonly List<InventorySlotUI> slotViews = new();
    private int draggedSlot = -1;
    private int draggedAmount;
    private bool splitDrag;

    private void OnEnable()
    {
        inventory.OnChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        inventory.OnChanged -= Refresh;
        CancelDrag(); // the panel can close (Tab/Escape) in the middle of a drag
    }

    // Start, not Awake: every Awake (including Inventory's, which creates its slots) has run by now.
    private void Start()
    {
        dragPreview.gameObject.SetActive(false);
        BuildSlots();
        Refresh();
    }

    private void BuildSlots()
    {
        for (int i = 0; i < inventory.count; i++)
        {
            InventorySlotUI view = Instantiate(slotPrefab, slotsParent);
            view.Setup(this, i);
            slotViews.Add(view);
        }
    }

    // Loops over the built views, so it is safe when OnEnable runs before Start (empty list).
    private void Refresh()
    {
        for (int i = 0; i < slotViews.Count; i++)
        {
            slotViews[i].Show(inventory.GetSlot(i));
        }
    }

    // split: right-drag, carries half the stack (rounded up). A single item just moves.
    public void BeginDrag(int index, bool split, Vector2 screenPosition)
    {
        InventorySlot slot = inventory.GetSlot(index);
        if (slot == null || slot.isEmpty) return;

        draggedSlot = index;
        draggedAmount = split && slot.amount > 1 ? Mathf.CeilToInt(slot.amount / 2f) : slot.amount;
        splitDrag = draggedAmount < slot.amount;
        dragIcon.sprite = slot.item.icon;
        dragAmountText.SetText(draggedAmount > 1 ? draggedAmount.ToString() : "");
        dragPreview.gameObject.SetActive(true);
        slotViews[index].SetDragging(true);
        UpdateDrag(screenPosition);
    }

    public void UpdateDrag(Vector2 screenPosition)
    {
        if (draggedSlot < 0) return;

        dragPreview.position = screenPosition;
    }

    // targetIndex is -1 when dropped outside a slot: the item goes back where it was.
    public void EndDrag(int targetIndex)
    {
        if (draggedSlot < 0) return;

        int from = draggedSlot;
        int amount = draggedAmount;
        bool split = splitDrag;
        CancelDrag();

        // Both fire OnChanged → Refresh.
        if (split)
            inventory.Split(from, targetIndex, amount);
        else
            inventory.MoveOrMerge(from, targetIndex);
    }

    private void CancelDrag()
    {
        if (draggedSlot < 0) return;

        slotViews[draggedSlot].SetDragging(false);
        draggedSlot = -1;
        draggedAmount = 0;
        splitDrag = false;
        dragPreview.gameObject.SetActive(false);
    }
}

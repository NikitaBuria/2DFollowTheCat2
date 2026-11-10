using UnityEngine;

// Draws the always-visible hotbar: the first HotbarSize inventory slots (keys 1-7)
// and a highlight on the selected one.
public class HotbarUI : MonoBehaviour
{
    private const int HotbarSize = 7;

    [SerializeField] private Inventory inventory;
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private Transform slotParent;

    private readonly InventorySlotUI[] hotbarSlots = new InventorySlotUI[HotbarSize];

    private void OnEnable()
    {
        inventory.OnChanged += Refresh;
        inventory.OnSelectedSlotChanged += ShowSelection;
        Refresh();
        ShowSelection(inventory.selectedSlot);
    }

    private void OnDisable()
    {
        inventory.OnChanged -= Refresh;
        inventory.OnSelectedSlotChanged -= ShowSelection;
    }

    // Start, not Awake: every Awake (including Inventory's, which creates its slots) has run by now.
    private void Start()
    {
        BuildSlots();
        Refresh();
        ShowSelection(inventory.selectedSlot);
    }

    private void BuildSlots()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            hotbarSlots[i] = Instantiate(slotPrefab, slotParent);
        }
    }

    // Skips views that aren't built yet, so it is safe when OnEnable runs before Start.
    private void Refresh()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] != null)
                hotbarSlots[i].Show(inventory.GetSlot(i));
        }
    }

    private void ShowSelection(int selectedSlot)
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] != null)
                hotbarSlots[i].SetHighlighted(i == selectedSlot);
        }
    }
}

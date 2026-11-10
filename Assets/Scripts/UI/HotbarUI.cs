using UnityEngine;

// Draws the always-visible hotbar: the first HotbarSize inventory slots (keys 1-7).
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
        Refresh();
    }

    private void OnDisable()
    {
        inventory.OnChanged -= Refresh;
    }

    // Start, not Awake: every Awake (including Inventory's, which creates its slots) has run by now.
    private void Start()
    {
        BuildSlots();
        Refresh();
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
}

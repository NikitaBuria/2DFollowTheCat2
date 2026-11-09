using System.Collections.Generic;
using UnityEngine;

// Draws the inventory grid slots from Inventory.OnChanged. Visibility is UIManager's job.
public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private Transform slotsParent;

    private readonly List<InventorySlotUI> slotViews = new();

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
        for (int i = 0; i < inventory.count; i++)
        {
            slotViews.Add(Instantiate(slotPrefab, slotsParent));
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
}

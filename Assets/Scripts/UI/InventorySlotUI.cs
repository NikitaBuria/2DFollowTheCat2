using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;

    public void Show(InventorySlot slot)
    {
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
}

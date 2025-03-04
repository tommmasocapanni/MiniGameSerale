using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConsumableSlot : InventorySlot
{
    [SerializeField] private TextMeshProUGUI quantityText;
    private ConsumableItem consumableItem;

    public ConsumableItem CurrentItem => consumableItem;

    public void SetConsumableItem(ConsumableItem item)
    {
        base.SetItem(item);
        consumableItem = item;
        UpdateQuantityDisplay();
    }

    private void UpdateQuantityDisplay()
    {
        if (quantityText != null && consumableItem != null)
        {
            quantityText.text = consumableItem.Quantity.ToString();
            quantityText.gameObject.SetActive(true);
        }
        else
        {
            quantityText.gameObject.SetActive(false);
        }
    }

    protected override void OnSlotClicked()
    {
        if (consumableItem != null)
        {
            if (consumableItem.Use())
            {
                UpdateQuantityDisplay();
            }
        }
    }
}

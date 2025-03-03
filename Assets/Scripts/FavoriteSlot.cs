using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class FavoriteSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI slotNumberText;
    
    private int slotIndex;
    private CollectibleItem currentItem;

    public void Initialize(int index)
    {
        slotIndex = index;
        if (slotNumberText != null)
        {
            slotNumberText.text = (index + 1).ToString();
        }
    }

    public void SetItem(CollectibleItem item)
    {
        currentItem = item;
        
        if (itemIconImage != null)
        {
            if (item != null && item.ItemIcon != null)
            {
                itemIconImage.sprite = item.ItemIcon;
                itemIconImage.enabled = true;
            }
            else
            {
                itemIconImage.enabled = false;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Left click - equip the item
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (currentItem != null)
            {
                InventoryManager.Instance.EquipFavorite(slotIndex);
            }
        }
        // Right click - remove from favorites
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (currentItem != null)
            {
                InventoryManager.Instance.RemoveFromFavorites(slotIndex);
            }
        }
    }
}

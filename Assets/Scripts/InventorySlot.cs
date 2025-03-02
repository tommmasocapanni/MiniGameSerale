using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    
    private CollectibleItem currentItem;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnSlotClicked);
        }
    }

    public void SetItem(CollectibleItem item)
    {
        if (item == null) return;
        
        currentItem = item;
        
        // Setup semplificato dell'immagine
        if (itemIconImage != null)
        {
            itemIconImage.sprite = item.ItemIcon;
            itemIconImage.enabled = true;
            Debug.Log($"Tentativo di impostare sprite: {item.ItemName}, Sprite null? {item.ItemIcon == null}");
        }
        
        if (itemNameText != null)
        {
            itemNameText.text = item.ItemName;
        }

        gameObject.SetActive(true);
    }

    private void OnSlotClicked()
    {
        if (currentItem != null)
        {
            InventoryManager.Instance.EquipItem(currentItem);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnSlotClicked);
        }
    }
}

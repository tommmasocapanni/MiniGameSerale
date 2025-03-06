using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VendingMachineSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button purchaseButton;

    private VendingMachine vendingMachine;
    private VendingMachine.VendingMachineItem item;

    public void Setup(VendingMachine.VendingMachineItem item, VendingMachine machine)
    {
        if (item == null)
        {
            Debug.LogError("VendingMachineSlot: item is null!");
            return;
        }

        this.item = item;
        this.vendingMachine = machine;

        if (iconImage == null) Debug.LogError("VendingMachineSlot: iconImage is null!");
        if (itemNameText == null) Debug.LogError("VendingMachineSlot: itemNameText is null!");
        if (priceText == null) Debug.LogError("VendingMachineSlot: priceText is null!");
        if (purchaseButton == null) Debug.LogError("VendingMachineSlot: purchaseButton is null!");

        iconImage.sprite = item.icon;
        itemNameText.text = item.itemName;
        priceText.text = $"${item.price}";
        
        Debug.Log($"Setup VendingMachineSlot: {item.itemName}, Price: {item.price}");

        purchaseButton.onClick.AddListener(OnPurchase);
    }

    private void OnPurchase()
    {
        // Qui puoi aggiungere la logica per controllare se il player ha abbastanza soldi
        vendingMachine.DispenseItem(item.itemPrefab);
    }

    private void OnDestroy()
    {
        purchaseButton.onClick.RemoveListener(OnPurchase);
    }
}

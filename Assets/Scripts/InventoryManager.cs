using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject slotPrefab; // Prefab per ogni slot
    [SerializeField] private Transform slotsParent; // Il container degli slot (es: Grid Layout Group)
    
    private List<CollectibleItem> collectedItems = new List<CollectibleItem>();
    private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    private bool isInventoryOpen = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    public void AddItem(CollectibleItem item)
    {
        if (!collectedItems.Contains(item))
        {
            Debug.Log($"Aggiunto item: {item.ItemName}"); // Debug
            collectedItems.Add(item);
            CreateItemSlot(item);
        }
    }

    private void CreateItemSlot(CollectibleItem item)
    {
        if (slotPrefab == null || slotsParent == null)
        {
            Debug.LogError("Manca il prefab dello slot o il parent!");
            return;
        }

        GameObject newSlot = Instantiate(slotPrefab, slotsParent);
        InventorySlot slotScript = newSlot.GetComponent<InventorySlot>();
        
        if (slotScript != null)
        {
            Debug.Log($"Creazione slot per: {item.ItemName}"); // Debug
            slotScript.SetItem(item);
            inventorySlots.Add(slotScript);
        }
        else
        {
            Debug.LogError("InventorySlot component non trovato nel prefab!");
        }
    }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(isInventoryOpen);
        }
    }
}

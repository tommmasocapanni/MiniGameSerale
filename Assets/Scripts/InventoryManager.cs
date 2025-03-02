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
    private CollectibleItem equippedItem;
    private int currentItemIndex = -1;

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

        // Gestione rotella mouse
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel != 0 && collectedItems.Count > 0)
        {
            if (scrollWheel > 0)
                EquipNextItem();
            else
                EquipPreviousItem();
        }
    }

    public void AddItem(CollectibleItem item)
    {
        if (!collectedItems.Contains(item))
        {
            collectedItems.Add(item);
            CreateItemSlot(item);
            Debug.Log($"Aggiunto item: {item.ItemName}");
            
            // Equipaggia automaticamente OGNI oggetto raccolto
            EquipItem(item);
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

    public void EquipItem(CollectibleItem item)
    {
        // Non fare nulla se stiamo tentando di equipaggiare lo stesso oggetto
        if (equippedItem == item) return;

        // Disattiva l'oggetto precedentemente equipaggiato
        if (equippedItem != null)
        {
            equippedItem.UnequipItem();
        }

        // Equipaggia il nuovo oggetto
        equippedItem = item;
        currentItemIndex = collectedItems.IndexOf(item);
        item.EquipItem();
        
        Debug.Log($"Equipaggiato: {item.ItemName}");
    }

    private void EquipNextItem()
    {
        if (collectedItems.Count == 0) return;
        
        // Se nessun item è equipaggiato, equipaggia il primo
        if (equippedItem == null)
        {
            currentItemIndex = 0;
        }
        else
        {
            currentItemIndex = (currentItemIndex + 1) % collectedItems.Count;
        }
        
        EquipItem(collectedItems[currentItemIndex]);
    }

    private void EquipPreviousItem()
    {
        if (collectedItems.Count == 0) return;
        
        // Se nessun item è equipaggiato, equipaggia l'ultimo
        if (equippedItem == null)
        {
            currentItemIndex = collectedItems.Count - 1;
        }
        else
        {
            currentItemIndex--;
            if (currentItemIndex < 0) currentItemIndex = collectedItems.Count - 1;
        }
        
        EquipItem(collectedItems[currentItemIndex]);
    }
}

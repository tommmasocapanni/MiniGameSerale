using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject slotPrefab; // Prefab per ogni slot
    [SerializeField] private Transform slotsParent; // Il container degli slot (es: Grid Layout Group)
    
    [Header("UI References")]
    [SerializeField] private Image equippedItemImage; // Riferimento all'immagine UI dell'oggetto equipaggiato
    
    [Header("Favorites System")]
    [SerializeField] private int maxFavoriteSlots = 4; // Numero massimo di slot preferiti
    [SerializeField] private FavoriteSlot[] favoriteSlots; // Riferimenti agli slot preferiti esistenti nella UI
    
    [SerializeField] private GameObject consumableSlotPrefab;
    [SerializeField] private Transform consumablesParent;
    private List<ConsumableItem> consumableItems = new List<ConsumableItem>();
    private List<ConsumableSlot> consumableSlots = new List<ConsumableSlot>();
    
    private List<CollectibleItem> collectedItems = new List<CollectibleItem>();
    private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    private CollectibleItem[] favoriteItems; // Array di oggetti preferiti
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
        
        // Inizializza l'array dei preferiti
        favoriteItems = new CollectibleItem[maxFavoriteSlots];
    }

    private void Start()
    {
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }

        // Inizializza l'immagine dell'oggetto equipaggiato
        if (equippedItemImage != null)
        {
            equippedItemImage.enabled = false;
        }
        
        // Verifica che abbiamo gli slot preferiti
        if (favoriteSlots == null || favoriteSlots.Length == 0)
        {
            Debug.LogWarning("Non ci sono FavoriteSlot configurati. Cercando slot nella scena...");
            favoriteSlots = FindObjectsOfType<FavoriteSlot>();
            
            if (favoriteSlots.Length == 0)
            {
                Debug.LogError("Nessun FavoriteSlot trovato nella scena! Il sistema dei preferiti non funzionerà.");
            }
            else if (favoriteSlots.Length < maxFavoriteSlots)
            {
                Debug.LogWarning($"Trovati solo {favoriteSlots.Length} FavoriteSlot, ma servono {maxFavoriteSlots} slot.");
                maxFavoriteSlots = favoriteSlots.Length;
            }
        }
        
        // Inizializza gli slot con i rispettivi indici
        for (int i = 0; i < favoriteSlots.Length && i < maxFavoriteSlots; i++)
        {
            favoriteSlots[i].Initialize(i);
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

        // Aggiunta gestione tasti numerici per selezionare i preferiti
        for (int i = 0; i < maxFavoriteSlots; i++)
        {
            // I tasti numerici sono 1, 2, 3, 4, ecc.
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
            {
                EquipFavorite(i);
            }
        }
    }

    public void AddItem(CollectibleItem item)
    {
        if (item is ConsumableItem consumable)
        {
            AddConsumableItem(consumable);
        }
        else
        {
            if (!collectedItems.Contains(item))
            {
                collectedItems.Add(item);
                CreateItemSlot(item);
                Debug.Log($"Aggiunto item: {item.ItemName}");
                
                // Aggiungi l'item ai preferiti se c'è spazio
                TryAddToFavorites(item);
                
                // Equipaggia automaticamente OGNI oggetto raccolto
                EquipItem(item);
            }
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
        
        // Control cursor visibility based on inventory state
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetInventoryOpen(isInventoryOpen);
        }
        else
        {
            // Fallback if no cursor manager exists
            if (isInventoryOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
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
        
        // Aggiorna l'immagine dell'oggetto equipaggiato
        if (equippedItemImage != null)
        {
            equippedItemImage.sprite = item.ItemIcon;
            equippedItemImage.enabled = true;
        }
        
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

    private void TryAddToFavorites(CollectibleItem item)
    {
        // Cerca il primo slot vuoto nei preferiti
        for (int i = 0; i < favoriteItems.Length && i < favoriteSlots.Length; i++)
        {
            if (favoriteItems[i] == null)
            {
                // Aggiungi l'item a questo slot preferito
                favoriteItems[i] = item;
                favoriteSlots[i].SetItem(item);
                Debug.Log($"Aggiunto '{item.ItemName}' al preferito #{i+1}");
                return; // Esci dalla funzione una volta trovato uno slot
            }
        }
        
        // Se arriviamo qui, tutti gli slot preferiti sono pieni
        Debug.Log("Tutti gli slot preferiti sono occupati. L'item non è stato aggiunto ai preferiti.");
    }

    public void RemoveFromFavorites(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= favoriteItems.Length || slotIndex >= favoriteSlots.Length)
        {
            Debug.LogError($"Indice slot preferito non valido: {slotIndex}");
            return;
        }
        
        favoriteItems[slotIndex] = null;
        favoriteSlots[slotIndex].SetItem(null);
    }

    public void SwapFavorites(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= favoriteItems.Length || fromIndex >= favoriteSlots.Length || 
            toIndex < 0 || toIndex >= favoriteItems.Length || toIndex >= favoriteSlots.Length)
        {
            Debug.LogError("Indici slot preferiti non validi per lo scambio");
            return;
        }
        
        // Scambia gli item
        CollectibleItem temp = favoriteItems[fromIndex];
        favoriteItems[fromIndex] = favoriteItems[toIndex];
        favoriteItems[toIndex] = temp;
        
        // Aggiorna la UI
        favoriteSlots[fromIndex].SetItem(favoriteItems[fromIndex]);
        favoriteSlots[toIndex].SetItem(favoriteItems[toIndex]);
    }

    public void EquipFavorite(int index)
    {
        if (index < 0 || index >= favoriteItems.Length)
        {
            Debug.LogError($"Indice preferito non valido: {index}");
            return;
        }
        
        CollectibleItem item = favoriteItems[index];
        if (item != null)
        {
            Debug.Log($"Equipaggio preferito #{index+1}: {item.ItemName}");
            EquipItem(item);
        }
        else
        {
            Debug.Log($"Nessun item nel preferito #{index+1}");
        }
    }

    // Add this method to manually show the cursor when needed
    public void ShowCursor()
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.ForceShowCursor();
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void AddConsumableItem(ConsumableItem newItem)
    {
        // Cerca un item esistente dello stesso tipo
        ConsumableItem existingItem = consumableItems.Find(item => 
            item.GetType() == newItem.GetType() && item.CanStack(newItem));

        if (existingItem != null)
        {
            // Usa SOLO pickupAmount, ignora la quantity dell'item
            existingItem.AddToStack(newItem.PickupAmount);
            UpdateConsumableSlot(existingItem);
            Debug.Log($"Stacked {newItem.ItemName}, New quantity: {existingItem.Quantity}");
            Destroy(newItem.gameObject);
        }
        else
        {
            // Per nuovo item, imposta la quantity al pickupAmount
            newItem.Quantity = newItem.PickupAmount;
            consumableItems.Add(newItem);
            CreateConsumableSlot(newItem);
            Debug.Log($"Added new consumable: {newItem.ItemName}, Initial quantity: {newItem.Quantity}");
        }
    }

    private void CreateConsumableSlot(ConsumableItem item)
    {
        if (consumableSlotPrefab == null || consumablesParent == null) return;

        GameObject newSlot = Instantiate(consumableSlotPrefab, consumablesParent);
        ConsumableSlot slotScript = newSlot.GetComponent<ConsumableSlot>();
        
        if (slotScript != null)
        {
            slotScript.SetConsumableItem(item);
            consumableSlots.Add(slotScript);
        }
    }

    private void UpdateConsumableSlot(ConsumableItem item)
    {
        ConsumableSlot slot = consumableSlots.Find(s => s.GetComponent<ConsumableSlot>().CurrentItem == item);
        if (slot != null)
        {
            slot.SetConsumableItem(item);
        }
    }

    public void RemoveEmptyConsumable(ConsumableItem item)
    {
        consumableItems.Remove(item);
        ConsumableSlot slot = consumableSlots.Find(s => s.GetComponent<ConsumableSlot>().CurrentItem == item);
        if (slot != null)
        {
            consumableSlots.Remove(slot);  // Fixed: 'remove' to 'Remove'
            Destroy(slot.gameObject);
        }
        Destroy(item.gameObject);
    }
}

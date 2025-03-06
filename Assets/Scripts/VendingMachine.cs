using UnityEngine;

public class VendingMachine : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private Canvas vendingMachineCanvas;
    [SerializeField] private Transform itemsContainer; // Il parent degli slot
    [SerializeField] private VendingMachineSlot slotPrefab;
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 0, 0.5f); // Offset dalla macchinetta

    [Header("Items")]
    [SerializeField] private VendingMachineItem[] availableItems;

    [Header("Settings")]
    [SerializeField] private Transform spawnPoint; // Aggiunto il campo mancante

    private Camera mainCamera;
    private bool playerInRange = false;

    [System.Serializable]
    public class VendingMachineItem
    {
        public GameObject itemPrefab;
        public Sprite icon;
        public string itemName;
        public int price;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        SetupUI();
        vendingMachineCanvas.gameObject.SetActive(false);
    }

    private void SetupUI()
    {
        if (availableItems == null || availableItems.Length == 0)
        {
            Debug.LogError("VendingMachine: No items available!");
            return;
        }

        if (slotPrefab == null)
        {
            Debug.LogError("VendingMachine: slotPrefab is null!");
            return;
        }

        if (itemsContainer == null)
        {
            Debug.LogError("VendingMachine: itemsContainer is null!");
            return;
        }

        Debug.Log($"Setting up {availableItems.Length} items in vending machine");
        
        foreach (var item in availableItems)
        {
            if (item == null)
            {
                Debug.LogError("VendingMachine: Found null item in availableItems!");
                continue;
            }

            VendingMachineSlot slot = Instantiate(slotPrefab, itemsContainer);
            if (slot == null)
            {
                Debug.LogError("VendingMachine: Failed to instantiate slot!");
                continue;
            }

            slot.Setup(item, this);
        }
    }

    private void Update()
    {
        if (playerInRange)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                vendingMachineCanvas.gameObject.SetActive(!vendingMachineCanvas.gameObject.activeSelf);
            }

            UpdateUIRotation();
        }
    }

    private void UpdateUIRotation()
    {
        if (mainCamera != null && vendingMachineCanvas.gameObject.activeSelf)
        {
            // Posizione
            vendingMachineCanvas.transform.position = transform.position + uiOffset;

            // Rotazione
            Vector3 dirToCamera = mainCamera.transform.position - vendingMachineCanvas.transform.position;
            dirToCamera.y = 0; // Mantiene la rotazione solo sull'asse Y
            Quaternion targetRotation = Quaternion.LookRotation(-dirToCamera);
            vendingMachineCanvas.transform.rotation = targetRotation;
        }
    }

    public void DispenseItem(GameObject itemPrefab)
    {
        if (itemPrefab != null && spawnPoint != null)
        {
            GameObject dispensedItem = Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
            if (dispensedItem.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                float randomX = Random.Range(-0.5f, 0.5f);
                float randomZ = Random.Range(-0.5f, 0.5f);
                rb.AddForce(new Vector3(randomX, 0, randomZ), ForceMode.Impulse);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}

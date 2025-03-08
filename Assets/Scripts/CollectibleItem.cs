using UnityEngine;
using System.Collections;

public abstract class CollectibleItem : MonoBehaviour
{
    [SerializeField] private string itemName = "Item";
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private Vector3 equipPosition = Vector3.zero;
    [SerializeField] private Vector3 equipRotation = Vector3.zero;
    // Rimosso fireRate da qui

    // Aggiungi queste proprietà pubbliche
    public string ItemName => itemName;
    public Sprite ItemIcon => itemIcon;

    private bool isCollected = false;
    private Transform playerHandTransform;
    protected Animator playerAnimator;
    private bool canFire = true;
    private float nextFireTime = 0f;
    protected bool isEquipped = false;
    private Vector3 inventoryPosition = new Vector3(0, -1000, 0); // Posizione fuori vista

    private void Start()
    {
        // Assicurati che l'oggetto abbia un trigger collider
        if (GetComponent<Collider>() == null)
        {
            Debug.LogWarning($"Aggiungi un Collider a {gameObject.name}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            // Setup del transform e animator
            playerHandTransform = other.transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand/Hand");
            playerAnimator = other.GetComponent<Animator>();
            
            if (playerHandTransform == null || playerAnimator == null)
            {
                Debug.LogWarning("Missing references on player!");
                return;
            }

            // Raccogli e aggiungi all'inventario (che lo equipaggerà automaticamente)
            isCollected = true;
            OnPickup();  // Call OnPickup before adding to inventory
            InventoryManager.Instance.AddItem(this);
        }
    }

    private void Update()
    {
        if (isCollected && isEquipped)
        {
            HandleInput();
        }
    }

    protected virtual void HandleInput()
    {
        // Base implementation empty - override in derived classes
    }

    protected virtual void Fire()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isFiring", true);
            Debug.Log("Firing!");
        }
    }

    protected virtual void OnPickup()
    {
        // Base implementation - can be empty
    }

    public virtual void EquipItem()
    {
        if (isEquipped) return;
        
        isEquipped = true;
        transform.SetParent(playerHandTransform);
        transform.localPosition = equipPosition;
        transform.localEulerAngles = equipRotation;
        gameObject.SetActive(true);
        canFire = true;
    }

    public void UnequipItem()
    {
        if (!isEquipped) return;
        
        isEquipped = false;
        canFire = false;
        transform.position = inventoryPosition; // Sposta l'oggetto fuori vista
        gameObject.SetActive(false);
    }
}

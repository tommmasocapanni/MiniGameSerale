using UnityEngine;

public abstract class ConsumableItem : CollectibleItem
{
    [Header("Stack Settings")]
    [SerializeField] protected int maxStack = 99;
    [SerializeField] protected int quantity = 0;      // Partiamo da 0
    [SerializeField] protected int pickupAmount = 1;  // Quanti ne raccoglie ogni volta

    [Header("Debug Info")]
    [SerializeField] protected bool showDebugInfo = true;

    public int Quantity 
    { 
        get => quantity;
        set 
        {
            quantity = Mathf.Clamp(value, 0, maxStack);
            if (showDebugInfo) Debug.Log($"{ItemName}: Quantity set to {quantity}");
        }
    }

    public int PickupAmount => pickupAmount;

    public bool IsStackable => maxStack > 1;

    public virtual bool CanStack(ConsumableItem other)
    {
        return other != null && 
               GetType() == other.GetType() && 
               quantity < maxStack;
    }

    public virtual void AddToStack(int amount)
    {
        int oldQuantity = quantity;
        Quantity = Mathf.Clamp(quantity + amount, 0, maxStack);
        
        if (showDebugInfo)
        {
            int added = quantity - oldQuantity;
            Debug.Log($"{ItemName}: Added {added} (attempted {amount}). New total: {quantity}/{maxStack}");
        }
    }

    public virtual bool Use()
    {
        if (quantity <= 0) return false;
        
        quantity--;
        bool result = OnUse();
        
        if (quantity <= 0)
        {
            // Notify inventory to remove empty items
            InventoryManager.Instance.RemoveEmptyConsumable(this);
        }
        
        return result;
    }

    protected abstract bool OnUse();
}

using UnityEngine;

public class MoneyItem : ConsumableItem
{
    [Header("Money Properties")]
    [SerializeField] private int moneyAmount = 10;

    protected override void OnPickup()
    {
        base.OnPickup();
        
        PlayerMoney playerMoney = FindFirstObjectByType<PlayerMoney>();
        if (playerMoney != null)
        {
            playerMoney.AddMoney(moneyAmount * pickupAmount);
            
            // Update all MoneyItem slots in inventory
            var slots = FindObjectsOfType<ConsumableSlot>();
            foreach (var slot in slots)
            {
                if (slot.CurrentItem is MoneyItem)
                {
                    slot.SetConsumableItem(slot.CurrentItem);
                }
            }
        }
        
        // Hide the object but don't destroy it
        if (GetComponent<Renderer>()) GetComponent<Renderer>().enabled = false;
        if (GetComponent<Collider>()) GetComponent<Collider>().enabled = false;
    }

    public override bool CanStack(ConsumableItem other)
    {
        // Money items dello stesso tipo possono sempre essere stackati fino al maxStack
        return other is MoneyItem && quantity < maxStack;
    }

    protected override bool OnUse()
    {
        return false; // Money can't be used directly
    }
}

using UnityEngine;

public class MoneyItem : ConsumableItem
{
    protected override bool OnUse()
    {
        // Money can't be used directly
        return false;
    }

    protected override void OnPickup()
    {
        base.OnPickup();
        // Destroy the game object when picked up
        Destroy(gameObject);
    }
}

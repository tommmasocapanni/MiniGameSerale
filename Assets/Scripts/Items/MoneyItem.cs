using UnityEngine;

public class MoneyItem : ConsumableItem
{
    protected override bool OnUse()
    {
        // Money can't be used directly
        return false;
    }
}

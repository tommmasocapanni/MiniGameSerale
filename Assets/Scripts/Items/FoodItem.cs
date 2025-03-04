using UnityEngine;

public class FoodItem : ConsumableItem
{
    [Header("Food Properties")]
    [SerializeField] private int healthRestoreAmount = 20;

    protected override bool OnUse()
    {
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Heal(healthRestoreAmount);
            return true;
        }
        return false;
    }
}

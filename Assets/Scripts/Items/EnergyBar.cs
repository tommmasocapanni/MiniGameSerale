using UnityEngine;

public class EnergyBar : ConsumableItem
{
    [Header("Energy Properties")]
    [SerializeField] private float energyRestoreAmount = 25f;

    protected override bool OnUse()
    {
        PlayerEnergy playerEnergy = FindFirstObjectByType<PlayerEnergy>();
        if (playerEnergy != null)
        {
            playerEnergy.RestoreEnergy(energyRestoreAmount);
            return true;
        }
        return false;
    }
}

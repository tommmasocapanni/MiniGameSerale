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

    protected override void OnPickup()
    {
        base.OnPickup();
        // Disabilita solo il renderer e il collider, non distruggere l'oggetto
        if (GetComponent<Renderer>()) GetComponent<Renderer>().enabled = false;
        if (GetComponent<Collider>()) GetComponent<Collider>().enabled = false;
    }
}

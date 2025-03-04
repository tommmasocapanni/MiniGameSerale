using UnityEngine;

public class PlayerEnergy : MonoBehaviour
{
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float energyRegenRate = 5f;
    private float currentEnergy;

    private void Start()
    {
        currentEnergy = maxEnergy;
    }

    private void Update()
    {
        // Natural energy regeneration over time
        if (currentEnergy < maxEnergy)
        {
            currentEnergy = Mathf.Min(currentEnergy + (energyRegenRate * Time.deltaTime), maxEnergy);
        }
    }

    public void RestoreEnergy(float amount)
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        Debug.Log($"Energy restored: {amount}. Current energy: {currentEnergy}");
    }

    public void ConsumeEnergy(float amount)
    {
        currentEnergy = Mathf.Max(currentEnergy - amount, 0);
        Debug.Log($"Energy consumed: {amount}. Current energy: {currentEnergy}");
    }

    public float GetCurrentEnergy()
    {
        return currentEnergy;
    }

    public bool HasEnough(float amount)
    {
        return currentEnergy >= amount;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class PlayerEnergy : MonoBehaviour
{
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float startingEnergy = 50f; // Energia iniziale, può essere <= maxEnergy
    [SerializeField] private float energyRegenRate = 5f;
    [SerializeField] private ParticleSystem energyRestoreEffect;
    [SerializeField] private Image energyFillImage; // Nuovo riferimento all'UI
    private float currentEnergy;

    private void Start()
    {
        currentEnergy = Mathf.Min(startingEnergy, maxEnergy); // Assicuriamoci che non superi maxEnergy
        UpdateEnergyUI();
    }

    private void Update()
    {
        if (currentEnergy < maxEnergy)
        {
            currentEnergy = Mathf.Min(currentEnergy + (energyRegenRate * Time.deltaTime), maxEnergy);
            UpdateEnergyUI(); // Aggiorna UI quando rigenera
        }
    }

    private void UpdateEnergyUI()
    {
        if (energyFillImage != null)
        {
            energyFillImage.fillAmount = currentEnergy / maxEnergy;
        }
    }

    public void RestoreEnergy(float amount)
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        UpdateEnergyUI(); // Aggiorna UI quando ripristina
        if (energyRestoreEffect != null)
        {
            energyRestoreEffect.Play();
        }
        Debug.Log($"Energy restored: {amount}. Current energy: {currentEnergy}");
    }

    public void ConsumeEnergy(float amount)
    {
        currentEnergy = Mathf.Max(currentEnergy - amount, 0);
        UpdateEnergyUI(); // Aggiorna UI quando consuma
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

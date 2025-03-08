using UnityEngine;
using UnityEngine.UI; // Add this for Image component

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float startingHealth = 50f; // Salute iniziale, può essere <= maxHealth
    [SerializeField] private Image healthFillImage; // Reference to UI Image
    private float currentHealth;

    private void Start()
    {
        currentHealth = Mathf.Min(startingHealth, maxHealth); // Assicuriamoci che non superi maxHealth
        UpdateHealthUI();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();
        Debug.Log($"Player healed for {amount}. Current health: {currentHealth}");
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        UpdateHealthUI();
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");
        // Implement death logic here
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}

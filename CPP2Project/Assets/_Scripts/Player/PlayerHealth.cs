using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100;
    public float currentHealth;
    public HealthUI healthBar;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetHealth(1f);
    }

    //Function to deal damage, trigger death logic, update UI, includes debug log
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    //RIP
    private void Die()
    {
        Debug.Log("Player has died.");
        SceneController sceneController = Object.FindFirstObjectByType<SceneController>();
        if (sceneController != null)
        {
            sceneController.LoadDeathScreen();
        }
    }

    //Increases health and updates UI
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();

        Debug.Log($"Player healed {amount} health. Current health: {currentHealth}");
    }

    //Update UI according to damage and heal
    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            float normalizedHealth = currentHealth / maxHealth;
            healthBar.SetHealth(normalizedHealth);
        }
    }
}

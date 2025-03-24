using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider healthSlider;
    public Image fillImage; //Assign this to the Fill Area in the inspector
    public Gradient healthColorGradient;
    private Coroutine warningCoroutine;
    private bool isWarning = false; //flag to prevent redundant coroutine calls

    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            float normalizedHealth = (float)currentHealth / maxHealth;
            healthSlider.value = normalizedHealth;
        }
    }

    //Directly sets health value without interpolation
    public void SetHealth(float normalizedHealth)
    {
        normalizedHealth = Mathf.Clamp01(normalizedHealth);

        if (healthSlider != null)
        {
            healthSlider.value = normalizedHealth;
        }

        if (fillImage != null && healthColorGradient != null)
        {
            fillImage.color = healthColorGradient.Evaluate(normalizedHealth);
        }

        //Current threshold 20% of total health
        if (normalizedHealth < 0.2f && !isWarning)
        {
            isWarning = true;
            warningCoroutine = StartCoroutine(BlinkRedEffect());
           
        }
        else if (normalizedHealth >= 0.2f && isWarning)
        {
            isWarning = false;
            StopCoroutine(warningCoroutine);
            warningCoroutine = null;
            fillImage.color = healthColorGradient.Evaluate(normalizedHealth);
        }
    }

    private IEnumerator BlinkRedEffect()
    {
        while (true)
        {
            fillImage.color = Color.red;
            yield return new WaitForSeconds(0.3f);
            fillImage.color = Color.grey;
            yield return new WaitForSeconds(0.3f);
        }
    }
}



using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Chickens : MonoBehaviour
{
        
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private Image iconImage; //Assign in Inspector
    public Sprite chickenSprite;
    public Sprite skullSprite;

    private int capturedCount = 0;
    private int killedCount = 0;
    private bool isDarkMode = false;
    

    private void Start()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OnToneChanged += HandleToneChanged;
        }

        UpdateCounter();
    }

    private void OnDestroy()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OnToneChanged -= HandleToneChanged;
        }
    }

    public void IncrementCaptured()
    {
        if (!isDarkMode)
        {
            capturedCount++;
        }
        UpdateCounter();
    }

    public void IncrementKilled()
    {
        if (isDarkMode)
        {
            killedCount++;
        }
        UpdateCounter();
    }

    private void HandleToneChanged(GameTone newTone)
    {
        isDarkMode = newTone == GameTone.Dark;

        // Switch Image
        if (iconImage != null)
        {
            iconImage.sprite = isDarkMode ? skullSprite : chickenSprite;
        }

        UpdateCounter();
    }

    private void UpdateCounter()
    {
        if (pointsText != null)
        {
            pointsText.text = isDarkMode ? $"{killedCount}" : $"{capturedCount}";
        }
    }
}

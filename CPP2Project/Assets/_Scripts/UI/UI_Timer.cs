using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI minText;
    [SerializeField] private TextMeshProUGUI secText;
    [SerializeField] private Image fillImage;

    private Coroutine minuteWarning;
    private bool isWarning;

    // Start is called before the first frame update
    void Start()
    {
        TimerManager.Instance.OnTimerUpdate += DisplayTime;
    }

    private void OnDestroy()
    {
        TimerManager.Instance.OnTimerUpdate -= DisplayTime;
    }

    private void DisplayTime(float time)
    {
        int timeLeft = (int)time;

        minText.text = (timeLeft / 60).ToString();
        secText.text = (timeLeft % 60).ToString("00");

        if (timeLeft <= 30 && !isWarning)
        {
            isWarning = true;
            minuteWarning = StartCoroutine(BlinkRedEffect());
        }
        else if (timeLeft > 30 && isWarning)
        {
            isWarning = false;
            if (minuteWarning != null)
            {
                StopCoroutine(minuteWarning);
                fillImage.color = Color.white; //Reset colour
            }
        }
    }

    private IEnumerator BlinkRedEffect()
    {
        
        while (isWarning)
        {
            fillImage.color = Color.red;
            secText.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            fillImage.color = Color.white;
            secText.color = Color.white;
            yield return new WaitForSeconds(0.5f);

        }
        
    }

}

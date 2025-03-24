using UnityEngine;
using TMPro;

public class WinScreenUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    private void Start()
    {
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        scoreText.text = finalScore.ToString();
    }
}

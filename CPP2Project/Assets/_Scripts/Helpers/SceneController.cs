using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void LoadGame()
    {
        SceneManager.LoadScene("LevelTest");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadWinScreenCollected()
    {
        SceneManager.LoadScene("WinScreen_Collected");
    }

    public void LoadWinScreenDarkMode()
    {
        SceneManager.LoadScene("WinScreen_DarkMode");
    }

    public void LoadDeathScreen()
    {
        SceneManager.LoadScene("DeathScreen");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimerManager : MonoBehaviour
{

    public delegate void Timer(float currentTime);
    public event Timer OnTimerUpdate;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public static TimerManager Instance {  get; private set; }

    public float levelTime = 300f; //5 mins
    private float currentTime;

    public bool isTimerRunning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "LevelTest")
        {
            isTimerRunning = false;
        }
    }

    void Start()
    {
        currentTime = levelTime;   
    }

    // Update is called once per frame
    void Update()
    {
        if (!isTimerRunning)
        {
            UpdateTimer();
        }
    }

    public void StartTimer()
    {
        isTimerRunning = true;
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    private void UpdateTimer()
    {
        currentTime -= Time.deltaTime;
        currentTime = Mathf.Clamp(currentTime, 0, levelTime);

        if (currentTime <= 0)
        {
            EndGame();
        }

        OnTimerUpdate?.Invoke(currentTime);
    }

    private void EndGame()
    {
        isTimerRunning = false;

        int finalScore = ChickenManager.instance.GetFinalScore();
        PlayerPrefs.SetInt("FinalScore", finalScore);

        if (ChickenManager.instance.IsDarkMode())
        {
            SceneManager.LoadScene("WinScreen_DarkMode");
        }
        else
        {
            SceneManager.LoadScene("WinScreen_Collected");
        }
    }

    public void ResetTimer()
    {
        currentTime = levelTime;
    }


}

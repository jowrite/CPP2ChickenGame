using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ChickenManager : MonoBehaviour
{
    public static ChickenManager instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private AudioClip collectSound;
    //[SerializeField] private ParticleSystem captureEffect;

    private HashSet<Collectible> collectedChickens = new HashSet<Collectible>();
    private int killCount = 0;
    private int score = 0;

    public event System.Action<int> OnChickenCaptured = delegate { };
    public event System.Action<int> OnScoreUpdated = delegate { };

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public void CaptureChicken(Collectible chicken)
    {
        if (chicken == null || collectedChickens.Contains(chicken)) return;

        //Add to collection
        collectedChickens.Add(chicken);

        int points = chicken.GetPoints();
        score += points;

        //Handle events
        OnChickenCaptured?.Invoke(points);
        OnScoreUpdated?.Invoke(score);

        Object.FindFirstObjectByType<UI_Chickens>().IncrementCaptured();

        PlayCaptureEffects(chicken.transform.position);

        //Disable chicken
        chicken.gameObject.SetActive(false);
    }

    private void PlayCaptureEffects(Vector3 position)
    {
        if (collectSound != null)
            AudioManager.instance.PlaySFX(collectSound);

        //if (captureEfect != null)
        //    Instantiate(captureEffect, position, Quaternion.identity);
    }

    public void ChickenKilled()
    {
        killCount++;
    }

    public int GetTotalChickens() => collectedChickens.Count;
    public int GetTotalPoints() => collectedChickens.Sum(c => c.GetPoints());
    public int GetScore() => score;

    public void SetAllChickensToAggro()
    {
        foreach (EnemyAI chicken in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
        {
            if (chicken.CurrentState == EnemyAI.AIState.Patrol)
            {
                chicken.ChangeState(EnemyAI.AIState.Chase);
            }
        }
    }

    public int GetFinalScore()
    {
        return Mathf.Max(collectedChickens.Count, killCount); // Higher score determines win type
    }

    public bool IsDarkMode()
    {
        return killCount > collectedChickens.Count;
    }
}

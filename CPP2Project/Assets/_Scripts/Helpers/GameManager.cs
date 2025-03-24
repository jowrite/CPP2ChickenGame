using UnityEngine;

public enum GameTone
{
    Light,
    Dark
}
public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    public GameTone currentTone = GameTone.Light;

    public event System.Action<GameTone> OnToneChanged = delegate { };

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

    public void SetTone(GameTone tone)
    {
        if (currentTone != tone)
        {
            currentTone = tone;
            OnToneChanged.Invoke(tone);


            if (tone == GameTone.Dark)
            {
                ChickenManager.instance.SetAllChickensToAggro();
            }
        }
    }

    public void ResetTone()
    {
        SetTone(GameTone.Light);
    }
}

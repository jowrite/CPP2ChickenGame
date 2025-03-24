using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    private CinemachineCamera freeLook;

    void Awake()
    {
        freeLook = GetComponent<CinemachineCamera>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LevelTest")
        {
            GameObject newPlayer = GameObject.FindGameObjectWithTag("Player");
            if (newPlayer != null)
            {
                freeLook.Follow = newPlayer.transform;
                freeLook.LookAt = newPlayer.transform;
            }
        }
    }
}

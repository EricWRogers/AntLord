using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioSceneMusicController : MonoBehaviour
{
    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Music Names (AudioManager2 Sound names)")]
    public string menuMusicName = "MenuMusic";

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (AudioManager2.instance == null) return;

        // If in main menu, start menu music
        if (scene.name == mainMenuSceneName)
        {
            AudioManager2.instance.Play(menuMusicName);
        }
        // Otherwise, make sure menu music is off
        else
        {
            AudioManager2.instance.Stop(menuMusicName);
        }
    }
}
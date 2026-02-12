using UnityEngine;
using UnityEngine.SceneManagement;

public class MM : MonoBehaviour
{
    private GameObject pm;
    
    void Awake()
    {
        Transform t = transform.Find("PM");
        if (t != null)
        {
            pm = t.gameObject;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && pm != null)
        {
            Pause();
        }
    }

    public void Play(string level)
    {
        SceneManager.LoadScene(level);
        Debug.Log("Game Started.");
    }

    public void Options(GameObject menu)
    {
        menu.SetActive(true);
        Debug.Log("Options Menu Opened.");
    }

    public void Quit()
    {
        Time.timeScale = 1;

        Debug.Log("Bye, Bye! (Exited Game)");

        if (Application.isEditor)
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
        else
        {
            Application.Quit();
        }
    }

    public void Home(string level)
    {
        SceneManager.LoadScene(level);
        Time.timeScale = 1;
        Debug.Log("Returned to Home.");
    }

    public void Restart(string level)
    {
        SceneManager.LoadScene(level);
        Time.timeScale = 1;
        Debug.Log("Game Restarted.");
    }

    public void Pause()
    {
        if (pm == null) return;

        bool paused = Time.timeScale == 0;
        Time.timeScale = paused ? 1:0;
        pm.SetActive(!paused);
        Debug.Log("Game " + (paused ? "Resumed." : "Paused."));
    }
}

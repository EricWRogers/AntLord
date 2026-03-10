using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class MM : MonoBehaviour
{
    private GameObject pm;
    private bool vrButtonPressed = false;
    
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
        // PC Pause (Escape key)
        if (Input.GetKeyDown(KeyCode.Escape) && pm != null)
        {
            Pause();
        }

        // VR Pause (Left Hand Menu button)
        InputDevice lefthand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        if (lefthand.isValid && lefthand.TryGetFeatureValue(CommonUsages.menuButton, out bool pressed))
        {
            // Only trigger once when button is first pressed
            if (!vrButtonPressed)
            {
                Pause();
            }

            vrButtonPressed = pressed;
        }
    }


    public void Play(string level)
    {
        SceneTransitionManager.singleton.GoToSceneAsync(1);
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

    public void Resume()
    {
        if (pm != null)
        {
            Time.timeScale = 1;
            pm.SetActive(false);
            Debug.Log("Game Resumed.");
        }
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

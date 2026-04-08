using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PM : MonoBehaviour
{
    public GameObject pm;

    public bool isPaused = true;

    void Start()
    {
        DisplayPM();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DisplayPM();
        }
    }

    public void PauseButtonPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            DisplayPM();
        }
    }

    public void DisplayPM()
    {
        if (isPaused)
        {
            pm.SetActive(false);
            isPaused = false;
            Time.timeScale = 1;
            Debug.Log("Game Resumed.");
        }
        else if (!isPaused)
        {
            pm.SetActive(true);
            isPaused = true;
            Time.timeScale = 0;
            Debug.Log("Game Paused.");
        }
    }

    public void Restart(string levelName)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(levelName);
    }

     public void Options(GameObject menu)
    {
        menu.SetActive(true);
        Debug.Log("Options Menu Opened.");
    }

    public void Home(string levelName)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(levelName);
    }

    public void Quit()
    {
        Debug.Log("Exiting Game...");
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}

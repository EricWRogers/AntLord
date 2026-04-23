using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PM : MonoBehaviour
{
    public GameObject pm;

    public bool isPaused = true;

    public float spawnDistance = 20.0f;

    private Camera sceneCamera;

    void Start()
    {
        if (!sceneCamera) sceneCamera = Camera.main;

        DisplayPM();
    }

    void Update()
    {
        // Listen for the Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DisplayPM();
        }

        //tracks pm UI to always face the player.
        pm.transform.LookAt(new Vector3(sceneCamera.gameObject.transform.position.x, pm.transform.position.y, sceneCamera.gameObject.transform.position.z));
        pm.transform.forward *= -1;
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
            pm.transform.position = sceneCamera.gameObject.transform.position +
            new Vector3(sceneCamera.gameObject.transform.forward.x, -0.1f, sceneCamera.gameObject.transform.forward.z).normalized * spawnDistance;
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

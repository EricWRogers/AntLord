using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MM : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject pm; // Drag the PM object here in Inspector
    [SerializeField] private InputActionAsset customActions; // Drag your Input Asset here
    [SerializeField] private string actionMapName = "Menu"; // Matches your custom map name

    // private InputAction pauseAction;
    public GameObject levelSelectMenu; //drag the levelselect menu

     public void Play()
    {
        //Main menu start button will now open level select menu
        levelSelectMenu.SetActive(true);
        //SceneTransitionManager.singleton.GoToSceneAsyncByName(level);
        //Debug.Log("Game Started: " + level);
    }

    public void OpenSubMenu(GameObject subMenu)
    {
        subMenu.SetActive(true);
    }

    public void BackButton(GameObject subMenu) //hide submenu
    { 
        subMenu.SetActive(false); 
    }

    public void Restart(string levelName)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(levelName);
    }

    public void Quit()
    {
        Time.timeScale = 1;
        Debug.Log("Exiting Game...");
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
    
    public void Pause()
    {
        if (pm == null) 
        {
            Debug.LogError("Yo! The PM (Pause Menu) object is missing!");
            return;
        }

        bool isCurrentlyPaused = Time.timeScale == 0;
        
        if (isCurrentlyPaused)
        {
            Resume();
        }
        else
        {
            Time.timeScale = 0;
            pm.SetActive(true);
            
            // Pro Tip: Force the menu to look at the player when it pops up
            pm.transform.LookAt(new Vector3(Camera.main.transform.position.x, pm.transform.position.y, Camera.main.transform.position.z));
            pm.transform.Rotate(0, 180, 0); // Flip it so it's not backwards

            Debug.Log("Game Paused.");
        }
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

    public void StartLevel(string level)
    {
        SceneManager.LoadScene(level);
        Debug.Log("Game Started: " + level);
    }

    // --- Scene Management (Keeping your original logic) ---

   /* void OnEnable()
    {
        // This turns on the 'radio' so Unity actually hears the button click
        if (customActions != null)
        {
            var map = customActions.FindActionMap(actionMapName);
            if (map != null)
            {
                Debug.Log("Enabling Input Action Map: " + actionMapName);
                map.Enable();
                // Optional: find the action directly if Unity Events act up
                pauseAction = map.FindAction("MenuP");
            }
        }
    }

    void Awake()
    {
        // Backup: if you didn't drag it in, we try to find it
        if (pm == null)
        {
            Transform t = transform.Find("PM");
            if (t != null) pm = t.gameObject;
        }

        // Start with the menu hidden so it's not in your face at spawn
        if (pm != null) pm.SetActive(false);
    }

    void Update()
    {
        // PC Pause (Escape key) - Keepin' it classic
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

    // This is the one you link to your Player Input Events
    public void PauseButtonPressed(InputAction.CallbackContext context)
    {
        // 'started' is better than 'performed' for VR menu buttons
        // It prevents the menu from flickering on/off in one click
        if (context.started)
        {
            Pause();
        }
    }*/
}
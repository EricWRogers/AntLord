using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectCode : MonoBehaviour
{
    //Create Singleton
    public static LevelSelectCode instance = null;

    //Set Playmode
    public bool VRIsActivated;

    public GameObject DesktopCam;
    public GameObject VRCam;

    //LevelSelect
    public GameObject levelSelectMenu;

    void Awake()
    {
        if (instance == null)
        {
            Debug.Log("Singleton init");
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    void Start()
    {
        SetPlayMode();
        levelSelectMenu.SetActive(false);
    }

    public void SetPlayMode()
    {
        if (VRIsActivated){
            DesktopCam.SetActive(false);
            VRCam.SetActive(true);
        }
        else
        {
            DesktopCam.SetActive(true);
            VRCam.SetActive(false);
        }
    }

    public void StartLevel(string level)
    {
        SceneManager.LoadScene(level);
        Debug.Log("Game Started: " + level);
    }

     public void StartLevel1(string level){
        SceneManager.LoadScene(level);
        Debug.Log("Game Started: " + level);
    }
    public void StartLevel2(string level){
        SceneManager.LoadScene(level);
        Debug.Log("Game Started level 2: " + level);
    }
    public void StartLevel3(string level){
        SceneManager.LoadScene(level);
        Debug.Log("Game Started level 3: " + level);
    }

    public void BackButton(){ //hide level select
        levelSelectMenu.SetActive(false);
        
    }

    public void ShowLevelMenu(){ //show level select
        levelSelectMenu.SetActive(true);
        
    }
}

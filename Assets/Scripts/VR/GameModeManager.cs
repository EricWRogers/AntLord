using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
using System.Collections;

public class GameModeManager : MonoBehaviour
{
    //Create Singleton
    public static GameModeManager instance = null;

    public GameObject DesktopCam;
    public GameObject VRCam;

    public bool VRIsActivated;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

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
    }

    void OnLevelWasLoaded(int level)
    {
        SetPlayMode();
        Debug.Log("level loaded: Level " + level);
    }

    public void SetPlayMode()
    {
        if (!DesktopCam)
            DesktopCam = GameObject.Find("DesktopCam");
        
        if (!VRCam)
            VRCam = GameObject.Find("VR Player");

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

    public void SetVRGameMode()
    {
        DesktopCam.SetActive(false);
        VRCam.SetActive(true);

        VRIsActivated = true;
    }

    public void SetDesktopGameMode()
    {
        DesktopCam.SetActive(true);
        VRCam.SetActive(false);

        VRIsActivated = false;
    }
}

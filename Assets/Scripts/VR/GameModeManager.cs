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

    [SerializeReference]
    public static bool VRIsActivated;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        
    }

    void Start()
    {
        //SetPlayMode();
    }

    void OnLevelWasLoaded(int level)
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

        int i = 0;
        while (i < 1)
        {
            SetPlayMode();
            Debug.Log("level loaded. Vr is active? : " + VRIsActivated);
            i += 1;
        }
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

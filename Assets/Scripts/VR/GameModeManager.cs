using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    //Create Singleton
    public static GameModeManager instance = null;

    public GameObject DesktopCam;
    public GameObject VRCam;
    public GameObject PlayerPlane;

    public static bool VRIsActivated;

    [Tooltip("click this if you need to run the VR controls instead of the desktop controls in bugtesting.")]
    public bool isVROverride;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

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
    }

    void Awake()
    {
        SetPlayMode();
        Debug.Log("level loaded. Vr is active? : " + VRIsActivated);
    }    

    public void SetPlayMode()
    {
        if (!DesktopCam)
            DesktopCam = GameObject.Find("DesktopCam");
        
        if (!VRCam)
            VRCam = GameObject.Find("VR Player");


        if (VRIsActivated || isVROverride){
            DesktopCam.SetActive(false);
            VRCam.SetActive(true);

            if (PlayerPlane)
            {
                PlayerPlane.SetActive(true);
            }
        }
        else
        {
            DesktopCam.SetActive(true);
            VRCam.SetActive(false);

            if (PlayerPlane)
            {
                PlayerPlane.SetActive(false);
            }
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

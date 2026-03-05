using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
using System.Collections;

public class GameModeManager : MonoBehaviour
{

    public bool VRIsActivated;

    public GameObject DesktopCam;
    public GameObject VRCam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
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
}

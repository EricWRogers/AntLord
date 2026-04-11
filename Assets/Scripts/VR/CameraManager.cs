using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

public class CameraManager : MonoBehaviour
{

    public GameObject VRCam;
    public GameObject DesktopCam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        //StartCoroutine("StartXRCoroutine");

        Debug.Log("Initializing XR...");
        if (XRGeneralSettings.Instance.Manager.activeLoader != null) 
        {
            Debug.Log("Loading VR.");
            // XR device detected/loaded
            VRCam.gameObject.SetActive(false);
            DesktopCam.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("VR failed. Loading Desktop.");
            VRCam.gameObject.SetActive(true);
            DesktopCam.gameObject.SetActive(false);
        }
    }

    /*
    public IEnumerator StartXRCoroutine()
    {
        Debug.Log("Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");

            //load desktop instead
            StopXR();
            StartDesktopMode();
        }
        else
            Debug.Log("Initialization Finished. Starting XR Subsystems...");

            //Try to start all subsystems and check if they were all successfully started (HMD prepared).
            bool loaderSuccess = XRGeneralSettings.Instance.Manager.activeLoader.Start();               
            if(loaderSuccess)
            {
                Debug.Log("All Subsystems Started!");

                VRCam.gameObject.SetActive(true);
                DesktopCam.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("Starting Subsystems Failed. Directing to Normal Interaciton Mode...!");
                StopXR();
                StartDesktopMode();
            }
    }

    void StopXR()
    {
        Debug.Log("Stopping XR...");

        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        Debug.Log("XR stopped completely.");
    }
    void StartDesktopMode()
    {
        VRCam.gameObject.SetActive(false);
        DesktopCam.gameObject.SetActive(true);
    }
    */

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Management;

public class CameraManager : MonoBehaviour
{

    public GameObject VRCam;
    public GameObject DesktopCam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public bool IsHeadsetConnected()
    {
        var devices = new List<InputDevice>();
        // Get all devices that are Head Mounted Displays (HMDs)
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, devices);
        return devices.Count > 0;
    }
    void Start()
    {
        
        //StartCoroutine("StartXRCoroutine");
        //IsHeadsetConnected();

        var devices = new List<InputDevice>();
        // Get all devices that are Head Mounted Displays (HMDs)
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, devices);

        /*if (devices.Count > 0) {
        // XR device detected/loaded
            Debug.Log("headset connected!");
            VRCam.gameObject.SetActive(true);
            DesktopCam.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("No headset found, enabling desktop controls.");
            VRCam.gameObject.SetActive(false);
            DesktopCam.gameObject.SetActive(true);
        }*/

        Debug.Log("Initializing XR...");
        if (IsHeadsetConnected())
        {
            Debug.Log("headset connected!");
            VRCam.gameObject.SetActive(true);
            DesktopCam.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("No headset found, enabling desktop controls.");
            VRCam.gameObject.SetActive(false);
            DesktopCam.gameObject.SetActive(true);
        }
        

    }

    
    /*
    public IEnumerator StartXRCoroutine()
    {
        Debug.Log("Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed. Loading Desktop.");

            //load desktop instead
            StopXR();
            StartDesktopMode();
        }
        else
        {
            Debug.Log("Initialization Finished. Starting XR Subsystems...");

            //Try to start all subsystems and check if they were all successfully started (HMD prepared).
            bool loaderSuccess = XRGeneralSettings.Instance.Manager.activeLoader.Start();               
            if(loaderSuccess)
            {
                Debug.Log("All XR Subsystems Started!");

                VRCam.gameObject.SetActive(true);
                DesktopCam.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("Starting Subsystems Failed. Directing to Desktop mode!");
                StopXR();
                StartDesktopMode();
            }
        }
    }

    void StopXR()
    {
        Debug.Log("Stopping XR...");

        VRCam.gameObject.SetActive(false);
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        Debug.Log("XR stopped completely.");
    }
    void StartDesktopMode()
    {
        DesktopCam.gameObject.SetActive(true);
    }
    */
}

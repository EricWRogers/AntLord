using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

//Left Primary button toggles dig mode and shovel
//Right Trigger digs voxels
//Left Trigger places voxels back

public class VRTerrainModSystem : RayOMayhem
{
    public XRRayInteractor LeftRayInteractor;
    public XRRayInteractor RightRayInteractor;

    [Header("Input Binds")]
    public XRIDefaultInputActions VRInputActions; //input c# script
    public InputActionMap VRRIGHTInteraction;//action map
    public InputActionMap VRLEFTInteraction;
    public InputAction RTrigger;//actions
    public InputAction RPrimaryButton;
    public InputAction LTrigger;//actions
    public InputAction LPrimaryButton;
    public InputAction LSecondaryButton;



    //-----------VR
    private void OnEnable()
    {
        VRInputActions.XRIRightInteraction.Enable();
        VRInputActions.XRILeftInteraction.Enable();
    }
    private void OnDisable()
    {
        VRInputActions.XRIRightInteraction.Disable();
        VRInputActions.XRILeftInteraction.Disable();
    }

    void Awake()
    {
        //set input binds
        VRInputActions = new XRIDefaultInputActions();

        VRRIGHTInteraction = VRInputActions.XRIRightInteraction;
        VRLEFTInteraction = VRInputActions.XRILeftInteraction;

        RTrigger = VRRIGHTInteraction.FindAction("Activate");
        LTrigger = VRLEFTInteraction.FindAction("Activate");

        LSecondaryButton = VRLEFTInteraction.FindAction("SecondaryButtonSelect");

        if (VRInputActions != null)
        {
            VRRIGHTInteraction.Enable();
            VRLEFTInteraction.Enable();

            RTrigger.Enable();
            LPrimaryButton.Enable();
            LTrigger.Enable();
        }

        LSecondaryButton.performed += OnLeftSecondary;
        OnClicked += ProcessVoxelMod;
    }
    protected override void Update()
    {
        base.Update();

        if (!inModMode) return;

        //if (RTrigger.ReadValue<float>() > 0.1f && shovel.activeInHierarchy)
        //{
            TriggerClick();
        //}
        //if (LTrigger.ReadValue<float>() > 0.1f && resourceManager != null && resourceManager.sand > 0)
        //{
        //    TriggerClick();
        //}
    }
    private void OnLeftSecondary(InputAction.CallbackContext context)
    {
        inModMode = !inModMode;
        Debug.Log($"Mod mode {inModMode}");
        if (inModMode)
        {
            shovel.SetActive(true);
            TriggerClick();
        }
        else
        {
            shovel.SetActive(false);
            TriggerExit();
        }
    }

    private void ProcessVoxelMod()//this is called by the TriggerClick event, which is called by the input actions
    {
        if (RightRayInteractor.TryGetHitInfo(out Vector3 rightPos, out Vector3 rightNormal, out int rightIndex, out bool rightValid))// && shovel.activeInHierarchy
        {
            Debug.Log($"Right Ray Hit at: {rightPos}");
            HandleVoxelInput(rightPos, isBreaking: true);
        }
        else
        {
            Debug.Log("Right Ray hit nothing.");
        }

        if (LeftRayInteractor.TryGetHitInfo(out Vector3 leftPos, out Vector3 leftNormal, out int leftIndex, out bool leftValid))// && resourceManager != null && resourceManager.sand > 0
        {
            Debug.Log($"Left Ray Hit at: {leftPos}");
            HandleVoxelInput(leftPos, isBreaking: false);
        }
        else
        {
            Debug.Log("Left Ray hit nothing.");
        }
    }
}
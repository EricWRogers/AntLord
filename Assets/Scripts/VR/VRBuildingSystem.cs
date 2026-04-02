using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

//Reminder: include the ResourceManager in the build with like 100000 food
//Right Primary button toggles build mode
//Right Trigger places buildings
public class VRBuildingSystem : BuildingPlacementSystem
{
    public XRRayInteractor LeftRayInteractor;
    public XRRayInteractor RightRayInteractor;

    [Header("Input Binds")]
    public XRIDefaultInputActions VRInputActions; //input c# script
    public InputActionMap VRRIGHTInteraction;//action map
    public InputActionMap VRLEFTInteraction;
    public InputAction RTrigger;//actions
    public InputAction RPrimaryButton;
    public InputAction LTrigger;
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
        //VRLEFTInteraction = VRInputActions.XRILeftInteraction;

        RTrigger = VRRIGHTInteraction.FindAction("Activate");
        RPrimaryButton = VRRIGHTInteraction.FindAction("PrimaryButtonSelect");
        //LTrigger = VRLEFTInteraction.FindAction("Activate");
        //LPrimaryButton = VRLEFTInteraction.FindAction("PrimaryButtonSelect");
        //LSecondaryButton = VRLEFTInteraction.FindAction("SecondaryButtonSelect");

        if (VRInputActions != null)
        {
            VRRIGHTInteraction.Enable();
            //VRLEFTInteraction.Enable();

            RTrigger.Enable();
            RPrimaryButton.Enable();
            //LTrigger.Enable();
            //LPrimaryButton.Enable();
            //LSecondaryButton.Enable();
        }

        RTrigger.performed += OnRightTrigger;
        RPrimaryButton.performed += OnRightPrimary;
        //LPrimaryButton.performed += OnLeftPrimaryDown;
        //LSecondaryButton.performed += OnLeftSecondaryDown;
    }

    void OnRightPrimary(InputAction.CallbackContext context)
    {
        inBuildMode = !inBuildMode;
        Debug.Log($"Build mode {inBuildMode}");
        if (inBuildMode)
        {
            //UI stuff and pop ups should go here
            //assign a button with StartPlacement and with whatever index the building is
            StartPlacement(0);
        }
        else
        {
            TriggerExit();
        }
    }
    private void OnRightTrigger(InputAction.CallbackContext context)
    {

        if (inBuildMode)
        {
            TriggerClick();
        }
    }

    //-----------RayCast thingie
    protected override Vector3 GetSelectedMapPosition()
    {
        if (RightRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            //voodoo bitwise operation im not going to pretend to understand because i cant just do hit.layer == placementlayer
            if (((1 << hit.collider.gameObject.layer) & placementLayermask) != 0)
            {
                lastPosition = hit.point;
                angle = Vector3.Angle(hit.normal, Vector3.up);
            }
        }
        return lastPosition;
    }
}




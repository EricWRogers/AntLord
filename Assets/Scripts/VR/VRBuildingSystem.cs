using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

//Reminder: include the ResourceManager in the build with like 100000 food
//Possible problems:
//both triggers are pressed to enter buildMode, and I cant test simultaneous button presses in fake VR
public class VRBuildingSystem : BuildingPlacementSystem
{
    public XRRayInteractor rayInteractor;

    [Header("Input Binds")]
    public XRIDefaultInputActions VRInputActions; //input c# script
    public InputActionMap VRRIHGHTInteraction;//action map
    public InputActionMap VRLEFTInteraction;
    public InputAction RTrigger;//actions
    public InputAction LTrigger;
    public InputAction LPrimaryButton;
    public InputAction LSecondaryButton;

    [Header("Building System")]
    bool rightIsPressed = false;
    bool leftIsPressed = false;





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

        VRRIHGHTInteraction = VRInputActions.XRIRightInteraction;
        VRLEFTInteraction = VRInputActions.XRILeftInteraction;

        RTrigger = VRRIHGHTInteraction.FindAction("Activate");
        LTrigger = VRLEFTInteraction.FindAction("Activate");
        LPrimaryButton = VRLEFTInteraction.FindAction("PrimaryButtonSelect");
        LSecondaryButton = VRLEFTInteraction.FindAction("SecondaryButtonSelect");

        if (VRInputActions != null)
        {
            VRRIHGHTInteraction.Enable();
            VRLEFTInteraction.Enable();

            RTrigger.Enable();
            LTrigger.Enable();
            LPrimaryButton.Enable();
            LSecondaryButton.Enable();
        }

        RTrigger.performed += OnRightTrigger;
        LTrigger.performed += OnLeftTrigger;
        //LPrimaryButton.performed += OnLeftPrimaryDown;
        //LSecondaryButton.performed += OnLeftSecondaryDown;
    }

    private void OnRightTrigger(InputAction.CallbackContext context)
    {
        //Debug.Log("VR INPUT: Right trigger Activated " + gameObject.name);

        rightIsPressed = true;
        CheckBuildToggle();
        if (inBuildMode)
        {
            TriggerClick();
        }
    }
    private void OnLeftTrigger(InputAction.CallbackContext context)
    {
        //Debug.Log("VR INPUT: Right trigger Activated " + gameObject.name);

        leftIsPressed = true;
        CheckBuildToggle();
    }

    //-----------RayCast thingie
    protected override Vector3 GetSelectedMapPosition()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
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

    //--------Building Stuff

    void CheckBuildToggle()
    {
        if ((rightIsPressed && leftIsPressed) && !inBuildMode)//<----replace the && with || inside the paranthesis to test
        {
            AssignActions();
            StartPlacement(0);

            //Debug.Log($"Build mode: {inBuildMode}");

            rightIsPressed = false;
            leftIsPressed = false;
        }
        else if (rightIsPressed && leftIsPressed && inBuildMode)//<----Dont touch this one because it will just enable and disable buildMode instantly
        {
            DismissActions();
            StopPlacement();

            rightIsPressed = false;
            leftIsPressed = false;
        }
    }
}



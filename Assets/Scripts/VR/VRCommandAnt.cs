using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VRCommandAnt : CommandParent
{
    public XRRayInteractor LeftRayInteractor;
    public XRRayInteractor RightRayInteractor;

    [Header("Input Binds")]
    public XRIDefaultInputActions VRInputActions; //input c# script
    public InputActionMap VRRIHGHTInteraction;//action map
    public InputActionMap VRLEFTInteraction;
    public InputAction RTrigger;//actions
    public InputAction RPrimaryButton; //this is being used in building system!
    public InputAction RSecondaryButton; 
    public InputAction LTrigger;
    public InputAction LPrimaryButton;
    public InputAction LSecondaryButton;

    private int taskValue = 0;

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
        RPrimaryButton = VRLEFTInteraction.FindAction("PrimaryButtonSelect");
        RSecondaryButton = VRRIHGHTInteraction.FindAction("SecondaryButtonSelect");

        LTrigger = VRLEFTInteraction.FindAction("Activate");
        LPrimaryButton = VRLEFTInteraction.FindAction("PrimaryButtonSelect");
        LSecondaryButton = VRLEFTInteraction.FindAction("SecondaryButtonSelect");

        if (VRInputActions != null)
        {
            VRRIHGHTInteraction.Enable();
            VRLEFTInteraction.Enable();

            RTrigger.Enable();
            RPrimaryButton.Enable();
            RSecondaryButton.Enable();

            LTrigger.Enable();
            LPrimaryButton.Enable();
            LSecondaryButton.Enable();
        }

        RTrigger.performed += OnRightTrigger;
        RPrimaryButton.performed += OnRightPrimaryDown;
        RSecondaryButton.performed += OnRightSecondaryDown;

        LTrigger.performed += OnLeftTrigger;
        LPrimaryButton.performed += OnLeftPrimaryDown;
        LSecondaryButton.performed += OnLeftSecondaryDown;
    }

    void Update()
    {
        CheckLassoSelect(RSecondaryButton);
    }


    private void OnRightTrigger(InputAction.CallbackContext context)
    {
        //Debug.Log("VR INPUT: Right trigger Activated " + gameObject.name);

        if(RTrigger.WasPerformedThisFrame() && RightRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            SimpleAntSelect(hit);
        }
    }

    private void OnLeftTrigger(InputAction.CallbackContext context)
    {
        //Debug.Log("VR INPUT: Left trigger Activated " + gameObject.name);

        if(LTrigger.WasPerformedThisFrame() && LeftRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            //Debug.LogWarning("RAYCAST HIT!");
            DirectAnt(hit);
        }
    }

    private void OnLeftPrimaryDown(InputAction.CallbackContext context)
    {
        if (taskValue == 1){
            SwitchToManual();
            taskValue = 0;
            Debug.Log("task = manual");
        }
        else
        {
            SwitchToFood();
            taskValue = 1;
            Debug.Log("task = food");
        }
    }
    private void OnLeftSecondaryDown(InputAction.CallbackContext context)
    {
        
    }

    private void OnRightPrimaryDown(InputAction.CallbackContext context)
    {
        
    }
    private void OnRightSecondaryDown(InputAction.CallbackContext context)
    {
        
    }
}

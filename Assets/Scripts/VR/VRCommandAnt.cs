using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VRCommandAnt : CommandParent
{
    public XRRayInteractor rayInteractor;

    [Header("Input Binds")]
    public XRIDefaultInputActions VRInputActions; //input c# script
    public InputActionMap VRRIHGHTInteraction;//action map
    public InputActionMap VRLEFTInteraction;
    public InputAction RTrigger;//actions
    public InputAction LTrigger;

    /*
    public InputActionReference inputActionReference;
    public InputAction LeftTriggerAction;
    public InputAction XButtonAction; //primary button left
    public InputAction YButtonAction; //secondary button left
    public InputAction RightTriggerAction;
    */
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
        LTrigger = VRLEFTInteraction.FindAction("Activate");

        if (VRInputActions != null)
        {
            VRRIHGHTInteraction.Enable();
            VRLEFTInteraction.Enable();

            RTrigger.Enable();
            LTrigger.Enable();
        }

        RTrigger.performed += OnRightTrigger;
        LTrigger.performed += OnLeftTrigger;

        /*
        if (inputActionReference != null)
        {
            LeftTriggerAction = inputActionReference.action;
            XButtonAction = inputActionReference.action;
            YButtonAction = inputActionReference.action;

            RightTriggerAction = inputActionReference.action;

            LeftTriggerAction?.Enable();
            XButtonAction?.Enable();
            YButtonAction?.Enable();

            RightTriggerAction?.Enable();
        }
        */
    }


    private void OnRightTrigger(InputAction.CallbackContext context)
    {
        Debug.Log("VR INPUT: Right trigger Activated");

        if(rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            SimpleAntSelect(hit);
        }
    }

    private void OnLeftTrigger(InputAction.CallbackContext context)
    {
        Debug.Log("VR INPUT: Left trigger Activated");

        if(rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            Debug.LogWarning("RAYCAST HIT!");
            DirectAnt(hit);
        }
    }

    void Update()
    {
        /*
        if(RightTriggerAction != null)
        {
            
            float triggerValue = RightTriggerAction.ReadValue<float>();
            if (triggerValue > 0.1f)
            {
                if(rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                {
                    SimpleAntSelect(hit);
                }
            }
        }

        if (LeftTriggerAction != null)
        {
            float triggerValue = LeftTriggerAction.ReadValue<float>();
            if (triggerValue > 0.1f)
            {
                if(rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                {
                    DirectAnt(hit);
                }
            }
        }

        if (XButtonAction != null)
        {
            bool buttonValue = XButtonAction.WasPressedThisFrame();
            if (buttonValue)
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
        }
        */
    }

}

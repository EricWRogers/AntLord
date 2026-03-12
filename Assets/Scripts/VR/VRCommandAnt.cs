using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VRCommandAnt : CommandParent
{
    public XRRayInteractor rayInteractor;

    [Header("Input Binds")]
    public InputActionReference inputActionReference;
    public InputAction LeftTriggerAction;
    public InputAction XButtonAction; //primary button left
    public InputAction YButtonAction; //secondary button left
    public InputAction RightTriggerAction;

    private int taskValue = 0;

    void Start()
    {
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
    }

    void Update()
    {
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
    }

}

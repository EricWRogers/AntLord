using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VRCommandAnt : CommandParent
{

    public InputActionReference inputActionReference;
    public XRRayInteractor rayInteractor;
    private InputAction RightTriggerAction;

    void Start()
    {
        if (inputActionReference != null)
        {
            RightTriggerAction = inputActionReference.action;
            if (RightTriggerAction != null)
            {
                RightTriggerAction.Enable();
            }
        }
    }

    void Update()
    {
        if(RightTriggerAction != null)
        {
            
            float triggerValue = RightTriggerAction.ReadValue<float>();
            if (triggerValue > 0.1f)
            {
                //Debug.Log("Trigger pulled"!);


                if(rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                {
                    SimpleAntSelect(hit);
                }
                
            }
        }
    }

}

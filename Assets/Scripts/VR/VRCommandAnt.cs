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
            //read a float value (good for reading triggers)
            float triggerValue = RightTriggerAction.ReadValue<float>();
            if (triggerValue > 0.1f)
            {
                //Debug.Log("Trigger pulled"!);

                //RaycastHit rayHit;

            //     if (rayInteractor.TryGetCurrent3DRaycastHit(out rayHit))
            //     {
                    
            //         Debug.Log("raycast hit:" + rayHit.transform.gameObject.name);

            //         if (rayHit.transform.gameObject.CompareTag("Ant"))
            //         {
            //             Debug.Log("I spy a little ant!");
            //         }
            //     }
            // }

                if(rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                {
                    SimpleAntSelect(hit);
                }
            }

            //check if button was pressed
            /*
            if (triggerAction.WasPressedThisFrame())
            {
                Debug.Log("Button Pressed This Frame!");
            } */
        }
    }
}

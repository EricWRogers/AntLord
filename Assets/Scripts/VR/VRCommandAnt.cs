using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VRCommandAnt : MonoBehaviour
{

    public InputActionReference inputActionReference;
    public XRRayInteractor rayInteractor;

    private InputAction triggerAction;

    void Start()
    {
        if (inputActionReference != null)
        {
            triggerAction = inputActionReference.action;
            if (triggerAction != null)
            {
                triggerAction.Enable();
            }
        }
    }

    void Update()
    {
        if(triggerAction != null)
        {
            //read a float value (good for reading triggers)
            float triggerValue = triggerAction.ReadValue<float>();
            if (triggerValue > 0.1f)
            {
                //Debug.Log("Trigger pulled"!);

                RaycastHit rayHit;

                if (rayInteractor.TryGetCurrent3DRaycastHit(out rayHit))
                {
                    Debug.Log("raycast hit:" + rayHit.transform.gameObject.name);

                    if (rayHit.transform.CompareTag("Ant"))
                    {
                        Debug.Log("I spy a little ant!");
                    }
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

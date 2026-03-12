using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VRCommandAnt : MonoBehaviour
{

    public InputActionReference inputActionReference;
    public XRRayInteractor rayInteractor;
    private InputAction RightTriggerAction;

    public float sphereCastRadius = 2.0f;

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


                if(rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit)){

                    Collider[] hits = Physics.OverlapSphere(hit.point, sphereCastRadius);

                    // Direct click
                    if (hit.transform.CompareTag("Ant") && hit.transform.GetComponent<AntBrain>().antType.teamID == 0)
                    {
                        Debug.Log("I spy a little ant!");
                    }
                    // Secondary "close enough" selection
                    else
                    {
                        
                        foreach (Collider col in hits)
                        {
                            if (col.CompareTag("Ant") && col.transform.GetComponent<AntBrain>().antType.teamID == 0)
                            {
                                Debug.Log("I spy a little ant!");
                                // stop looking after we find the first ant
                                break;
                            }
                        }
                    }
                }
                
            }
        }
    }

}

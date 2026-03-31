using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
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
        LPrimaryButton.performed += OnLeftPrimaryDown;
        LSecondaryButton.performed += OnLeftSecondaryDown;
    }

    void Update()
    {
        // THIS WAS MEANT TO USE THE LASSO FUNCTION

        if (LSecondaryButton.WasPerformedThisFrame())
        {
            Debug.LogWarning("STARTING!");
            //Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                shiftDragStart = hit.point;
                shiftDragging = true;

                if (selectionRing) selectionRing.Show(shiftDragStart, 0.1f);
            }
        }


        if (shiftDragging)
        {
            // If shift was released mid-drag, cancel 
            if (!LSecondaryButton.IsInProgress())
            {
                shiftDragging = false;
                if (selectionRing) selectionRing.Hide();
            }
            else
            {
                // Update ring while LMB held
                if (shiftDragging)
                {
                    Debug.LogWarning("UPDATING");

                    //Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                    {
                        
                        // center moves toward mouse, radius is half the distance
                        Vector3 current = hit.point;
                        Vector3 center = (shiftDragStart + current) * 0.5f;
                        float radius = Vector3.Distance(shiftDragStart, current) * 0.5f;

                        if (selectionRing) selectionRing.Show(center, radius);
                    }
                    else
                    {
                        
                        if (selectionRing) selectionRing.Hide();
                    }
                }

                // Release drag on mouse up, even if shift isn't held
                if (LSecondaryButton.WasCompletedThisFrame())
                {
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                    {
                        Vector3 end = hit.point;
                        Vector3 center = (shiftDragStart + end) * 0.5f;
                        float radius = Vector3.Distance(shiftDragStart, end) * 0.5f;

                        // hide ring
                        if (selectionRing) selectionRing.Hide();

                    
                        ClearSelectionVisualsOnly();
                        selectedAnts.Clear();
                        selectedLeader = null;

                        Collider[] hits = Physics.OverlapSphere(center, radius);
                        foreach (var col in hits)
                        {
                            if (!col.CompareTag("Ant")) continue;
                            var brain = col.GetComponent<AntBrain>();
                            if (brain == null) continue;

                            if (brain.antType.teamID == 0)
                            {
                                selectedAnts.Add(col.gameObject);
                                SetGlow(col.gameObject, selectedColor, selectedIntensity);
                            }
                        }
                    }
                    else
                    {
                        if (selectionRing) selectionRing.Hide();
                    }

                    shiftDragging = false;
                }
            }
        }

        // END LASSO FUNCTION
    }


    private void OnRightTrigger(InputAction.CallbackContext context)
    {
        //Debug.Log("VR INPUT: Right trigger Activated " + gameObject.name);

        if(rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            SimpleAntSelect(hit);
        }
    }

    private void OnLeftTrigger(InputAction.CallbackContext context)
    {
        //Debug.Log("VR INPUT: Left trigger Activated " + gameObject.name);

        if(rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
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
}

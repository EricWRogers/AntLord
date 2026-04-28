using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

//Left Primary button toggles dig mode and shovel
//Right Trigger digs voxels
//Left Trigger places voxels

public class VRTerrainModSystem : RayOMayhem
{
    [Header("Input Binds")]
    public XRIDefaultInputActions VRInputActions; //input c# script
    public InputAction RTrigger;//actions
    public InputAction LTrigger;//actions
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

        // RTrigger = VRRIGHTInteraction.FindAction("Activate");
        // LTrigger = VRLEFTInteraction.FindAction("Activate");
        // LSecondaryButton = VRLEFTInteraction.FindAction("SecondaryButtonSelect");

        RTrigger = VRInputActions.XRIRightInteraction.Activate;
        LTrigger = VRInputActions.XRILeftInteraction.Activate;
        LSecondaryButton = VRInputActions.XRILeftInteraction.SecondaryButtonSelect;

        LSecondaryButton.performed += OnLeftSecondary;
        RTrigger.performed += OnRightTrigger;
        LTrigger.performed += OnLeftTrigger;
        OnClicked += ProcessVoxelMod;
    }

    //     if (VRInputActions != null)
    //     {
    //         VRRIGHTInteraction.Enable();
    //         VRLEFTInteraction.Enable();

    //         RTrigger.Enable();
    //         LPrimaryButton.Enable();
    //         LTrigger.Enable();
    //     }

    //     LSecondaryButton.performed += OnLeftSecondary;
    //     OnClicked += ProcessVoxelMod;
    // }
    protected override void Update()
    {
        base.Update();

        if (!inModMode) return;

        if (RTrigger.ReadValue<float>() > 0.1f || LTrigger.ReadValue<float>() > 0.1f)
        {
            TriggerClick();
        }
        
    }
    private void OnLeftSecondary(InputAction.CallbackContext context)
    {
        inModMode = !inModMode;
        if (shovel != null) shovel.SetActive(inModMode);
        Debug.Log($"Mod mode {inModMode}");
        if (!inModMode) TriggerExit();
    }

    private void OnRightTrigger(InputAction.CallbackContext context){}
    private void OnLeftTrigger(InputAction.CallbackContext context){}


    private void ProcessVoxelMod()//this is called by the TriggerClick event, which is called by the input actions
    {
        if (RTrigger.ReadValue<float>() > 0.1f && shovel.activeInHierarchy)
        {
            if (rightRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                Debug.Log($"Right Ray Hit at: {hit.point}");
                HandleVoxelInput(hit.point, isBreaking: true);
            }
        }
        if (LTrigger.ReadValue<float>() > 0.1f && resourceManager != null && resourceManager.sand > 0)
        {
            if (leftRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                Debug.Log($"Left Ray Hit at: {hit.point}");
                HandleVoxelInput(hit.point, isBreaking: false);
            }
        }
    }
}
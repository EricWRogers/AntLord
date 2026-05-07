using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class RayOMayhem : MonoBehaviour
{
    [Header("VR Fallback References")]
    public XRRayInteractor leftRayInteractor;
    public XRRayInteractor rightRayInteractor;

    [Header("Desktop References")]
    public Camera cam;

    [Header("Voxel Settings")]
    public GameObject shovel;
    public MarchingCubeManager voxelManager;
    public LayerMask layerMask = ~0; 
    public ResourceManager resourceManager;

    public enum VoxelAction { None, Breaking, Placing }
    private VoxelAction currentAction = VoxelAction.None;

    public bool didModifyVoxel = false;
    public bool trailEnd = false;
    public bool inModMode = false;
    public float adjustmentRate = 0.1f;
    private float nextAdjustmentTime;

    [Header("Limits (optional)")]
    public bool useHeightLimits = false;
    public float breakMinY = 1.4f;
    public float placeMaxY = 8.4f;
    public event Action OnClicked, OnExit;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    protected virtual void Update()
    {
        if (voxelManager == null) return;

        //HANDLE BREAKING (Left Click OR Right Controller Trigger)
        bool isBreakingInput = Input.GetMouseButton(0);

        if (isBreakingInput && shovel.activeInHierarchy)
        {
            if (TryGetHitPoint(out Vector3 hitPoint, isLeftHand: false))
            {
                currentAction = VoxelAction.Breaking;
                HandleVoxelInput(hitPoint, isBreaking: true);
            }
        }

        //HANDLE PLACING (Right Click OR Left Controller Trigger)
        bool isPlacingInput = Input.GetMouseButton(1);

        if (isPlacingInput)
        {
            if (resourceManager != null && resourceManager.sand <= 0) return;

            if (TryGetHitPoint(out Vector3 hitPoint, isLeftHand: true))
            {
                currentAction = VoxelAction.Placing;
                HandleVoxelInput(hitPoint, isBreaking: false);
            }
        }

        //Reset upon no input
        bool anyInput = isBreakingInput || isPlacingInput;
        if (!anyInput && currentAction != VoxelAction.None)
        {
            didModifyVoxel = false;
            currentAction = VoxelAction.None;
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            EnableTerrainMod();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            DisableTerrainMod();
        }
    }

    private bool TryGetHitPoint(out Vector3 hitPoint, bool isLeftHand)
    {
        hitPoint = Vector3.zero;

        XRRayInteractor activeInteractor = isLeftHand ? leftRayInteractor : rightRayInteractor;
        
        if (activeInteractor != null && activeInteractor.gameObject.activeInHierarchy)
        {
            if (activeInteractor.TryGetCurrent3DRaycastHit(out RaycastHit vrHit))
            {
                hitPoint = vrHit.point;
                return true;
            }
        }

        if (cam != null)
        {
            Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mouseRay, out RaycastHit mouseHit, 500f, layerMask))
            {
                hitPoint = mouseHit.point;
                return true;
            }
        }
        return false;
    }

    public void HandleVoxelInput(Vector3 hitPoint, bool isBreaking)
    {
        if (Time.time < nextAdjustmentTime) return;

        if (useHeightLimits)
        {
            if (isBreaking && hitPoint.y < breakMinY) return;
            if (!isBreaking && hitPoint.y > placeMaxY) return;
        }

        float targetValue = isBreaking ? 1.0f : 0.0f;
        //modify the terrain at the hit point
        voxelManager.ModifyVoxel(hitPoint, 1.0f, targetValue);

        if (resourceManager != null)
        {
            if (isBreaking) resourceManager.AddSand(1);
            else resourceManager.SubSand(1);
        }

        nextAdjustmentTime = Time.time + adjustmentRate;
        didModifyVoxel = true;
    }

    public void EnableTerrainMod()
    {
        inModMode = true;
        if (shovel != null) shovel.SetActive(inModMode);
    } 

    public void DisableTerrainMod()
    {
        inModMode = false;
        if (shovel != null) shovel.SetActive(inModMode);
    }

    //these are for VR invoking events
    //you cant directly invoke another classes events even if theyre inherited
    protected void TriggerClick()
    {
        OnClicked?.Invoke();
    }

    protected void TriggerExit()
    {
        OnExit?.Invoke();
    }
}
using System;
using UnityEditor;
using UnityEngine;


public class RayOMayhem : MonoBehaviour
{
    public Camera cam;
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
        // Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (voxelManager == null) return;

        // if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        //     return;

        // LEFT mouse: break
        if (Input.GetMouseButton(0))
        {
            if (!shovel.activeInHierarchy) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.green);

            if (Physics.Raycast(ray, out RaycastHit hit, 500f, layerMask))
            {
                currentAction = VoxelAction.Breaking;
                HandleVoxelInput(hit.point, isBreaking: true);
                Debug.Log("Ray Hit collider" + hit.collider.name);
            }
            // currentAction = VoxelAction.Breaking;
            // HandleVoxelInput(targetValue: 1.0f, isBreaking: true);
        }
        // RIGHT mouse: place
        else if (Input.GetMouseButton(1))
        {
            if (resourceManager != null && resourceManager.sand <= 0) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.green);

            if (Physics.Raycast(ray, out RaycastHit hit, 500f, layerMask))
            {
                currentAction = VoxelAction.Placing;
                HandleVoxelInput(hit.point, isBreaking: false);
                Debug.Log("Ray Hit collider" + hit.collider.name);
            } 
            // currentAction = VoxelAction.Placing;
            // HandleVoxelInput(targetValue: 0.0f, isBreaking: false);
        }
        
        // Reset state when mouse buttons are released
        if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && currentAction != VoxelAction.None)
        {
            trailEnd = true;
            didModifyVoxel = false;
            currentAction = VoxelAction.None;
            Debug.Log("End Of Trail");
        }
    }

    public void HandleVoxelInput(Vector3 hitPoint, bool isBreaking)//, float targetValue, bool isBreaking)
    {
        if (Time.time < nextAdjustmentTime) return;

        float targetValue = isBreaking ? 1.0f : 0.0f;

        // Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        // if (!Physics.Raycast(ray, out RaycastHit hit, 500f, layerMask, QueryTriggerInteraction.Ignore))
        // {
        //     Debug.Log("Raycast did NOT hit voxel terrain. Check layerMask + collider.");
        //     return;
        // }
        
        //Debug.Log($"Hit: {hit.collider.name} layer={LayerMask.LayerToName(hit.collider.gameObject.layer)} point={hit.point}");

        if (useHeightLimits)
        {
            if (isBreaking && hitPoint.y < breakMinY) return;
            if (!isBreaking && hitPoint.y > placeMaxY) return;
        }
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
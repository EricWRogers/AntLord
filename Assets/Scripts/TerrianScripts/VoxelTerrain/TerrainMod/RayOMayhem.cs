using NUnit.Framework;
using UnityEngine;

public class RayOMayhem : MonoBehaviour
{
    public Camera cam;
    public MarchingCubeManager voxelManager;
    public ItemPickup itemPickup;
    public LayerMask layerMask = ~0; 
    public ResourceManager resourceManager;

    public enum VoxelAction { None, Breaking, Placing }
    private VoxelAction currentAction = VoxelAction.None;

    public bool didModifyVoxel = false;
    public bool trailEnd = false;

    public float adjustmentRate = 0.1f;
    private float nextAdjustmentTime;

    [Header("Limits (optional)")]
    public bool useHeightLimits = false;
    public float breakMinY = 1.4f;
    public float placeMaxY = 8.4f;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.green);

        if (voxelManager == null) return;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            return;

        // LEFT mouse: break
        if (Input.GetMouseButton(0))
        {
            currentAction = VoxelAction.Breaking;
            HandleVoxelInput(targetValue: 1.0f, isBreaking: true);
        }
        // RIGHT mouse: place
        else if (Input.GetMouseButton(1))
        {
            if (resourceManager != null && resourceManager.sand <= 0) return;

            currentAction = VoxelAction.Placing;
            HandleVoxelInput(targetValue: 0.0f, isBreaking: false);
        }
        
        if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && currentAction != VoxelAction.None)
        {
            trailEnd = true;
            didModifyVoxel = false;
            currentAction = VoxelAction.None;
            Debug.Log("End Of Trail");
        }

        if (Input.GetKeyDown(KeyCode.F) && Physics.Raycast(ray, out RaycastHit hit, 500f))
        {
            itemPickup.OnMouseDown();
        }
    }

    private void HandleVoxelInput(float targetValue, bool isBreaking)
    {
        if (Time.time < nextAdjustmentTime) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 500f, layerMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Raycast did NOT hit voxel terrain. Check layerMask + collider.");
            return;
        }
        
        Debug.Log($"Hit: {hit.collider.name} layer={LayerMask.LayerToName(hit.collider.gameObject.layer)} point={hit.point}");

        if (useHeightLimits)
        {
            if (isBreaking && hit.point.y < breakMinY) return;
            if (!isBreaking && hit.point.y > placeMaxY) return;
        }

        voxelManager.ModifyVoxel(hit.point, 1.0f, targetValue);

        if (resourceManager != null)
        {
            if (isBreaking) resourceManager.AddSand(1);
            else resourceManager.SubSand(1);
        }

        nextAdjustmentTime = Time.time + adjustmentRate;
        didModifyVoxel = true;
    }
}
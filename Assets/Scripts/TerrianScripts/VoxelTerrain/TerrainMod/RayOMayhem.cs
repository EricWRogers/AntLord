using UnityEngine;

public class RayOMayhem : MonoBehaviour
{
    public Camera cam;
    public MarchingCubeManager voxelManager; 
    public LayerMask layerMask;
    public ResourceManager resourceManager;
    public enum VoxelAction { None , Breaking, Placing }
    public bool didModifyVoxel = false;
    public bool trailEnd = false;
    public float adjustmentRate = 0.1f;
    private float nextAdjustmentTime;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    private VoxelAction currentAction = VoxelAction.None;
    void Update()
    {
        if (voxelManager == null) return;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            return;

        //mouse Button 0: Break Voxel (Setting height to 1.0f "Air")
        if (Input.GetMouseButton(0))
        {
            currentAction = VoxelAction.Breaking;
            HandleVoxelInput(1.0f, 1.4f, true);
        }

        //mouse Button 1: Place Voxel (Setting height to 0.0f "Solid")
        if (Input.GetMouseButton(1) && resourceManager.sand > 0)
        {
            currentAction = VoxelAction.Placing;
            HandleVoxelInput(0.0f, 8.4f, false);
        }

        //update the navmesh only after the player releases the mouse button
        if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && currentAction != VoxelAction.None)
        {
            trailEnd = true;
            Debug.Log("End Of Trail");
            didModifyVoxel = false;
            currentAction = VoxelAction.None;
        }
    }
    private void HandleVoxelInput(float targetValue, float yThreshold, bool isBreaking)
    {
        if (Time.time >= nextAdjustmentTime)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 500f, layerMask))
            {
                //limit checks
                bool withinHeightLimit = isBreaking ? (hit.point.y >= yThreshold) : (hit.point.y <= yThreshold);

                if (withinHeightLimit)
                {
                    voxelManager.ModifyVoxel(hit.point, 1.0f, targetValue);
                    
                    if (isBreaking)
                    {
                        resourceManager.AddSand(1);
                    }
                    else
                    {
                        resourceManager.SubSand(1);
                    }

                    nextAdjustmentTime = Time.time + adjustmentRate;
                    didModifyVoxel = true;
                }
            }
        }
    }
}
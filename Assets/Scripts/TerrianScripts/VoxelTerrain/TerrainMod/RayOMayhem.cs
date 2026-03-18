using UnityEngine;

public class RayOMayhem : MonoBehaviour
{
    public Camera cam;
    public MarchingCubeManager voxelManager; 
    public LayerMask layerMask;

    public bool wasBuilding = false;
    public bool trailEnd = false;

    public float adjustmentRate = 0.1f;
    private float nextAdjustmentTime;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        //safety check if manager isn't assigned
        if (voxelManager == null) return;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            return;

        //mouse Button 0: Break Voxel (Setting height to 1.0f / "Air")
        if (Input.GetMouseButton(0))
        {
            HandleVoxelInput(1.0f, 1.4f, true);
        }

        //mouse Button 1: Place Voxel (Setting height to 0.0f / "Solid")
        if (Input.GetMouseButton(1))
        {
            HandleVoxelInput(0.0f, 8.4f, false);
        }

        //update the navmesh only after the player releases the mouse button
        if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && wasBuilding)
        {
            trailEnd = true;
            Debug.Log("End Of Trail");
            wasBuilding = false;
        }
    }
    private void HandleVoxelInput(float targetValue, float yThreshold, bool isBreaking)
    {
        if (Time.time >= nextAdjustmentTime)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 500f, layerMask))
            {
                //if breaking, check if we are above floor. If placing, check if we are below ceiling.
                bool withinHeightLimit = isBreaking ? (hit.point.y >= yThreshold) : (hit.point.y <= yThreshold);

                if (withinHeightLimit)
                {
                    //call the Manager's generic Modify method
                    voxelManager.ModifyVoxel(hit.point, 1.0f, targetValue);
                    
                    nextAdjustmentTime = Time.time + adjustmentRate;
                    wasBuilding = true;
                }
            }
        }
    }
}
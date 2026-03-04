using UnityEngine;

public class RayOMayhem : MonoBehaviour
{
    public Camera cam;
    public MarchingCubes voxelTerrain;

    public bool wasBuilding = false;

    public bool trailEnd = false;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    public float adjustmentRate = 0.1f;
    private float nextAdjustmentTime;

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            return;

        // if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        // {
        //     voxelTerrain.TriggerNavMeshUpdate();
        //     Debug.Log("I want navmesh update pls.");
        // }


        //right click to break voxel block
        if (Input.GetMouseButton(0))
        {
            if (Time.time >= nextAdjustmentTime)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 500f, voxelTerrain.layerMask))
                {
                    Debug.Log("Y: " + hit.point.y);
                    if (hit.point.y >= 1.4f)
                    {
                    //apply voxel breaking logic here;
                    voxelTerrain.BreakVoxel(hit.point, 1.0f);
                    nextAdjustmentTime = Time.time + adjustmentRate;
                    wasBuilding = true;
                    }
                }
            }
        }

        



        //left click to add voxel block
        if (Input.GetMouseButton(1))
        {
            if (Time.time >= nextAdjustmentTime)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 500f, voxelTerrain.layerMask))
                {
                    Debug.Log("Y: " + hit.point.y);
                    if (hit.point.y <= 8.4f)
                    {
                    //apply voxel breaking logic here;
                    voxelTerrain.PlaceVoxel(hit.point, 1.0f);
                    nextAdjustmentTime = Time.time + adjustmentRate;
                    wasBuilding = true;
                    }
                }
            }
        }

        if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) ) && wasBuilding)
        {
            trailEnd = true;
            wasBuilding = false;
            Debug.Log("Trail Has ended");
        }
    }
}
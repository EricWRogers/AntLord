using UnityEngine;

public class RayOMayhem : MonoBehaviour
{
    public Camera cam;
    public MarchingCubes voxelTerrain;
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

        if (Input.GetKeyDown(KeyCode.R))
        {
            voxelTerrain.ResetFlat(10); //reset terrain to flat with height of 10
        }

        //right click to break voxel block
        if (Input.GetMouseButton(0))
        {
            if (Time.time >= nextAdjustmentTime)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 500f, voxelTerrain.layerMask))
                {
                    //apply voxel breaking logic here;
                    voxelTerrain.BreakVoxel(hit.point);
                    nextAdjustmentTime = Time.time + adjustmentRate;
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
                    //apply voxel placing logic here;
                    voxelTerrain.PlaceVoxel(hit.point);
                    nextAdjustmentTime = Time.time + adjustmentRate;
                }
            }
        }
    }
}
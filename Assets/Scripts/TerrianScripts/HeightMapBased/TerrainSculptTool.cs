using UnityEngine;

public class TerrainSculptTool : MonoBehaviour
{
    public Camera cam;
    public HeightmapTerrain hm;

    [Header("Brush")]
    public float radiusWorld = 4f;
    public float strengthPerSecond = 0.15f;
    public AnimationCurve falloff = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Raycast")]
    public LayerMask terrainMask;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            return;

        if (!hm) return;

        if (Input.GetKeyDown(KeyCode.R))
            hm.ResetFlat(0.5f);

        // Left mouse = raise, Right mouse = lower
        int dir = 0;
        if (Input.GetMouseButton(0)) dir = +1;
        else if (Input.GetMouseButton(1)) dir = -1;
        else return;

        //right click to lower

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, terrainMask))
        {
            hm.ApplyBrush(hit.point, radiusWorld, strengthPerSecond, dir, falloff);
        }
    }
}

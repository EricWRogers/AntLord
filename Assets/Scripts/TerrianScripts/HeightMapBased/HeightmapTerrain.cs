using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class HeightmapTerrain : MonoBehaviour
{
    public Terrain terrain;
    public TerrainData data;

    [Header("Sculpt Limits")]
    public float minNormalized = 0f;
    public float maxNormalized = 1f;

    [Header("Water")]
    [Range(0f, 1f)]
    public float waterLevelNormalized = 0.25f; // fixed elevation water plane will use

    int res; // heightmap resolution
    float[,] heights; 

    void Awake()
    {
        terrain = GetComponent<Terrain>();
        data = terrain.terrainData;
        res = data.heightmapResolution;

        
        heights = data.GetHeights(0, 0, res, res);
    }

    
    public void WorldToHM(Vector3 world, out int x, out int z)
    {
        Vector3 local = world - terrain.transform.position;

        float nx = Mathf.Clamp01(local.x / data.size.x);
        float nz = Mathf.Clamp01(local.z / data.size.z);

        
        x = Mathf.RoundToInt(nx * (res - 1));
        z = Mathf.RoundToInt(nz * (res - 1));
    }

    public float GetHeightNormalizedAtWorld(Vector3 world)
    {
        WorldToHM(world, out int x, out int z);
        return heights[z, x]; 
    }

    public float GetHeightWorldAtWorld(Vector3 world)
    {
        float hn = GetHeightNormalizedAtWorld(world);
        return terrain.transform.position.y + hn * data.size.y;
    }

    public bool IsWaterAtWorld(Vector3 world)
    {
        return GetHeightNormalizedAtWorld(world) <= waterLevelNormalized;
    }

    
    public void ApplyBrush(Vector3 worldCenter, float radiusWorld, float strengthPerSecond, int direction, AnimationCurve falloff)
    {
        WorldToHM(worldCenter, out int cx, out int cz);

        // convert the brush to cellss of height map
        float cellsPerWorldX = (res - 1) / data.size.x;
        float cellsPerWorldZ = (res - 1) / data.size.z;

        int rx = Mathf.CeilToInt(radiusWorld * cellsPerWorldX);
        int rz = Mathf.CeilToInt(radiusWorld * cellsPerWorldZ);

        int x0 = Mathf.Clamp(cx - rx, 0, res - 1);
        int x1 = Mathf.Clamp(cx + rx, 0, res - 1);
        int z0 = Mathf.Clamp(cz - rz, 0, res - 1);
        int z1 = Mathf.Clamp(cz + rz, 0, res - 1);

        float dt = Time.deltaTime;

        for (int z = z0; z <= z1; z++)
        {
            for (int x = x0; x <= x1; x++)
            {
                float dx = (x - cx) / (float)rx;
                float dz = (z - cz) / (float)rz;
                float dist01 = Mathf.Sqrt(dx * dx + dz * dz);

                if (dist01 > 1f) continue;

                float w = falloff != null ? falloff.Evaluate(1f - dist01) : (1f - dist01);
                float delta = direction * strengthPerSecond * dt * w;

                heights[z, x] = Mathf.Clamp(heights[z, x] + delta, minNormalized, maxNormalized);
            }
        }

        // Push only the edited region
        int width = (x1 - x0) + 1;
        int height = (z1 - z0) + 1;

        float[,] patch = new float[height, width];
        for (int z = 0; z < height; z++)
            for (int x = 0; x < width; x++)
                patch[z, x] = heights[z0 + z, x0 + x];

        data.SetHeightsDelayLOD(x0, z0, patch);
        terrain.Flush(); //update the visuals
    }


    public void ResetFlat(float flatNormalized = 0.5f)
    {
        flatNormalized = Mathf.Clamp(flatNormalized, minNormalized, maxNormalized);

        for (int z = 0; z < res; z++)
            for (int x = 0; x < res; x++)
                heights[z, x] = flatNormalized;

        data.SetHeightsDelayLOD(0, 0, heights);
        terrain.Flush();
    }
}

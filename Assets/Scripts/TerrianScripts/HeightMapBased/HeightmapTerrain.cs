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
    public float waterLevelNormalized = 0.25f;

    [Header("Sand Behavior")]
    public bool sandMode = true;
    public float angleOfReposeDeg = 33f;
    public int slumpIterations = 3;
    [Range(0f, 1f)]
    public float slumpStrength = 0.5f;

    [Header("Physics Push (prevents objects being swallowed)")]
    public LayerMask pushMask;      
    public float pushPadding = 0.02f;

    int res;
    float[,] heights;

    
    public float[,] Heights => heights;
    public int Resolution => res;

    void Awake()
    {
        terrain = GetComponent<Terrain>();
        data = terrain.terrainData;
        res = data.heightmapResolution;

        // Cache the full heightmap once
        heights = data.GetHeights(0, 0, res, res);
    }

    public void WorldToHM(Vector3 world, out int x, out int z)
    {
        Vector3 local = world - terrain.transform.position;
        float nx = Mathf.Clamp01(local.x / data.size.x);
        float nz = Mathf.Clamp01(local.z / data.size.z);

        x = Mathf.FloorToInt(nx * (res - 1));
        z = Mathf.FloorToInt(nz * (res - 1));
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

    public bool IsWaterAtWorld(Vector3 world) => GetHeightNormalizedAtWorld(world) <= waterLevelNormalized;

    

    public void CommitRegion(int x0, int z0, int x1, int z1)
    {
        x0 = Mathf.Clamp(x0, 0, res - 1);
        x1 = Mathf.Clamp(x1, 0, res - 1);
        z0 = Mathf.Clamp(z0, 0, res - 1);
        z1 = Mathf.Clamp(z1, 0, res - 1);

        int width = (x1 - x0) + 1;
        int height = (z1 - z0) + 1;

        float[,] patch = new float[height, width];
        for (int zz = 0; zz < height; zz++)
            for (int xx = 0; xx < width; xx++)
                patch[zz, xx] = heights[z0 + zz, x0 + xx];

        data.SetHeights(x0, z0, patch);  
        terrain.Flush();   

        LiftRigidbodiesInRegion(x0, z0, x1, z1);
    }

   

    void ApplySandSlump(int x0, int z0, int width, int height)
    {
        if (!sandMode) return;

        float dxWorld = data.size.x / (res - 1);
        float dzWorld = data.size.z / (res - 1);

        float maxDhX = Mathf.Tan(angleOfReposeDeg * Mathf.Deg2Rad) * dxWorld;
        float maxDhZ = Mathf.Tan(angleOfReposeDeg * Mathf.Deg2Rad) * dzWorld;

        int pad = 2;
        int ax0 = Mathf.Clamp(x0 - pad, 0, res - 1);
        int az0 = Mathf.Clamp(z0 - pad, 0, res - 1);
        int ax1 = Mathf.Clamp(x0 + width - 1 + pad, 0, res - 1);
        int az1 = Mathf.Clamp(z0 + height - 1 + pad, 0, res - 1);

        for (int iter = 0; iter < slumpIterations; iter++)
        {
            for (int z = az0; z <= az1; z++)
            {
                for (int x = ax0; x <= ax1; x++)
                {
                    RelaxPair(x, z, x + 1, z, maxDhX);
                    RelaxPair(x, z, x - 1, z, maxDhX);
                    RelaxPair(x, z, x, z + 1, maxDhZ);
                    RelaxPair(x, z, x, z - 1, maxDhZ);
                }
            }
        }
    }

    void RelaxPair(int xA, int zA, int xB, int zB, float maxDhWorld)
    {
        if (xB < 0 || xB >= res || zB < 0 || zB >= res) return;

        float hA = heights[zA, xA] * data.size.y;
        float hB = heights[zB, xB] * data.size.y;

        float dh = hA - hB;
        if (dh <= maxDhWorld) return;

        float excess = dh - maxDhWorld;
        float move = excess * slumpStrength * 0.5f;

        hA -= move;
        hB += move;

        heights[zA, xA] = Mathf.Clamp01(hA / data.size.y);
        heights[zB, xB] = Mathf.Clamp01(hB / data.size.y);
    }

    

    public void ApplyBrush(Vector3 worldCenter, float radiusWorld, float strengthPerSecond, int direction, AnimationCurve falloff)
    {
        WorldToHM(worldCenter, out int cx, out int cz);

        float cellsPerWorldX = (res - 1) / data.size.x;
        float cellsPerWorldZ = (res - 1) / data.size.z;

        int rx = Mathf.Max(1, Mathf.CeilToInt(radiusWorld * cellsPerWorldX));
        int rz = Mathf.Max(1, Mathf.CeilToInt(radiusWorld * cellsPerWorldZ));

        int x0 = Mathf.Clamp(cx - rx, 0, res - 1);
        int x1 = Mathf.Clamp(cx + rx, 0, res - 1);
        int z0 = Mathf.Clamp(cz - rz, 0, res - 1);
        int z1 = Mathf.Clamp(cz + rz, 0, res - 1);

        float dt = Time.deltaTime;

        for (int z = z0; z <= z1; z++)
        {
            for (int x = x0; x <= x1; x++)
            {
                // Ellipse distance 
                float dx = x - cx;
                float dz = z - cz;
                float dist01 = Mathf.Sqrt((dx * dx) / (rx * rx) + (dz * dz) / (rz * rz));
                if (dist01 > 1f) continue;

                float w = falloff != null ? falloff.Evaluate(1f - dist01) : (1f - dist01);
                float delta = direction * strengthPerSecond * dt * w;

                heights[z, x] = Mathf.Clamp(heights[z, x] + delta, minNormalized, maxNormalized);
            }
        }

       
        int width = (x1 - x0) + 1;
        int height = (z1 - z0) + 1;
        ApplySandSlump(x0, z0, width, height);

        CommitRegion(x0, z0, x1, z1);
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

   

    void LiftRigidbodiesInRegion(int x0, int z0, int x1, int z1)
    {
        if (pushMask.value == 0) return;

        Vector3 tPos = terrain.transform.position;

        float minX = tPos.x + (x0 / (float)(res - 1)) * data.size.x;
        float maxX = tPos.x + (x1 / (float)(res - 1)) * data.size.x;
        float minZ = tPos.z + (z0 / (float)(res - 1)) * data.size.z;
        float maxZ = tPos.z + (z1 / (float)(res - 1)) * data.size.z;

        Bounds b = new Bounds(
            new Vector3((minX + maxX) * 0.5f, tPos.y + data.size.y * 0.5f, (minZ + maxZ) * 0.5f),
            new Vector3(Mathf.Abs(maxX - minX), data.size.y * 2f, Mathf.Abs(maxZ - minZ))
        );

        Collider[] cols = Physics.OverlapBox(b.center, b.extents, Quaternion.identity, pushMask);
        for (int i = 0; i < cols.Length; i++)
        {
            Rigidbody rb = cols[i].attachedRigidbody;
            if (rb == null) continue;

            Vector3 p = rb.position;
            float groundY = terrain.SampleHeight(p) + terrain.transform.position.y;

            float bottomY = cols[i].bounds.min.y;
            if (bottomY < groundY + pushPadding)
            {
                float lift = (groundY + pushPadding) - bottomY;
                rb.position += new Vector3(0f, lift, 0f);

                
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, 0.5f), rb.linearVelocity.z);
            }
        }
    }
}
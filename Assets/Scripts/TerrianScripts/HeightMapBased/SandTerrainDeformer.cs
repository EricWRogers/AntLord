using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Terrain))]
public class SandTerrainDeformer : MonoBehaviour
{
    public Terrain terrain;
    public TerrainData data;

    [Header("Imprinters")]
    public LayerMask imprinterMask;
    public float scanRadius = 40f;  
    public Transform focus;         

    [Header("Sand Relax")]
    public bool sandSlump = true;
    public float angleOfReposeDeg = 33f;
    public int slumpIterations = 2;
    public float slumpStrength = 0.4f;

    int res;
    float[,] heights;

    void Awake()
    {
        terrain = GetComponent<Terrain>();
        data = terrain.terrainData;
        res = data.heightmapResolution;

        
        heights = data.GetHeights(0, 0, res, res);
        if (focus == null && Camera.main != null) focus = Camera.main.transform;
    }

    void FixedUpdate()
    {
        if (focus == null) return;

        Collider[] hits = Physics.OverlapSphere(focus.position, scanRadius, imprinterMask);
        if (hits == null || hits.Length == 0) return;

        bool changed = false;

        foreach (var col in hits)
        {
            var imp = col.GetComponentInParent<SandImprinter>();
            if (imp == null) continue;
            if (!imp.ShouldImprint()) continue;

            changed |= Imprint(imp);
        }

        if (changed)
        {
            data.SetHeightsDelayLOD(0, 0, heights);
            terrain.Flush();
        }
    }

    bool Imprint(SandImprinter imp)
    {
        Vector3 p = imp.transform.position;

       
        WorldToHM(p, out int cx, out int cz);

        float cellsPerWorldX = (res - 1) / data.size.x;
        float cellsPerWorldZ = (res - 1) / data.size.z;

        int rX = Mathf.CeilToInt(imp.radius * cellsPerWorldX);
        int rZ = Mathf.CeilToInt(imp.radius * cellsPerWorldZ);

        int x0 = Mathf.Clamp(cx - rX, 0, res - 1);
        int x1 = Mathf.Clamp(cx + rX, 0, res - 1);
        int z0 = Mathf.Clamp(cz - rZ, 0, res - 1);
        int z1 = Mathf.Clamp(cz + rZ, 0, res - 1);

        
        float targetWorldY = imp.BottomY();
        float terrainBaseY = terrain.transform.position.y;

        
        float targetN = Mathf.Clamp01((targetWorldY - terrainBaseY) / data.size.y);

        // Limit depth
        float currentCenterN = heights[cz, cx];
        float maxDepthN = imp.maxDepth / data.size.y;
        float minAllowedN = Mathf.Clamp01(currentCenterN - maxDepthN);
        targetN = Mathf.Max(targetN, minAllowedN);

        float dt = Time.fixedDeltaTime;
        bool changed = false;
        float removed = 0f;

        for (int z = z0; z <= z1; z++)
        {
            for (int x = x0; x <= x1; x++)
            {
                float dx = (x - cx) / (float)rX;
                float dz = (z - cz) / (float)rZ;
                float dist01 = Mathf.Sqrt(dx * dx + dz * dz);
                if (dist01 > 1f) continue;

                float w = 1f - dist01; 
                float h = heights[z, x];

                
                if (h > targetN)
                {
                    float newH = Mathf.Lerp(h, targetN, imp.strength * dt * w);
                    removed += (h - newH);
                    heights[z, x] = newH;
                    changed = true;
                }
            }
        }

       
        if (changed && imp.makeBerm && removed > 0f)
        {
            AddBerm(cx, cz, imp, removed);
        }

       
        if (changed && sandSlump)
        {
            int width = (x1 - x0) + 1;
            int height = (z1 - z0) + 1;
            ApplySandSlump(x0, z0, width, height);
        }

        return changed;
    }

    void AddBerm(int cx, int cz, SandImprinter imp, float removedN)
    {
        float cellsPerWorldX = (res - 1) / data.size.x;
        float cellsPerWorldZ = (res - 1) / data.size.z;

        float bermRadius = imp.radius * imp.bermRadiusMultiplier;
        int rX = Mathf.CeilToInt(bermRadius * cellsPerWorldX);
        int rZ = Mathf.CeilToInt(bermRadius * cellsPerWorldZ);

        int x0 = Mathf.Clamp(cx - rX, 0, res - 1);
        int x1 = Mathf.Clamp(cx + rX, 0, res - 1);
        int z0 = Mathf.Clamp(cz - rZ, 0, res - 1);
        int z1 = Mathf.Clamp(cz + rZ, 0, res - 1);

        
        float inner = 1f / imp.bermRadiusMultiplier; 
        float toAdd = removedN * imp.bermStrength;

        for (int z = z0; z <= z1; z++)
        {
            for (int x = x0; x <= x1; x++)
            {
                float dx = (x - cx) / (float)rX;
                float dz = (z - cz) / (float)rZ;
                float dist01 = Mathf.Sqrt(dx * dx + dz * dz);
                if (dist01 < inner || dist01 > 1f) continue;

                float w = (dist01 - inner) / (1f - inner); 
                float ringFalloff = 1f - Mathf.Abs(w - 0.5f) * 2f; 

                heights[z, x] = Mathf.Clamp01(heights[z, x] + toAdd * ringFalloff * 0.02f);
            }
        }
    }

    void WorldToHM(Vector3 world, out int x, out int z)
    {
        Vector3 local = world - terrain.transform.position;
        float nx = Mathf.Clamp01(local.x / data.size.x);
        float nz = Mathf.Clamp01(local.z / data.size.z);
        x = Mathf.RoundToInt(nx * (res - 1));
        z = Mathf.RoundToInt(nz * (res - 1));
    }

    void ApplySandSlump(int x0, int z0, int width, int height)
    {
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
}

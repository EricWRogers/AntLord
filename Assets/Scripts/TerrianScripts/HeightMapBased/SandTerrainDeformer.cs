using UnityEngine;

[RequireComponent(typeof(Terrain))]
[RequireComponent(typeof(HeightmapTerrain))]
public class SandTerrainDeformer : MonoBehaviour
{
    public HeightmapTerrain hm;

    [Header("Imprinters")]
    public LayerMask imprinterMask;
    public float scanRadius = 40f;
    public Transform focus;

    [Header("Sand Relax")]
    public bool useHmSandSettings = true;
    public bool sandSlump = true;
    public float angleOfReposeDeg = 33f;
    public int slumpIterations = 2;
    [Range(0f, 1f)]
    public float slumpStrength = 0.4f;

    int res;
    float[,] heights;

    void Awake()
    {
        hm = GetComponent<HeightmapTerrain>();
        res = hm.Resolution;
        heights = hm.Heights;

        if (focus == null && Camera.main != null) focus = Camera.main.transform;

        if (useHmSandSettings)
        {
            sandSlump = hm.sandMode;
            angleOfReposeDeg = hm.angleOfReposeDeg;
            slumpIterations = hm.slumpIterations;
            slumpStrength = hm.slumpStrength;
        }
    }

    void FixedUpdate()
    {
        if (focus == null) return;

        Collider[] hits = Physics.OverlapSphere(focus.position, scanRadius, imprinterMask);
        if (hits == null || hits.Length == 0) return;

        for (int i = 0; i < hits.Length; i++)
        {
            var imp = hits[i].GetComponentInParent<SandImprinter>();
            if (imp == null) continue;
            if (!imp.ShouldImprint()) continue;

            Imprint(imp);
        }
    }

    void Imprint(SandImprinter imp)
    {
        Vector3 p = imp.transform.position;

        
        float groundY = hm.terrain.SampleHeight(p) + hm.terrain.transform.position.y;
        float bottomY = imp.BottomY();
        if (bottomY > groundY + 0.03f) return;

        hm.WorldToHM(p, out int cx, out int cz);

        float cellsPerWorldX = (res - 1) / hm.data.size.x;
        float cellsPerWorldZ = (res - 1) / hm.data.size.z;

        int rX = Mathf.Max(1, Mathf.CeilToInt(imp.radius * cellsPerWorldX));
        int rZ = Mathf.Max(1, Mathf.CeilToInt(imp.radius * cellsPerWorldZ));

        int x0 = Mathf.Clamp(cx - rX, 0, res - 1);
        int x1 = Mathf.Clamp(cx + rX, 0, res - 1);
        int z0 = Mathf.Clamp(cz - rZ, 0, res - 1);
        int z1 = Mathf.Clamp(cz + rZ, 0, res - 1);

        
        float targetWorldY = bottomY;
        float terrainBaseY = hm.terrain.transform.position.y;
        float targetN = Mathf.Clamp01((targetWorldY - terrainBaseY) / hm.data.size.y);

        
        float currentCenterN = heights[cz, cx];
        float maxDepthN = imp.maxDepth / hm.data.size.y;
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
                    float t = Mathf.Clamp01(imp.strength * dt * w);
                    float newH = Mathf.Lerp(h, targetN, t);
                    removed += (h - newH);
                    heights[z, x] = newH;
                    changed = true;
                }
            }
        }

        if (!changed) return;

        if (imp.makeBerm && removed > 0f)
            AddBerm(cx, cz, imp, removed);

        if (sandSlump)
            ApplySandSlump(x0, z0, (x1 - x0) + 1, (z1 - z0) + 1);

        // Commit only the region touched
        hm.CommitRegion(x0, z0, x1, z1);
    }

    void AddBerm(int cx, int cz, SandImprinter imp, float removedN)
    {
        float cellsPerWorldX = (res - 1) / hm.data.size.x;
        float cellsPerWorldZ = (res - 1) / hm.data.size.z;

        float bermRadius = imp.radius * imp.bermRadiusMultiplier;
        int rX = Mathf.Max(1, Mathf.CeilToInt(bermRadius * cellsPerWorldX));
        int rZ = Mathf.Max(1, Mathf.CeilToInt(bermRadius * cellsPerWorldZ));

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

    void ApplySandSlump(int x0, int z0, int width, int height)
    {
        float dxWorld = hm.data.size.x / (res - 1);
        float dzWorld = hm.data.size.z / (res - 1);

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

        float hA = heights[zA, xA] * hm.data.size.y;
        float hB = heights[zB, xB] * hm.data.size.y;

        float dh = hA - hB;
        if (dh <= maxDhWorld) return;

        float excess = dh - maxDhWorld;
        float move = excess * slumpStrength * 0.5f;

        hA -= move;
        hB += move;

        heights[zA, xA] = Mathf.Clamp01(hA / hm.data.size.y);
        heights[zB, xB] = Mathf.Clamp01(hB / hm.data.size.y);
    }
}
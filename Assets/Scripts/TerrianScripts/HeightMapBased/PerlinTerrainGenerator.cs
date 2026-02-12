using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class PerlinTerrainGenerator : MonoBehaviour
{
    [Header("Terrain")]
    public Terrain terrain;
    public TerrainData data;

    [Header("Noise Size")]
    public int heightmapResolutionOverride = 513; 

    [Header("Perlin Settings")]
    public int seed = 0;                 
    public float noiseScale = 6f;        
    public float heightAmplitude = 0.25f; 

    [Header("Octaves (optional, but useful)")]
    public int octaves = 4;
    public float lacunarity = 2f;       
    public float persistence = 0.5f;    


    public float[,] heights;  
    public float[,] slopeDeg; 
    int res;

    void Awake()
    {
        terrain = GetComponent<Terrain>();
        data = terrain.terrainData;

   
        if (heightmapResolutionOverride > 0 && data.heightmapResolution != heightmapResolutionOverride)
            data.heightmapResolution = heightmapResolutionOverride;

        res = data.heightmapResolution;

        Generate();
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        if (seed == 0)
            seed = Random.Range(int.MinValue, int.MaxValue);

        
        float offX = (seed * 0.000123f) % 10000f;
        float offZ = (seed * 0.000987f) % 10000f;

        heights = new float[res, res];

        for (int z = 0; z < res; z++)
        {
            for (int x = 0; x < res; x++)
            {
                // Normalize to 0..1 
                float nx = (float)x / (res - 1);
                float nz = (float)z / (res - 1);

                float h = FractalPerlin(nx, nz, offX, offZ);

                
                heights[z, x] = Mathf.Clamp01(h * heightAmplitude);
            }
        }

        data.SetHeightsDelayLOD(0, 0, heights);
        terrain.Flush();

        BuildSlopeMap();
    }

    float FractalPerlin(float nx, float nz, float offX, float offZ)
    {
        
        float frequency = noiseScale;
        float amplitude = 1f;

        float value = 0f;
        float ampSum = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float sx = nx * frequency + offX;
            float sz = nz * frequency + offZ;

            float p = Mathf.PerlinNoise(sx, sz); // 0..1
            value += p * amplitude;
            ampSum += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

    
        return (ampSum > 0f) ? (value / ampSum) : 0f;
    }

    
    void BuildSlopeMap()
    {
        slopeDeg = new float[res, res];

        float dxWorld = data.size.x / (res - 1);
        float dzWorld = data.size.z / (res - 1);

        for (int z = 0; z < res; z++)
        {
            for (int x = 0; x < res; x++)
            {
                int x0 = Mathf.Clamp(x - 1, 0, res - 1);
                int x1 = Mathf.Clamp(x + 1, 0, res - 1);
                int z0 = Mathf.Clamp(z - 1, 0, res - 1);
                int z1 = Mathf.Clamp(z + 1, 0, res - 1);

               
                float hL = heights[z, x0] * data.size.y;
                float hR = heights[z, x1] * data.size.y;
                float hD = heights[z0, x] * data.size.y;
                float hU = heights[z1, x] * data.size.y;

              
                float dHx = (hR - hL) / (2f * dxWorld);
                float dHz = (hU - hD) / (2f * dzWorld);

                float grad = Mathf.Sqrt(dHx * dHx + dHz * dHz);

                // slope angle
                slopeDeg[z, x] = Mathf.Atan(grad) * Mathf.Rad2Deg;
            }
        }
    }

   
    public float GetSlopeDegAtWorld(Vector3 worldPos)
    {
        Vector3 local = worldPos - terrain.transform.position;
        float nx = Mathf.Clamp01(local.x / data.size.x);
        float nz = Mathf.Clamp01(local.z / data.size.z);

        int x = Mathf.RoundToInt(nx * (res - 1));
        int z = Mathf.RoundToInt(nz * (res - 1));

        return slopeDeg[z, x];
    }

    public static float SpeedMultiplierFromSlope(float slopeDeg, float maxSlowDeg = 45f, float minMult = 0.25f)
    {
        float t = Mathf.Clamp01(slopeDeg / maxSlowDeg);
        return Mathf.Lerp(1f, minMult, t);
    }

    //ant movement should be like this to slow down on slopes
    //float slope = perlinGen.GetSlopeDegAtWorld(transform.position);
        //      float speed = baseSpeed * SpeedMultiplierFromSlope(slope);
}

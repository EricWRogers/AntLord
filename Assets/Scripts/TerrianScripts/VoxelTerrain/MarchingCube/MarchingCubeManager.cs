using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(MarchingCubeManager))]
public class CubeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MarchingCubeManager mcm = (MarchingCubeManager)target;
        if (GUILayout.Button("MarchCubes"))
        {
            mcm.ClearWorld();
            mcm.InitializeWorld();
        }
    }
}
#endif


public class MarchingCubeManager : MonoBehaviour
{
    [Header("World Settings:")]
    [SerializeField] private int worldSizeInChunks = 3;
    [SerializeField] private int chunkSize = 16;
    [SerializeField] private float resolution = 1f;
    [SerializeField] private float threshold = 0.5f;

    [Header("Noise Settings:")]
    [SerializeField] private float noiseScale = 0.1f;
    [SerializeField] private float noiseAmplitude = 5f;
    [SerializeField] private bool use3DNoise;
    [SerializeField] private bool chunkGenerated;

    private Dictionary<Vector3Int, MarchingCubeChunk> chunks = new Dictionary<Vector3Int, MarchingCubeChunk>(); //dictionary holding all chunks!
    
    [Header("Inspector Fields:")]
    public MarchingCubeChunk chunkPrefab;
    public Material sandMaterial;
    public RayOMayhem rayOMayhem;
    public NavMeshSurface navMeshSurface;

    void Start() 
    {
        ClearWorld();
        InitializeWorld();
    }
    void Update()
    {
        if (rayOMayhem != null && rayOMayhem.trailEnd)
        {
            navMeshSurface?.BuildNavMesh();
            rayOMayhem.trailEnd = false;
        }
    }

    public void InitializeWorld()
    {
        if (chunks == null) chunks = new Dictionary<Vector3Int, MarchingCubeChunk>();
        
        for (int x = 0; x < worldSizeInChunks; x++)
        {
            for (int z = 0; z < worldSizeInChunks; z++)
            {
                CreateChunk(new Vector3Int(x * chunkSize, 0, z * chunkSize));
            }
        }
    }

    private void CreateChunk(Vector3Int offset)
    {
        MarchingCubeChunk chunk = Instantiate(chunkPrefab, (Vector3)offset * resolution, Quaternion.identity, transform);
        chunk.name = $"Chunk_{offset.x}_{offset.z}";

        int TerrainLayer = LayerMask.NameToLayer("Terrain");
        if (TerrainLayer != -1)
        {
            chunk.gameObject.layer = TerrainLayer;
        }
        else
        {
            Debug.LogWarning("Layer 'Terrain' not found.");
        }
        chunk.Setup(offset, chunkSize, resolution, threshold, sandMaterial);
        
        //noise Generation
        GenerateChunkHeights(chunk, offset);
        chunk.UpdateMesh();
        chunks.Add(offset, chunk);
    }

    public void GenerateEditorWorld()
    {
        //clear existing chunks to avoid stacking
        ClearWorld();
        InitializeWorld();
        Debug.Log("Editor Marching Complete.");
    }

    public void ClearWorld()
    {
        //find all children in the dictionary 
        //clears whenever the script recompiles in the editor.
        var children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);
    
        foreach (var child in children)
        {
            //must use DestroyImmediate in Editor scripts
            DestroyImmediate(child);
        }

        if (chunks != null) chunks.Clear();
    }

    private void GenerateChunkHeights(MarchingCubeChunk chunk, Vector3Int offset)
    {
        for (int x = 0; x <= chunkSize; x++)
        {
            for (int y = 0; y <= chunkSize; y++)
            {
                for (int z = 0; z <= chunkSize; z++)
                {
                    float worldX = offset.x + x;
                    float worldY = offset.y + y;
                    float worldZ = offset.z + z;

                    if (use3DNoise)
                    {
                        chunk.heights[x, y, z] = PerlinNoise3D(worldX * noiseScale, worldY * noiseScale, worldZ * noiseScale);
                    }
                    else
                    {
                        float ground = Mathf.PerlinNoise(worldX * noiseScale, worldZ * noiseScale) * noiseAmplitude;
                        chunk.heights[x, y, z] = Mathf.Clamp01((y - ground) + 0.5f);
                    }
                }
            }
        }
    }

    public void ModifyVoxel(Vector3 worldPosition, float radius, float newValue)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPosition) / resolution;
        
        //determine which chunks are affected by the radius
        int minX = Mathf.FloorToInt((localPos.x - radius) / chunkSize);
        int maxX = Mathf.FloorToInt((localPos.x + radius) / chunkSize);
        int minZ = Mathf.FloorToInt((localPos.z - radius) / chunkSize);
        int maxZ = Mathf.FloorToInt((localPos.z + radius) / chunkSize);

        for (int cx = minX; cx <= maxX; cx++)
        {
            for (int cz = minZ; cz <= maxZ; cz++)
            {
                Vector3Int chunkKey = new Vector3Int(cx * chunkSize, 0, cz * chunkSize);
                if (chunks.TryGetValue(chunkKey, out MarchingCubeChunk chunk))
                {
                    bool changed = ApplyModification(chunk, localPos, radius, newValue, chunkKey);
                    if (changed) chunk.UpdateMesh();
                }
            }
        }
    }

    private bool ApplyModification(MarchingCubeChunk chunk, Vector3 localPos, float radius, float val, Vector3Int offset)
    {
        bool changed = false;
        for (int x = 0; x <= chunkSize; x++)
        {
            for (int y = 0; y <= chunkSize; y++)
            {
                for (int z = 0; z <= chunkSize; z++)
                {
                    Vector3 voxelWorldPos = new Vector3(offset.x + x, offset.y + y, offset.z + z);
                    if (Vector3.Distance(localPos, voxelWorldPos) <= radius)
                    {
                        chunk.heights[x, y, z] = val;
                        changed = true;
                    }
                }
            }
        }
        return changed;
    }

    private float PerlinNoise3D(float x, float y, float z)
    {
        float xy = Mathf.PerlinNoise(x, y);
        float xz = Mathf.PerlinNoise(x, z);
        float yz = Mathf.PerlinNoise(y, z);
        float yx = Mathf.PerlinNoise(y, x);
        float zx = Mathf.PerlinNoise(z, x);
        float zy = Mathf.PerlinNoise(z, y);
        return (xy + xz + yz + yx + zx + zy) / 6f;
    }
}

using UnityEngine;

public class Chunk : MonoBehaviour
{
    private Voxel[,,] voxels;
    [SerializeField] private int chunkSize = 5; // Serialized so you can see it in Inspector

    // Use OnValidate to initialize in the editor without pressing Play
    private void OnValidate()
    {
        if (voxels == null) InitializeVoxels();
    }

    void Start()
    {
        InitializeVoxels();
    }

    private void InitializeVoxels()
    {
        voxels = new Voxel[chunkSize, chunkSize, chunkSize];
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    // Store LOCAL position relative to the chunk
                    voxels[x, y, z] = new Voxel(new Vector3(x, y, z), Color.white);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (voxels != null)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawCube(transform.position + new Vector3(chunkSize / 2, chunkSize / 2, chunkSize / 2), new Vector3(chunkSize, chunkSize, chunkSize));
        }
    }
    public void Initialize(int size)
    {
        this.chunkSize = size;
        voxels = new Voxel[size, size, size];
        InitializeVoxels();
        // Assign a random color for this chunk's gizmos
        gizmoColor = new Color(Random.value, Random.value, Random.value, 0.4f); // Semi-transparent
    }
    private Color gizmoColor; 
}
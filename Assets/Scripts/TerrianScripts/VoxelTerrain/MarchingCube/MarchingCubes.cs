using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(MarchingCubes))]
public class CubeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MarchingCubes mc = (MarchingCubes)target;

        if (GUILayout.Button("MarchCubes"))
        {
            mc.Initialize();
            mc.MarchCubes();
        }
    }
}

#endif

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MarchingCubes : MonoBehaviour
{
    [SerializeField] private int width = 30;
    [SerializeField] private int height = 10;

    [SerializeField] float resolution = 1;
    [SerializeField] float noiseScale = 1;

    [SerializeField] private float heightTresshold = 0.5f;

    [SerializeField] private float noiseAmplitude = 5f;

    [SerializeField] bool visualizeNoise;
    [SerializeField] bool use3DNoise;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private float[,,] heights;

    public RayOMayhem rayOMayhem;

    private MeshFilter meshFilter;
    private Mesh mesh;

    public LayerMask layerMask;

    public void Start()
    {
        //if (GetComponent<MeshCollider>() == null || GameObject.Find("NavMesh Surface") == null)
        Initialize();
    }

    void Update()
    {
        if (rayOMayhem.trailEnd == true)
        {
            UpdateNavMesh();
            rayOMayhem.trailEnd = false;
        }
    }

    void UpdateVisuals()
    {
        MarchCubes();
        SetMesh();
        //GameObject.Find("NavMesh Surface")?.GetComponent<NavMeshSurface>()?.BuildNavMesh();
        Debug.Log("Visuals updated");
    }

    void UpdateNavMesh()
    {
        MarchCubes();
        SetMesh();
        GameObject.Find("NavMesh Surface")?.GetComponent<NavMeshSurface>()?.BuildNavMesh();
        Debug.Log("NavMesh updated");
    }


    public void Initialize()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshFilter.mesh = mesh;

        SetHeights();
        UpdateVisuals();
    }
    private IEnumerator TestAll()
    {
        while (true)
        {
            UpdateVisuals();
            yield return new WaitForSeconds(1f);
        }
    }

    private void SetMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        if (TryGetComponent<MeshCollider>(out MeshCollider meshCollider))
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
    }

    private void SetHeights()
    {
        heights = new float[width + 1, height + 1, width + 1];

        for (int x = 0; x < width + 1; x++)
        {
            for (int y = 0; y < height + 1; y++)
            {
                for (int z = 0; z < width + 1; z++)
                {
                    if (use3DNoise)
                    {
                        float currentHeight = PerlinNoise3D((float)x / width * noiseScale, (float)y / height * noiseScale, (float)z / width * noiseScale);

                        heights[x, y, z] = currentHeight;
                    }
                    else
                    {
                        float groundLevel = Mathf.PerlinNoise(x * noiseScale, z * noiseScale) * noiseAmplitude;
                        float value = y - groundLevel;
                        heights[x, y, z] = Mathf.Clamp01(value + 0.5f);
                    }
                }
            }
        }
    }

    /*public void ResetFlat(int height)
    {
        for (int x = 0; x < width + 1; x++)
        {
            for (int y = 0; y < height + 1; y++)
            {
                for (int z = 0; z < width + 1; z++)          For some reason breaks the code....DON'T Try it
                {
                    heights[x, y, z] = height;
                }
            }
        }
    }*/

    private float PerlinNoise3D(float x, float y, float z)
    {
        float xy = Mathf.PerlinNoise(x, y);
        float xz = Mathf.PerlinNoise(x, z);
        float yz = Mathf.PerlinNoise(y, z);

        float yx = Mathf.PerlinNoise(y, x);
        float zx = Mathf.PerlinNoise(z, x);
        float zy = Mathf.PerlinNoise(z, y);

        return (xy + xz + yz + yx + zx + zy) / 6;
    }

    private int GetConfigIndex(float[] cubeCorners)
    {
        int configIndex = 0;

        for (int i = 0; i < 8; i++)
        {
            if (cubeCorners[i] > heightTresshold)
            {
                configIndex |= 1 << i;
            }
        }

        return configIndex;
    }

    
    public void MarchCubes()
    {
        vertices.Clear();
        triangles.Clear();

        float[] cubeCorners = new float[8];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < width; z++)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3Int corner = new Vector3Int(x, y, z) + MarchingTable.Corners[i];
                        cubeCorners[i] = heights[corner.x, corner.y, corner.z];
                    }

                    MarchCube(new Vector3(x, y, z) * resolution, cubeCorners);
                }
            }
        }
    }

    private void MarchCube(Vector3 position, float[] cubeCorners)
    {
        int configIndex = GetConfigIndex(cubeCorners);

        if (configIndex == 0 || configIndex == 255)
        {
            return;
        }



        int edgeIndex = 0;
        for (int t = 0; t < 5; t++)
        {
            for (int v = 0; v < 3; v++)
            {
                int triTableValue = MarchingTable.Triangles[configIndex, edgeIndex];

                if (triTableValue == -1)
                {
                    return;
                }

                Vector3 edgeStart = position + MarchingTable.Edges[triTableValue, 0];
                Vector3 edgeEnd = position + MarchingTable.Edges[triTableValue, 1];

                Vector3 vertex = (edgeStart + edgeEnd) / 2;

                vertices.Add(vertex);
                triangles.Add(vertices.Count - 1);

                edgeIndex++;
            }
        }
    }

    public void BreakVoxel(Vector3 worldPosition, float radius = 1.5f)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        //calculating the range of voxels to check based on the radius
        int startX = Mathf.FloorToInt((localPos.x - radius) / resolution);
        int endX = Mathf.CeilToInt((localPos.x + radius) / resolution);
        int startY = Mathf.FloorToInt((localPos.y - radius) / resolution);
        int endY = Mathf.CeilToInt((localPos.y + radius) / resolution);
        int startZ = Mathf.FloorToInt((localPos.z - radius) / resolution);
        int endZ = Mathf.CeilToInt((localPos.z + radius) / resolution);

        bool changed = false;
        //loop through the voxels in the affected area and set them to air voxels if they are within the radius
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                for (int z = startZ; z <= endZ; z++)
                {
                    if (x < 0 || x > width || y < 0 || y > height || z < 0 || z > width) continue;

                    float dist = Vector3.Distance(localPos, new Vector3(x, y, z) * resolution);

                    if (dist <= radius)
                    {
                        heights[x, y, z] = 1f; //set voxels to empty or "air"
                        changed = true;
                    }
                }
            }
        }
        //only update the visuals if something is touched
        if (changed)
        {
            UpdateVisuals();
        }
    }

    public void PlaceVoxel(Vector3 worldPosition, float radius = 1.5f)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        //calculating the range of voxels to check based on the radius
        int startX = Mathf.FloorToInt((localPos.x - radius) / resolution);
        int endX = Mathf.CeilToInt((localPos.x + radius) / resolution);
        int startY = Mathf.FloorToInt((localPos.y - radius) / resolution);
        int endY = Mathf.CeilToInt((localPos.y + radius) / resolution);
        int startZ = Mathf.FloorToInt((localPos.z - radius) / resolution);
        int endZ = Mathf.CeilToInt((localPos.z + radius) / resolution);

        bool changed = false;
        //loop through the voxels in the affected area and set them to solid voxels if they are within the radius
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                for (int z = startZ; z <= endZ; z++)
                {
                    if (x < 0 || x > width || y < 0 || y > height || z < 0 || z > width) continue;

                    float dist = Vector3.Distance(localPos, new Vector3(x, y, z) * resolution);

                    if (dist <= radius)
                    {
                        heights[x, y, z] = 0f; //set voxels to solid
                        changed = true;
                    }
                }
            }
        }
        //only update the visuals if something is touched
        if (changed)
        {
            UpdateVisuals();
        }
    }
    public void SetVoxel(Vector3 worldPosition, float radius = 1.5f)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);

        int startX = Mathf.FloorToInt((localPos.x - radius) / resolution);
        int endX = Mathf.CeilToInt((localPos.x + radius) / resolution);
        int startY = Mathf.FloorToInt((localPos.y - radius) / resolution);
        int endY = Mathf.CeilToInt((localPos.y + radius) / resolution);
        int startZ = Mathf.FloorToInt((localPos.z - radius) / resolution);
        int endZ = Mathf.CeilToInt((localPos.z + radius) / resolution);

        bool changed = false;

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                for (int z = startZ; z <= endZ; z++)
                {
                    if (x < 0 || x >= width || y < 0 || y >= height || z < 0 || z >= width)
                        continue;

                    Vector3 voxelPos = new Vector3(x, y, z) * resolution;
                    float dist = Vector3.Distance(localPos, voxelPos);

                    if (dist <= radius)
                    {
                        if (localPos.y == voxelPos.y || voxelPos.y > localPos.y)
                        {
                            heights[x, y, z] = 1.0f;
                        }
                        else
                        {
                            heights[x,y,z] = 0.0f;
                        }
                        changed = true;
                    }
                }
            }
        }

        if (changed)
            UpdateVisuals();
    }

    private void OnDrawGizmosSelected()
    {
        if (!visualizeNoise || !Application.isPlaying)
        {
            return;
        }

        for (int x = 0; x < width + 1; x++)
        {
            for (int y = 0; y < height + 1; y++)
            {
                for (int z = 0; z < width + 1; z++)
                {
                    Gizmos.color = new Color(heights[x, y, z], heights[x, y, z], heights[x, y, z], 1);
                    Gizmos.DrawSphere(new Vector3(x * resolution, y * resolution, z * resolution), 0.2f * resolution);
                }
            }
        }
    }
}
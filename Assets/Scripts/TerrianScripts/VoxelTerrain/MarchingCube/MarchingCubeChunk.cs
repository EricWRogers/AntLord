using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class MarchingCubeChunk : MonoBehaviour
{
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    
    public float[,,] heights;
    private Vector3Int chunkOffset;
    private int size;
    private float resolution;
    private float threshold;

    public void Setup(Vector3Int offset, int size, float res, float threshold)
    {
        this.chunkOffset = offset;
        this.size = size;
        this.resolution = res;
        this.threshold = threshold;
        
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshFilter.mesh = mesh;
        
        heights = new float[size + 1, size + 1, size + 1];
    }

    public void UpdateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    float[] cubeCorners = new float[8];
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3Int corner = new Vector3Int(x, y, z) + MarchingTable.Corners[i];
                        cubeCorners[i] = heights[corner.x, corner.y, corner.z];
                    }
                    
                    MarchCube(new Vector3(x, y, z) * resolution, cubeCorners, vertices, triangles);
                }
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    private void MarchCube(Vector3 position, float[] cubeCorners, List<Vector3> verts, List<int> tris)
    {
        int configIndex = 0;
        for (int i = 0; i < 8; i++)
            if (cubeCorners[i] > threshold) configIndex |= 1 << i;

        if (configIndex == 0 || configIndex == 255) return;

        int edgeIndex = 0;
        for (int t = 0; t < 5; t++)
        {
            for (int v = 0; v < 3; v++)
            {
                int triTableValue = MarchingTable.Triangles[configIndex, edgeIndex];
                if (triTableValue == -1) return;

                Vector3 edgeStart = position + MarchingTable.Edges[triTableValue, 0];
                Vector3 edgeEnd = position + MarchingTable.Edges[triTableValue, 1];
                
                verts.Add((edgeStart + edgeEnd) / 2f);
                tris.Add(verts.Count - 1);
                edgeIndex++;
            }
        }
    }
}
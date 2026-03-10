using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class MarchingCubeChunk : MonoBehaviour
{
    private Voxel[,,] voxels;
    public int chunkSize = 16;

    private void OnValidate()
    {
        if (voxels == null) InitializeVoxels();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

    public void Initialize(int size)
    {
        this.chunkSize = size;
        voxels = new Voxel[size, size, size];
        InitializeVoxels();
        gizmoColor = new Color(0.82f, 0.71f, 0.55f, 1.0f); // Solid tan
    }
    private Color gizmoColor;
}

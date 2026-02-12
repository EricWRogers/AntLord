using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int worldSize = 5; // Size of the world in number of chunks
    private int chunkSize = 5; // Assuming chunk size is 16x16x16

    private Dictionary<Vector3, Chunk> chunks;

    void Start()
    {
        chunks = new Dictionary<Vector3, Chunk>();

        GenerateWorld();
    }

    private void GenerateWorld()
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                for (int z = 0; z < worldSize; z++)
                {
                    Vector3 chunkPosition = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize);
                    GameObject newChunkObject = new GameObject($"Chunk_{x}_{y}_{z}");
                    newChunkObject.transform.position = chunkPosition;
                    newChunkObject.transform.parent = this.transform;

                    Chunk newChunk = newChunkObject.AddComponent<Chunk>();
                    newChunk.Initialize(chunkSize);
                    chunks.Add(chunkPosition, newChunk);
                }
            }
        }
    }

    // Additional methods for managing chunks, like loading and unloading, can be added here
}
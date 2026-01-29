using UnityEngine;

public class GenerateGrid : MonoBehaviour
{
    public GameObject cubeObj;

    private int xWorldSize = 50;
    private int zWorldSize = 50;

    private int noiseHeight = 4;

    private float offset = 1f;


    void Start()
    {
        for (int x = 0; x < xWorldSize; x++)
        {
            for(int z = 0; z < zWorldSize; z++)
            {
                Vector3 position = new Vector3(x * offset, GenerateNoise(x, z, 15f) * noiseHeight, z * offset);

                GameObject cube = Instantiate(cubeObj, position, Quaternion.identity) as GameObject;

                cube.transform.SetParent(this.transform);
            }
        }
    }

    private float GenerateNoise(int x, int z, float scale)
    {
        float xNoise = (x + this.transform.position.x) / scale;
        float zNoise = (z + this.transform.position.y) / scale;

        return Mathf.PerlinNoise(xNoise, zNoise);
    }

}

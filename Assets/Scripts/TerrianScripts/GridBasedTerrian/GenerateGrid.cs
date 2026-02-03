using UnityEngine;

public class GenerateGrid : MonoBehaviour
{
    public GameObject cubeObj;

    public int xWorldSize = 50;
    public int yWorldSize = 4;
    public int zWorldSize = 50;
    public int noiseHeight;

    private float xOffset;
    private float yOffset;

    private float offset = 1f;


    void Start()
    {

        xOffset = Random.Range(0f, 9999f);
        yOffset = Random.Range(0f, 9999f);

        for (int x = 0; x < xWorldSize; x++)
        {
            for(int z = 0; z < zWorldSize; z++)
            {
                for (int y = 0; y < yWorldSize; y++)
                {
                    Vector3 pos = new Vector3(x * offset, (GenerateNoise(x, z, 15f) * noiseHeight) + (y * offset), z * offset);
                    GameObject cube = Instantiate(cubeObj, pos, Quaternion.identity) as GameObject;
                    cube.transform.SetParent(this.transform);
                }
            }
        }
    }

    private float GenerateNoise(int x, int z, float scale)
    {
        float xNoise = (x + this.transform.position.x) / scale + xOffset;
        float zNoise = (z + this.transform.position.y) / scale + yOffset;

        return Mathf.PerlinNoise(xNoise, zNoise);
    }

}

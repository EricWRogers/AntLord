using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    //Grid grid = new Grid(5, 2, 5, 1f);

    public int width = 256;
    public int height = 256;

    private void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.SetTexture("_BaseMap", GenerateTexture());
    }

    Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(width, height);

        // generate perlin noise map for texture
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = CalculateColor(x, y);
                texture.SetPixel(x, y, color); 
            }
        }

        texture.Apply();
        return texture;
    }

    Color CalculateColor(int x, int y)
    {
        float xPCoord = (float)x / width;
        float yPCoord = (float)y / height;

        float sample = Mathf.PerlinNoise(xPCoord, yPCoord);

        return new Color(sample, sample, sample);
    }
}

using UnityEngine;

public class AppleSpawn : MonoBehaviour
{
    //how often to spawn
    private float spawnTimer;
    public float spawnInterval = 4f;

    //what and where to spawn
    public GameObject apple;
    public float sphereRadius = 4f;



    void Update()
    {
        if (Time.time >= spawnTimer)
        {
            SpawnApple();
            spawnTimer = Time.time + spawnInterval;
        }
    }
    public void SpawnApple()
    {
        // int spawnPointX = Random.Range (-3,3);
        // int spawnPointY = Random.Range (4,6);
        // int spawnPointZ = Random.Range (-3,3);

        // Vector3 spawnPosition = new Vector3(spawnPointX, spawnPointY, spawnPointZ);

        Vector3 randomPoint = Random.insideUnitSphere * sphereRadius;
        Vector3 spawnPosition = transform.position + randomPoint;

        Instantiate(apple, spawnPosition, Quaternion.identity);
    }
    


    // void Start()
    // {
        
    // }


}

using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class AppleSpawn : MonoBehaviour
{
    //how often to spawn
    private float spawnTimer;
    // public float spawnInterval = 4f;

    //what and where to spawn
    public GameObject apple;
    public float sphereRadius = 4f;

    //size & growth during/after spawn 
    public float growDuration = 1.5f;
    public float startingSize = 0.1f;
    Coroutine growingApple;
    private bool isGrowing = false;

    //gravity
    private Rigidbody rb;


    void Update()
    {
        if (Time.time >= spawnTimer)
        {
            SpawnApple();
            spawnTimer = Time.time + growDuration + 5; 
        }
    }
    public async Task SpawnApple()
    {
        Vector3 randomPoint = Random.insideUnitSphere * sphereRadius;
        Vector3 spawnPosition = transform.position + randomPoint;

        Quaternion randomYRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        GameObject anApple = Instantiate(apple, spawnPosition, randomYRotation);

        Rigidbody rb = anApple.GetComponent<Rigidbody>();

        anApple.transform.localScale = new Vector3 (startingSize, startingSize, startingSize);

        growingApple = StartCoroutine(WatchAppleGrow(anApple.transform));

        CollisionDetectorForPrefabs forwarder = anApple.GetComponent<CollisionDetectorForPrefabs>();
        forwarder.OnHit = (collision) =>
        {
            Debug.Log("The apple hit: " + collision.gameObject.name);
            
            if (isGrowing == true)
            {
                StopCoroutine(growingApple);    
                Debug.Log ("stopped coroutine grow");
            }
            

            if (rb != null && rb.useGravity != true)
            {
                rb.useGravity = true;
                Debug.Log ("started gravity early");
            }
        };

        await Awaitable.WaitForSecondsAsync(growDuration);

        if (rb != null)
        {
            rb.useGravity = true;
            Debug.Log ("rb");
        }
        else
        {
            Debug.Log ("No rigidbody");
        }
        
    }

    IEnumerator WatchAppleGrow(Transform objTransform)
    {
        isGrowing = true;
        float elapsed = 0f;
        Vector3 startScale = new Vector3 (startingSize, startingSize, startingSize);
        Vector3 endScale = Vector3.one;

        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            objTransform.localScale = Vector3.Lerp(startScale, endScale, elapsed / growDuration);
            yield return null;
        }
        objTransform.localScale = endScale;
        isGrowing = false;
    }
    
    


    // void Start()
    // {
        
    // }


}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SBBerrySpawn : MonoBehaviour
{
    //how often to spawn
    private float spawnTimer;
    public float spawnInterval;
    public float spawnTiming;
    public int howManyToSpawn;

    //what and where to spawn
    public GameObject berry;
    public float sphereRadius = 40f;
    public List<Transform> spawnPoints;

    //size & growth during/after spawn 
    public float growDuration = 1.5f;
    public float startingSize = 0.1f;
    Coroutine growingApple;
    private bool isGrowing = false;

    //gravity
    private Rigidbody rb;
    float timer;

    void Start()
    {
        if(spawnPoints == null)
        {
            spawnPoints.Clear();
            for(int i = 0; i < transform.childCount; i++)
            {
                spawnPoints.Add(transform.GetChild(i));
            }
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnTimer)
        {
            StartCoroutine(SpawnApple());
            float randomTimer = Random.Range(growDuration/10, growDuration*2);
            spawnTimer = randomTimer; 
            timer = 0.0f;
        }
    }
    IEnumerator SpawnApple()
    {
        howManyToSpawn = Random.Range(1, spawnPoints.Count);
        Debug.Log("how many: " + howManyToSpawn);

        // spawnTiming = Random.Range(spawnInterval, spawnInterval * 2);
        // Debug.Log("timing: "+spawnTiming);

        for (int i = 0; i < howManyToSpawn; i++)
        {
            Debug.Log("i: "+i);

            int randomIndex = Random.Range(0, spawnPoints.Count);

            // Transform spawnPosition = spawnPoints[randomIndex];
            // GameObject targetObject = spawnPoints[randomIndex];

            Vector3 spawnPosition = spawnPoints[randomIndex].position;

            if (Physics.CheckSphere(spawnPosition, sphereRadius))
            {
                Debug.Log("something is there");
            }

            else
            {
                GameObject aBerry = Instantiate(berry, spawnPosition, Quaternion.identity);
            }


            

        //     Rigidbody rb = aBerry.GetComponent<Rigidbody>();
        //     // Debug.Log("rb: "+rb);

        //     if (rb != null && rb.useGravity == true)
        //     {
        //         rb.useGravity = false;
        //         // Debug.Log ("Turned off to grow");
        //     }

        //     aBerry.transform.localScale = new Vector3 (startingSize, startingSize, startingSize);

        //     growingApple = StartCoroutine(WatchAppleGrow(aBerry.transform));
        //     // Debug.Log("gAppl: "+growingApple);

        //     CollisionDetectorForPrefabs forwarder = aBerry.GetComponent<CollisionDetectorForPrefabs>();
        //     forwarder.OnHit = (collision) =>
        //     {
        //         Debug.Log("The berry hit: " + collision.gameObject.name);
            
        //         if (isGrowing == true)
        //         {
        //             StopCoroutine(growingApple);    
        //             isGrowing = false;
        //             Debug.Log ("stopped coroutine grow");
        //         }
            

        //         if (rb != null && rb.useGravity != true)
        //         {
        //             rb.useGravity = true;
        //             Debug.Log ("started gravity early");
        //         }
        //     };

        //     //await Awaitable.WaitForSecondsAsync(growDuration);
        //     yield return new WaitForSeconds(growDuration);

        //     if (rb != null)
        //     {
        //         rb.useGravity = true;
        //         Debug.Log ("rb");
        //     }
        //     else
        //     {
        //         Debug.Log ("No rigidbody");
        //     }
     
        }

        yield return null;
        // Vector3 randomPoint = Random.insideUnitSphere * sphereRadius;
        // Vector3 spawnPosition = transform.position + randomPoint;

        // Quaternion randomYRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);       
    }

    // IEnumerator WatchAppleGrow(Transform objTransform)
    // {
    //     Debug.Log("IEnum");
    //     isGrowing = true;
    //     float elapsed = 0f;
    //     Vector3 startScale = new Vector3 (startingSize, startingSize, startingSize);
    //     Vector3 endScale = Vector3.one;

    //     while (elapsed < growDuration)
    //     {
    //         Debug.Log("elapsed < growDuration");
    //         elapsed += Time.deltaTime;
    //         objTransform.localScale = Vector3.Lerp(startScale, endScale, elapsed / growDuration);
    //         yield return null;
    //     }
    //     objTransform.localScale = endScale;
    //     isGrowing = false;
    // }
}

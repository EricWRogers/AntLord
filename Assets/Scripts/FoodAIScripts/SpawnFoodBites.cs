using JetBrains.Annotations;
using UnityEngine;

public class SpawnFoodBites : MonoBehaviour
{
    public GameObject foodBitePrefab;
    private Vector3 biteSpawn;
    private FoodBites foodBite;

    Collider[] hitColliders;
    GameObject[] ants;
    public float radius = 1f;

    public Transform foodbit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        biteSpawn = new Vector3(0f, 0.3f, 0f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var hitColliders in hitColliders)
        {
            if(hitColliders.CompareTag("Player"))
            {
                
                if(hitColliders.transform.childCount < 14){
                    Instantiate(foodBitePrefab, hitColliders.transform.position + biteSpawn, transform.rotation);

                    foodBite = foodBitePrefab.gameObject.GetComponent<FoodBites>();

                    GameObject ant = hitColliders.gameObject;
                    foodBite.SetAnt(ant);
            
                    Debug.Log("bite iis spawned");
                }
            }
        }
    }
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something has entered the trigger");
        if(other.CompareTag("Player"))
        {
            Instantiate(foodBitePrefab, other.transform.position + biteSpawn, transform.rotation);

            foodBite = foodBitePrefab.gameObject.GetComponent<FoodBites>();

            GameObject ant = other.gameObject;
            foodBite.SetAnt(ant);
            
            Debug.Log("bite iis spawned");
        }
    }
}

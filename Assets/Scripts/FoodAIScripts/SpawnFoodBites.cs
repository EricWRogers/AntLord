using JetBrains.Annotations;
using UnityEngine;

public class SpawnFoodBites : MonoBehaviour
{
    public GameObject foodBitePrefab;
    private Vector3 biteSpawn;
    private FoodBites foodBite;

    Collider[] hitColliders;
    public float radius = 1f;
    public Transform foodbit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        biteSpawn = new Vector3(0f, 0.3f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var hitColliders in hitColliders)
        {
            if(hitColliders.CompareTag("Ant"))
            {
                if(hitColliders.GetComponent<FollowNav>() != null && !hitColliders.GetComponent<FollowNav>().amCarryingFood)
                {
                    
                    Instantiate(foodBitePrefab, hitColliders.transform.localPosition + biteSpawn, transform.rotation);

                    hitColliders.GetComponent<FollowNav>().amCarryingFood = true;

                    foodBite = foodBitePrefab.gameObject.GetComponent<FoodBites>();

                    GameObject ant = hitColliders.gameObject;
                    foodBite.SetAnt(ant);
            
                    Debug.Log("bite iis spawned");
                    FindFirstObjectByType<LeadNav>().foodBits++;
                }
                else if(hitColliders.GetComponent<LeadNav>() != null && !hitColliders.GetComponent<LeadNav>().amCarryingFood)
                {
                    Instantiate(foodBitePrefab, hitColliders.transform.localPosition + biteSpawn, transform.rotation);

                    hitColliders.GetComponent<LeadNav>().amCarryingFood = true;

                    foodBite = foodBitePrefab.gameObject.GetComponent<FoodBites>();

                    GameObject ant = hitColliders.gameObject;
                    foodBite.SetAnt(ant);
            
                    Debug.Log("bite iis spawned");
                    FindFirstObjectByType<LeadNav>().foodBits++;
                }

            }
        }
    }
    
}

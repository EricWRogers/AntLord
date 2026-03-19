using UnityEngine;

public class SpawnFoodBites : MonoBehaviour
{
    public GameObject foodBitePrefab;
    private Vector3 biteSpawn;
    //private FoodBites foodBite;
    public int foodHealth = 4;
    Collider[] hitColliders;
    public float radius = 1f;
    public Transform foodbit;
    public LeadNav mostRecentLead;
    public int tier = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        biteSpawn = new Vector3(0f, 0.3f, 0f);
    }

    void Update()
    {
        SpawnFoodBit();
    }
    // Update is called once per frame
    public void SpawnFoodBit()
    {
        hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var hitColliders in hitColliders)
        {
            if(hitColliders.CompareTag("Ant"))
            {   
                tier = hitColliders.GetComponent<LeadNav>().antTier;
                GameObject ant = hitColliders.gameObject;
                if (hitColliders.GetComponent<FollowNav>().enabled && !hitColliders.GetComponent<FollowNav>().amCarryingFood)
                {
                    if (!hitColliders.GetComponent<LeadNav>().enabled)
                    {
                        hitColliders.GetComponent<FollowNav>().amCarryingFood = true;
                        for (int i = 1; i <= tier; i++)
                        {
                            
                            Instantiate(foodBitePrefab, hitColliders.transform.position + (biteSpawn * i), transform.rotation).GetComponent<FoodBites>().SetAnt(ant);
                            mostRecentLead = hitColliders.GetComponent<FollowNav>().leader;
                            
                        }
                        foodHealth -= tier;
                        hitColliders.GetComponent<FollowNav>().leader.foodBits += tier;
                    }
                    


                }
                else if (hitColliders.GetComponent<LeadNav>().enabled && !hitColliders.GetComponent<LeadNav>().amCarryingFood)
                {
                    for (int i = 1; i <= tier; i++)
                    {
                        Instantiate(foodBitePrefab, hitColliders.transform.position + (biteSpawn * i), transform.rotation).GetComponent<FoodBites>().SetAnt(ant);
                        mostRecentLead = hitColliders.GetComponent<LeadNav>();
                        hitColliders.GetComponent<LeadNav>().amCarryingFood = true;
                    }
                    
                    foodHealth -= tier;
                    hitColliders.GetComponent<LeadNav>().foodBits += tier;
                }

            }
        }
        if (foodHealth <= 0 )
        {
            mostRecentLead.target = mostRecentLead.home;
            Destroy(gameObject);
        }
    }

    
    
}

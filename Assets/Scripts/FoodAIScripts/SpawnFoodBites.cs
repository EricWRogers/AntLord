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

    // Update is called once per frame
    void Update()
    {
        hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var hitColliders in hitColliders)
        {
            if(hitColliders.CompareTag("Ant"))
            {
                if (hitColliders.GetComponent<FollowNav>().enabled && !hitColliders.GetComponent<FollowNav>().amCarryingFood)
                {
                    tier = hitColliders.GetComponent<FollowNav>().antTier;
                    if (tier == 1)
                    {
                        hitColliders.GetComponent<FollowNav>().amCarryingFood = true;

                        //foodBite = foodBitePrefab.gameObject.GetComponent<FoodBites>();

                        GameObject ant = hitColliders.gameObject;
                        Instantiate(foodBitePrefab, hitColliders.transform.position + (biteSpawn * tier), transform.rotation).GetComponent<FoodBites>().SetAnt(ant);
                        //foodBite.SetAnt(ant);
                        Debug.Log("bite is spawned " + hitColliders.name);
                        mostRecentLead = hitColliders.GetComponent<FollowNav>().leader;
                        hitColliders.GetComponent<FollowNav>().leader.foodBits++;
                        foodHealth -= 1;
                    }
                    else if (tier == 2)
                    {
                        GameObject ant = hitColliders.gameObject;
                        Instantiate(foodBitePrefab, hitColliders.transform.position + biteSpawn, transform.rotation).GetComponent<FoodBites>().SetAnt(ant);
                        //foodBite.SetAnt(ant);
                        Debug.Log("bite is spawned " + hitColliders.name);
                        mostRecentLead = hitColliders.GetComponent<FollowNav>().leader;
                        hitColliders.GetComponent<FollowNav>().leader.foodBits++;

                        Instantiate(foodBitePrefab, hitColliders.transform.position + (biteSpawn * 2), transform.rotation).GetComponent<FoodBites>().SetAnt(ant);
                        mostRecentLead = hitColliders.GetComponent<FollowNav>().leader;
                        hitColliders.GetComponent<FollowNav>().leader.foodBits++;

                        foodHealth -= 2;
                        hitColliders.GetComponent<FollowNav>().amCarryingFood = true;
                    }


                }
                else if (hitColliders.GetComponent<LeadNav>().enabled && !hitColliders.GetComponent<LeadNav>().amCarryingFood)
                {
                    tier = hitColliders.GetComponent<LeadNav>().antTeir;
                    if (tier == 1)
                    {
                        Instantiate(foodBitePrefab, hitColliders.transform.position + biteSpawn, transform.rotation);

                        hitColliders.GetComponent<LeadNav>().amCarryingFood = true;

                        GameObject ant = hitColliders.gameObject;
                        Instantiate(foodBitePrefab, hitColliders.transform.position + biteSpawn, transform.rotation).GetComponent<FoodBites>().SetAnt(ant);

                        Debug.Log("bite is spawned " + hitColliders.name);
                        mostRecentLead = hitColliders.GetComponent<LeadNav>();
                        hitColliders.GetComponent<LeadNav>().foodBits++;
                        foodHealth -= 1;
                    }
                    else if (tier == 2)
                    {
                        GameObject ant = hitColliders.gameObject;
                        Instantiate(foodBitePrefab, hitColliders.transform.position + biteSpawn, transform.rotation).GetComponent<FoodBites>().SetAnt(ant);
                        mostRecentLead = hitColliders.GetComponent<LeadNav>();
                        hitColliders.GetComponent<LeadNav>().foodBits++;

                        Instantiate(foodBitePrefab, hitColliders.transform.position + (biteSpawn * 2), transform.rotation).GetComponent<FoodBites>().SetAnt(ant);
                        mostRecentLead = hitColliders.GetComponent<LeadNav>();
                        hitColliders.GetComponent<LeadNav>().foodBits++;

                        foodHealth -= 2;
                        hitColliders.GetComponent<LeadNav>().amCarryingFood = true;
                    }

                }

            }
        }
        if (foodHealth == 0 )
        {
            mostRecentLead.DoneWithFood();
            Destroy(gameObject);
        }
    }

    
    
}

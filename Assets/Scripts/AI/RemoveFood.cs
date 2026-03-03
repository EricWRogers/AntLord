using UnityEngine;

[RequireComponent(typeof(SpawnerBuilding))]
public class RemoveFood : MonoBehaviour
{
    Collider[] hitColliders;
    public float radius = 1f;

    // Update is called once per frame
    void Update()
    {
        hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var hitColliders in hitColliders)
        {
            if(hitColliders.CompareTag("Ant"))
            {
                if(hitColliders.GetComponent<FollowNav>().enabled && hitColliders.GetComponent<FollowNav>().amCarryingFood)
                {
                    
                    // Instantiate(foodBitePrefab, hitColliders.transform.localPosition + biteSpawn, transform.rotation);

                    // hitColliders.GetComponent<FollowNav>().amCarryingFood = true;

                    // foodBite = foodBitePrefab.gameObject.GetComponent<FoodBites>();

                    // GameObject ant = hitColliders.gameObject;
                    // foodBite.SetAnt(ant);
            
                    // Debug.Log("bite iis spawned");
                    // FindFirstObjectByType<LeadNav>().foodBits++;

                    hitColliders.GetComponent<FollowNav>().amCarryingFood = false;
                    Destroy(hitColliders.transform.GetComponentInChildren<FoodBites>().gameObject);

                    if(!hitColliders.GetComponent<LeadNav>().enabled)
                    {
                        hitColliders.GetComponent<FollowNav>().leader.foodBits--;
                        hitColliders.GetComponent<FollowNav>().myAgent.isStopped = true;
                        GetComponent<SpawnerBuilding>().GiveFood(1);
                    }
                }
                else if(hitColliders.GetComponent<LeadNav>().enabled && hitColliders.GetComponent<LeadNav>().amCarryingFood)
                {
                    // Instantiate(foodBitePrefab, hitColliders.transform.localPosition + biteSpawn, transform.rotation);

                    // hitColliders.GetComponent<LeadNav>().amCarryingFood = true;

                    // foodBite = foodBitePrefab.gameObject.GetComponent<FoodBites>();

                    // GameObject ant = hitColliders.gameObject;
                    // foodBite.SetAnt(ant);
            
                    // Debug.Log("bite iis spawned");
                    // FindFirstObjectByType<LeadNav>().foodBits++;

                    if(!hitColliders.GetComponent<FollowNav>().enabled)
                    {
                        hitColliders.GetComponent<LeadNav>().amCarryingFood = false;
                        Destroy(hitColliders.transform.GetComponentInChildren<FoodBites>().gameObject);
                        hitColliders.GetComponent<LeadNav>().foodBits--;
                        
                        if(hitColliders.GetComponent<LeadNav>().foodBits == 0)
                        {
                            hitColliders.GetComponent<LeadNav>().myAgent.isStopped = true;
                            hitColliders.GetComponent<LeadNav>().target = null;
                        }
                        
                        GetComponent<SpawnerBuilding>().GiveFood(1);
                    }
                }

            }
        }
    }
    
}

using UnityEngine;

[RequireComponent(typeof(SpawnerBuilding))]
public class RemoveFood : MonoBehaviour
{
    Collider[] hitColliders;
    public float radius = 1f;
    public int tier = 1;

    // Update is called once per frame
    void Update()
    {
        hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var hitColliders in hitColliders)
        {
            if(hitColliders.CompareTag("Ant"))
            {
                tier = hitColliders.GetComponent<LeadNav>().antTier;
                if (hitColliders.GetComponent<FollowNav>().enabled && hitColliders.GetComponent<FollowNav>().amCarryingFood)
                {
                    
                    for (int i = 1; i <= tier; i++)
                    {
                        if (!hitColliders.GetComponent<LeadNav>().enabled)
                        {
                            Destroy(hitColliders.transform.GetComponentInChildren<FoodBites>().gameObject); 
                            //hitColliders.GetComponent<FollowNav>().myAgent.isStopped = true;
                            GetComponent<SpawnerBuilding>().GiveFood(1);
                            hitColliders.GetComponent<FollowNav>().leader.foodBits--;
                        }
                       
                    }
                    hitColliders.GetComponent<FollowNav>().amCarryingFood = false;
                }
                else if (hitColliders.GetComponent<LeadNav>().enabled && hitColliders.GetComponent<LeadNav>().amCarryingFood)
                {
                   
                    for (int i = 1; i <= tier; i++)
                    {

                        // if(hitColliders.GetComponent<LeadNav>().foodBits == 0)
                        // {
                        //     hitColliders.GetComponent<LeadNav>().myAgent.isStopped = true;
                        //     hitColliders.GetComponent<LeadNav>().target = null;
                        // }
                        Destroy(hitColliders.transform.GetComponentInChildren<FoodBites>().gameObject);
                    }
                    hitColliders.GetComponent<LeadNav>().foodBits-=tier;
                    GetComponent<SpawnerBuilding>().GiveFood(tier);
                    hitColliders.GetComponent<LeadNav>().amCarryingFood = false;
                }

            }
        }
    }
    
}

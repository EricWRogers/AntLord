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
                    hitColliders.GetComponent<FollowNav>().amCarryingFood = false;
                    

                    if(!hitColliders.GetComponent<LeadNav>().enabled)
                    {
                        Destroy(hitColliders.transform.GetComponentInChildren<FoodBites>().gameObject);
                        hitColliders.GetComponent<FollowNav>().leader.foodBits--;
                        //hitColliders.GetComponent<FollowNav>().myAgent.isStopped = true;
                        GetComponent<SpawnerBuilding>().GiveFood(1);
                    }
                }
                else if(hitColliders.GetComponent<LeadNav>().enabled && hitColliders.GetComponent<LeadNav>().amCarryingFood)
                {

                    
                    hitColliders.GetComponent<LeadNav>().amCarryingFood = false;
                    
                    hitColliders.GetComponent<LeadNav>().foodBits--;
                        
                        // if(hitColliders.GetComponent<LeadNav>().foodBits == 0)
                        // {
                        //     hitColliders.GetComponent<LeadNav>().myAgent.isStopped = true;
                        //     hitColliders.GetComponent<LeadNav>().target = null;
                        // }
                        
                    GetComponent<SpawnerBuilding>().GiveFood(1);
                    Destroy(hitColliders.transform.GetComponentInChildren<FoodBites>().gameObject); 
                }

            }
        }
    }
    
}

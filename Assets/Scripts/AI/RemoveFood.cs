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
                    foreach (Transform child in hitColliders.transform)
                    {
                        if (child.CompareTag("FoodBit"))
                        {
                            if (!hitColliders.GetComponent<LeadNav>().enabled)
                            {
                                Destroy(child.gameObject);
                                hitColliders.GetComponent<FollowNav>().myAgent.isStopped = true;
                                GetComponent<SpawnerBuilding>().GiveFood(1);
                                hitColliders.GetComponent<FollowNav>().leader.foodBits--;
                            }
                        }
                        else if (child.CompareTag("RockBit"))
                        {
                            Destroy(child.gameObject);
                            hitColliders.GetComponent<FollowNav>().myAgent.isStopped = true;
                            GetComponent<SpawnerBuilding>().GiveRock(1);
                            hitColliders.GetComponent<FollowNav>().leader.foodBits--;
                        }
                    }
                    hitColliders.GetComponent<FollowNav>().amCarryingFood = false;
                }
                else if (hitColliders.GetComponent<LeadNav>().enabled && hitColliders.GetComponent<LeadNav>().amCarryingFood)
                {
                    foreach (Transform child in hitColliders.transform)
                    {
                        if (child.CompareTag("FoodBit"))
                        {
                            Destroy(child.gameObject);
                            GetComponent<SpawnerBuilding>().GiveFood(1);
                            hitColliders.GetComponent<LeadNav>().foodBits -= 1;
                        }
                        else if (child.CompareTag("RockBit"))
                        {
                            Destroy(child.gameObject);
                            GetComponent<SpawnerBuilding>().GiveRock(1);
                            hitColliders.GetComponent<LeadNav>().foodBits -= 1;
                        }
                    }    
                    hitColliders.GetComponent<LeadNav>().amCarryingFood = false;
                }

            }
        }
    }
    
}

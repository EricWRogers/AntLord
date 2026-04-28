using UnityEngine;

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
            if (hitColliders.CompareTag("Ant"))
            {
                
                var lead = hitColliders.GetComponent<LeadNav>();
                var follow = hitColliders.GetComponent<FollowNav>();

                if (lead != null) tier = lead.antTier;

                // FOLLOWER deposit 
                if (follow.isActiveAndEnabled && follow.amCarryingFood)
                {
                    foreach (Transform child in hitColliders.transform)
                    {
                        if (child.CompareTag("FoodBit"))
                        {
                            if (lead == null || !lead.enabled)
                            {
                                Destroy(child.gameObject);

                                // Old: follow.myAgent.isStopped = true;
                                // New: stop movement by clearing mover goal
                                if (follow.mover != null) follow.mover.ClearGoal();

                                GetComponent<SpawnerBuilding>().GiveFood(1);

                                if (follow.leader != null) follow.leader.foodBits--;
                            }
                        }
                        else if (child.CompareTag("RockBit"))
                        {
                            Destroy(child.gameObject);

                            // Old: follow.myAgent.isStopped = true;
                            if (follow.mover != null) follow.mover.ClearGoal();

                            GetComponent<SpawnerBuilding>().GiveRock(1);

                            if (follow.leader != null) follow.leader.foodBits--;
                        }
                        else if (child.CompareTag("StickBit"))
                        {
                            Debug.Log("desroyed stick");
                            Destroy(child.gameObject);

                            // Old: follow.myAgent.isStopped = true;
                            if (follow.mover != null) follow.mover.ClearGoal();

                            GetComponent<SpawnerBuilding>().GiveStick(1);

                            if (follow.leader != null) follow.leader.foodBits--;
                        }
                    }

                    follow.amCarryingFood = false;
                }
                // LEADER deposit 
                else if (lead.isActiveAndEnabled && lead.amCarryingFood)
                {
                    foreach (Transform child in hitColliders.transform)
                    {
                        if (child.CompareTag("FoodBit"))
                        {
                            Destroy(child.gameObject);
                            GetComponent<SpawnerBuilding>().GiveFood(1);
                            lead.foodBits -= 1;
                        }
                        else if (child.CompareTag("RockBit"))
                        {
                            Destroy(child.gameObject);
                            GetComponent<SpawnerBuilding>().GiveRock(1);
                            lead.foodBits -= 1;
                        }
                        else if (child.CompareTag("StickBit"))
                        {
                            Debug.Log("desroyed stick");
                            Destroy(child.gameObject);
                            GetComponent<SpawnerBuilding>().GiveStick(1);
                            lead.foodBits -= 1;
                        }
                    }
                    lead.amCarryingFood = false;
                }
            }
        }
    }
}
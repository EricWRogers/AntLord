using UnityEngine;
using System.Collections.Generic;

public class EnemyBuilding : Buildings
{
    [SerializeField] BuildingSO spawnerSO;
    public Transform spawnPoint;
    public float spawnPadding = 5.0f;
    float timer = 0.0f;
    public float spawnCooldown = 10.0f;
    public float radius = 5.0f;
    public int maxAnts = 10;
    List<GameObject> ants = new List<GameObject>();
    public GameObject antPrefab;
    public int maxHealth = 10;

    void Start()
    {
        this.currentHealth = maxHealth;
        slider.maxValue = currentHealth;
        slider.value = currentHealth;
    }
    void FixedUpdate()
    {
        if (ants.Count < maxAnts && this.currentHealth > 0) //GameManager.instance.GetFood() > minFoodPerAnt
        {
            timer += Time.deltaTime;
            if (timer >= spawnCooldown)
            {
                SpawnAnt();
                timer = 0;
            }
        }
        if (this.currentHealth <= 0)
        {
            FindFirstObjectByType<MM>().Pause();
        }
    }

    void SpawnAnt()
    {
        float randomX = 0.0f;
        float randomZ = 0.0f;
        while (randomX < transform.localScale.x && randomX > -transform.localScale.x)
        {
            randomX = transform.position.x + transform.localScale.x + Random.Range(-(transform.localScale.x + radius), transform.localScale.x + radius);
        }
        while (randomZ < transform.localScale.z && randomZ > -transform.localScale.z)
        {
            randomZ = transform.position.z + transform.localScale.z + Random.Range(-(transform.localScale.z + radius), transform.localScale.z + radius);
        }

        Vector3 spawn = new Vector3(randomX, transform.position.y, randomZ);

        GameObject newAnt = Instantiate(antPrefab, spawn, Quaternion.identity);
    
        AntBrain brain = newAnt.GetComponent<AntBrain>();
        LeadNav ln = newAnt.GetComponent<LeadNav>();
        FollowNav fn = newAnt.GetComponent<FollowNav>();

        LeadNav existingLeader = FindActiveEnemyLeader(brain.antType.teamID);

        if (existingLeader == null)
        {
            ln.enabled = true;
            if (fn != null) fn.enabled = false;
        
            ln.home = this.transform; 
        
            Debug.Log($"<color=orange>Enemy Spawner: First ant is now a Leader.</color>");
        }
        else
        {
            ln.enabled = false;
            if (fn != null)
            {
                fn.enabled = true;
                fn.leader = existingLeader;
            
                if(!existingLeader.followers.Contains(fn.myAgent))
                    existingLeader.followers.Add(fn.myAgent);
            }
        }

        ants.Add(newAnt);
    }

    private LeadNav FindActiveEnemyLeader(int teamID)
    {
        LeadNav[] allLeaders = Object.FindObjectsByType<LeadNav>(FindObjectsSortMode.None);
        foreach (LeadNav leader in allLeaders)
        {
            AntBrain b = leader.GetComponent<AntBrain>();
            // We only care about leaders on our team that are actually enabled
            if (leader.enabled && b != null && b.antType.teamID == teamID)
            {
                return leader;
            }
        }
        return null;
    }
}

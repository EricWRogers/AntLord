using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("AI Integration")]
    [Tooltip("Drag the EnemyCommander GameObject here.")]
    public EnemyCommanderAI commander;

    [Header("Squad Prefabs")]
    public GameObject leaderPrefab;
    public GameObject followerPrefab;

    [Header("Spawn Settings")]
    public int maxActiveAnts = 20;
    public float spawnCooldown = 15.0f;
    [Tooltip("How many followers spawn with each leader?")]
    public int followersPerSquad = 3;

    [Header("Spawn Area")]
    public float minSpawnDistance = 2.0f;
    public float maxSpawnDistance = 6.0f;

    private float timer = 0.0f;
    private List<GameObject> activeAnts = new List<GameObject>();

    void Update()
    {
        activeAnts.RemoveAll(ant => ant == null);

        if (activeAnts.Count + 1 + followersPerSquad <= maxActiveAnts)
        {
            timer += Time.deltaTime;
            if (timer >= spawnCooldown)
            {
                SpawnSquad();
                timer = 0f;
            }
        }
    }

    private void SpawnSquad()
    {
        // 1. SPAWN LEADER
        Vector3 leaderPos = GetRandomSpawnPosition();
        GameObject leaderObj = Instantiate(leaderPrefab, leaderPos, Quaternion.identity);
        activeAnts.Add(leaderObj);

        LeadNav squadLeader = leaderObj.GetComponent<LeadNav>();
        FollowNav leaderFollow = leaderObj.GetComponent<FollowNav>();

        if (squadLeader != null)
        {
            squadLeader.enabled = true;
            if (leaderFollow != null) leaderFollow.enabled = false;
        }

        if (commander != null && squadLeader != null)
        {
            commander.RegisterNewSquad(squadLeader);
        }

        // 2. SPAWN FOLLOWERS
        for (int i = 0; i < followersPerSquad; i++)
        {
            Vector3 followerPos = GetRandomSpawnPosition();
            GameObject followerObj = Instantiate(followerPrefab, followerPos, Quaternion.identity);
            activeAnts.Add(followerObj);

            FollowNav followerNav = followerObj.GetComponent<FollowNav>();
            LeadNav followerLead = followerObj.GetComponent<LeadNav>();

            if (followerNav != null)
            {
                followerNav.enabled = true;
                followerNav.leader = squadLeader;
                followerNav.crumbTrack = 0;
            }

            if (followerLead != null) followerLead.enabled = false;

            if (squadLeader != null && followerNav != null)
            {
                if (squadLeader.followers == null) squadLeader.followers = new List<FollowNav>();
                if (!squadLeader.followers.Contains(followerNav))
                    squadLeader.followers.Add(followerNav);
            }
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float randomDist = Random.Range(minSpawnDistance, maxSpawnDistance);

        return new Vector3(
            transform.position.x + (randomDir.x * randomDist),
            transform.position.y,
            transform.position.z + (randomDir.y * randomDist)
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minSpawnDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxSpawnDistance);
    }
}
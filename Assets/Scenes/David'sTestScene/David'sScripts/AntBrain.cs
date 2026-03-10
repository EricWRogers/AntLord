using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
public enum AntState { Idle, Following, Chasing, Attacking, Returning }

public class AntBrain : MonoBehaviour
{
    [Header("Data & State")]
    public AntDataType antType;
    public AntState currentState = AntState.Following;

    [Header("Detection Settings")]
    public float detectionRange = 5f;
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;

    [Header("Swarm Settings")]
    public bool canSwarm = true; // Toggle for swarm capability
    public SwarmAnt swarmAntPrefab; // Prefab for SwarmAnt (if needed, or use existing)
    public LayerMask groundMask; // Ground layer for swarm snapping
    public Vector2 spawnExtents = new Vector2(15f, 15f); // Spawn area for swarm
    private SwarmAnt swarmAnt; // Reference to the SwarmAnt component
    private SwarmManager currentSwarmManager; // Current swarm manager
    private bool isSwarming = false; // Flag for swarm mode
    private Transform swarmTarget; // Cached swarm target

    private float currentHealth;
    private float attackTimer;
    private AntBrain currentTarget;
    private NavMeshAgent agent;
    private FollowNav followNav;
    private Renderer antRenderer;
    private Color originalColor;
    private Color originalEmissionColor;
    private bool hadEmission;
    public float flashDuration = 0.2f;

    void Start()
    {
        currentHealth = antType.maxHealth;
        agent = GetComponent<NavMeshAgent>();
        followNav = GetComponent<FollowNav>();
        agent.speed = antType.moveSpeed;
        
        antRenderer = GetComponent<Renderer>();
        if (antRenderer == null)
            antRenderer = GetComponentInChildren<Renderer>();
        
        if (antRenderer != null)
        {
            Material mat = antRenderer.material;
            originalColor = mat.color;
            
            if (mat.HasProperty("_EmissionColor"))
            {
                originalEmissionColor = mat.GetColor("_EmissionColor");
                hadEmission = originalEmissionColor.maxColorComponent > 0f;
            }
        }

        // Adds SwarmAnt component if not present
        swarmAnt = GetComponent<SwarmAnt>();
        if (swarmAnt == null)
        {
            swarmAnt = gameObject.AddComponent<SwarmAnt>();
            swarmAnt.enabled = false; // Start disabled
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case AntState.Following:
                HandleFollowing();
                CheckForEnemies();
                break;
            case AntState.Chasing:
                HandleChasing();
                break;
            case AntState.Attacking:
                HandleAttacking();
                break;
        }
    }

    private void HandleFollowing()
    {
        if (followNav != null) followNav.enabled = true;
    }

    private void CheckForEnemies()
    {
        Collider[] potentialTargets = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);
        foreach (var col in potentialTargets)
        {
            AntBrain enemyAnt = col.GetComponent<AntBrain>();
            if (enemyAnt != null && enemyAnt.antType.teamID != antType.teamID)
            {
                // SOLO target: target is another ant
                currentTarget = enemyAnt;
                isSwarming = false;
                currentState = AntState.Chasing;
                if (followNav != null) followNav.enabled = false;
                return;
            }
            else if (canSwarm && (col.CompareTag("SwarmTarget") || col.GetComponent<EnemyBoidController>() != null)) // Adjust tag/component as needed
            {
                // Swarm Target: target is a big creature/base
                swarmTarget = col.transform;
                isSwarming = true;
                currentState = AntState.Chasing; // Or a new "Swarming" state
                if (followNav != null) followNav.enabled = false;
                return;
            }
        }
    }

    private void HandleChasing()
    {
        /*if (currentTarget == null)
        {
            currentState = AntState.Following;
            return;
        }

        agent.SetDestination(currentTarget.transform.position);

        float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (dist <= attackRange)
        {
            currentState = AntState.Attacking;
        }
        else if (dist > detectionRange * 1.5f)
        {
            currentTarget = null;
            currentState = AntState.Following;
        }*/

        if (isSwarming)
        {
            // Join and create swarm
            if (currentSwarmManager == null)
            {
                // Assume SwarmManager has access to antPrefab
                currentSwarmManager = SwarmManager.GetOrCreateSwarm(swarmTarget, swarmAntPrefab, groundMask, spawnExtents);
                currentSwarmManager.AddAnt(swarmAnt);
            }
            
            // Enable swarm behavior
            swarmAnt.enabled = true;
            agent.enabled = false; // Disable NavMesh for swarm
            followNav.enabled = false;
            
            // SwarmAnt handles movement / check if target is still valid
            if (swarmTarget == null || Vector3.Distance(transform.position, swarmTarget.position) > detectionRange * 2f)
            {
                StopSwarming();
            }
        }
        else
        {
            // Solo (David things)
            if (currentTarget == null)
            {
                currentState = AntState.Following;
                return;
            }
            agent.SetDestination(currentTarget.transform.position);
            float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (dist <= attackRange)
            {
                currentState = AntState.Attacking;
            }
            else if (dist > detectionRange * 1.5f)
            {
                currentTarget = null;
                currentState = AntState.Following;
            }
        }
    }

    private void HandleAttacking()
    {
        if (isSwarming)
        {
            // Let boids handle swarm mode
            return;
        }

        if (currentTarget == null)
        {
            currentState = AntState.Chasing;
            return;
        }

        transform.LookAt(currentTarget.transform);
        agent.velocity = Vector3.zero; 

        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f)
        {
            Attack(currentTarget);
            attackTimer = 0;
        }

        if (Vector3.Distance(transform.position, currentTarget.transform.position) > attackRange)
        {
            currentState = AntState.Chasing;
        }
    }

    public void Attack(AntBrain target)
    {
        target.TakeDamage(antType.damage);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        StartCoroutine(FlashRed());
        if (currentHealth <= 0) Die();
    }

    private IEnumerator FlashRed()
    {
        if (antRenderer != null)
        {
            Material mat = antRenderer.material;
            mat.color = Color.red;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", Color.red);
                mat.EnableKeyword("_EMISSION");
            }

            yield return new WaitForSeconds(flashDuration);

            mat.color = originalColor;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", originalEmissionColor);
                if (!hadEmission)
                    mat.DisableKeyword("_EMISSION");
            }
        }
    }
    void Die() 
    {
        LeadNav leadNav = GetComponent<LeadNav>();
        if (leadNav != null)
        {
            Debug.Log("<color=yellow>Leader " + gameObject.name + " died. Looking for successor...</color>");
            PromoteNewLeader(leadNav);
        }
        Destroy(gameObject);
    }

    private void PromoteNewLeader(LeadNav oldLeader)
    {
        FollowNav[] allFollowers = Object.FindObjectsByType<FollowNav>(FindObjectsSortMode.None);
        FollowNav bestCandidate = null;
        float closestDistance = Mathf.Infinity;

        if (allFollowers.Length == 0) Debug.LogWarning("No followers found in the scene to promote!");

        foreach (FollowNav follower in allFollowers)
        {
            AntBrain followerBrain = follower.GetComponent<AntBrain>();
            
            if (followerBrain != null && 
                followerBrain != this &&
                followerBrain.antType.teamID == antType.teamID && 
                followerBrain.currentHealth > 0)
            {
                float dist = Vector3.Distance(transform.position, follower.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    bestCandidate = follower;
                }
            }
        }

        if (bestCandidate != null)
        {
            Debug.Log("<color=green>Promoting " + bestCandidate.gameObject.name + " to new Leader!</color>");
            ExecutePromotion(oldLeader, bestCandidate);
        }
        else
        {
            Debug.LogError("Failed to find a valid teammate to promote.");
        }
    }

    private void ExecutePromotion(LeadNav oldLeader, FollowNav candidate)
    {
        LeadNav newLeader = candidate.gameObject.AddComponent<LeadNav>();
        newLeader.target = oldLeader.target;
        newLeader.home = oldLeader.home;
        newLeader.recentObjective = oldLeader.recentObjective;
        newLeader.crumbs = new List<Vector3>(oldLeader.crumbs);
        newLeader.arrived = oldLeader.arrived;
        newLeader.antTeir = candidate.antTier; 
        newLeader.followers = new List<NavMeshAgent>();
        Destroy(candidate); 
        ReassignFollowers(oldLeader, newLeader);
    }

    private void ReassignFollowers(LeadNav oldLeader, LeadNav newLeader)
    {
        FollowNav[] allFollowers = Object.FindObjectsByType<FollowNav>(FindObjectsSortMode.None);
        int reassignedCount = 0;

        foreach (FollowNav f in allFollowers)
        {
            if (f.leader == oldLeader)
            {
                f.leader = newLeader;
                f.crumbTrack = 0; 
                
                if (f.myAgent != null)
                {
                    newLeader.followers.Add(f.myAgent);
                    reassignedCount++;
                }
            }
        }
        Debug.Log("Squad reassigned: " + reassignedCount + " ants now following " + newLeader.name);
    }

    private void StopSwarming()
    {
        if (currentSwarmManager != null)
        {
            currentSwarmManager.RemoveAnt(swarmAnt);
            currentSwarmManager = null;
        }
        swarmAnt.enabled = false;
        agent.enabled = true;
        isSwarming = false;
        swarmTarget = null;
        currentState = AntState.Following;
    }
}
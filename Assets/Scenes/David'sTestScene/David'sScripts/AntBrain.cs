using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;
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

    [Header("Local UI Damage Feedback")]
    public GameObject damageTextObject;
    public float textDisplayTime = 0.8f;
    public float flashDuration = 0.2f;

    private float currentHealth;
    private float attackTimer;
    private AntBrain currentTarget;
    private NavMeshAgent agent;
    private FollowNav followNav;
    private Renderer antRenderer;
    private Color originalColor;
    private Color originalEmissionColor;
    private bool hadEmission;

    void Start()
    {
        currentHealth = antType.maxHealth;
        agent = GetComponent<NavMeshAgent>();
        followNav = GetComponent<FollowNav>();
        agent.speed = antType.moveSpeed;

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

        if (damageTextObject != null) damageTextObject.SetActive(false);
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

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        StopCoroutine("DisplayDamageNumber");
        StartCoroutine(DisplayDamageNumber(amount));

        StartCoroutine(FlashRed());

        if (currentHealth <= 0) Die();
    }

    private IEnumerator DisplayDamageNumber(float amount)
    {
        if (damageTextObject == null) yield break;

        var txt = damageTextObject.GetComponent<TMP_Text>();
        if (txt != null) txt.text = amount.ToString();
        else
        {
            var standardTxt = damageTextObject.GetComponent<Text>();
            if (standardTxt != null) standardTxt.text = amount.ToString();
        }

        damageTextObject.SetActive(true);
        yield return new WaitForSeconds(textDisplayTime);
        damageTextObject.SetActive(false);
    }

    private IEnumerator FlashRed()
    {
        if (antRenderer == null) yield break;
        Material mat = antRenderer.material;
        mat.color = Color.red;
        if (mat.HasProperty("_EmissionColor")) {
            mat.SetColor("_EmissionColor", Color.red);
            mat.EnableKeyword("_EMISSION");
        }
        yield return new WaitForSeconds(flashDuration);
        mat.color = originalColor;
        if (mat.HasProperty("_EmissionColor")) {
            mat.SetColor("_EmissionColor", originalEmissionColor);
            if (!hadEmission) mat.DisableKeyword("_EMISSION");
        }
    }

    private void HandleFollowing()
    {
        LeadNav lead = GetComponent<LeadNav>();
        if (lead != null)
        {
            if (lead.target != null && agent.destination != lead.target.position)
                agent.SetDestination(lead.target.position);
        }
        else if (followNav != null) followNav.enabled = true;
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
        if (dist <= attackRange) currentState = AntState.Attacking;
        else if (dist > detectionRange * 1.5f) ReturnToTask();
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

        if (currentTarget == null) { ReturnToTask(); return; }
        transform.LookAt(currentTarget.transform);
        agent.velocity = Vector3.zero;

        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f)
        {
            Attack(currentTarget);
            attackTimer = 0;
        }

        if (Vector3.Distance(transform.position, currentTarget.transform.position) > attackRange)
            currentState = AntState.Chasing;
    }

    private void ReturnToTask()
    {
        currentState = AntState.Following;
        currentTarget = null;
        if (followNav != null) followNav.enabled = true;
    }

    public void Attack(AntBrain target) { target.TakeDamage(antType.damage); }


    void Die()
    {
        CommandAnt cmd = Object.FindFirstObjectByType<CommandAnt>();
        if (cmd != null) cmd.selectedAnts.Remove(gameObject);

        LeadNav leadNav = GetComponent<LeadNav>();
        if (leadNav != null) PromoteNewLeader(leadNav);
        
        Destroy(gameObject);
    }

    private void PromoteNewLeader(LeadNav oldLeader)
    {
        FollowNav[] allFollowers = Object.FindObjectsByType<FollowNav>(FindObjectsSortMode.None);
        FollowNav bestCandidate = null;
        float closestDistance = Mathf.Infinity;

        foreach (FollowNav follower in allFollowers)
        {
            AntBrain fBrain = follower.GetComponent<AntBrain>();
            if (fBrain != null && fBrain != this && fBrain.antType.teamID == antType.teamID && fBrain.currentHealth > 0)
            {
                float dist = Vector3.Distance(transform.position, follower.transform.position);
                if (dist < closestDistance) { closestDistance = dist; bestCandidate = follower; }
            }
        }

        if (bestCandidate != null) ExecutePromotion(oldLeader, bestCandidate);
    }

    private void ExecutePromotion(LeadNav oldLeader, FollowNav candidate)
    {
// 1. Get the component and turn it on
    LeadNav newLeader = candidate.gameObject.GetComponent<LeadNav>();
    
    if (newLeader != null)
    {
        newLeader.enabled = true;
        newLeader.target = oldLeader.target;
        newLeader.home = oldLeader.home;
        newLeader.recentObjective = oldLeader.recentObjective;
        newLeader.crumbs = new List<Vector3>(oldLeader.crumbs);
        newLeader.antTeir = candidate.antTier;
        newLeader.task = oldLeader.task;
        newLeader.followers = new List<NavMeshAgent>();

        NavMeshAgent agent = candidate.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.ResetPath();
        }

        Destroy(candidate); 

        ReassignFollowers(oldLeader, newLeader);
        
        Debug.Log($"<color=green>PROMOTION: {candidate.gameObject.name} enabled as new leader.</color>");
    }
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
                    f.myAgent.SetDestination(newLeader.transform.position);
                    newLeader.followers.Add(f.myAgent);
                    reassignedCount++;
                }
            }
        }
        Debug.Log($"<color=cyan>SQUAD SYNC: {reassignedCount} ants successfully moved to new leader {newLeader.name}.</color>");
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
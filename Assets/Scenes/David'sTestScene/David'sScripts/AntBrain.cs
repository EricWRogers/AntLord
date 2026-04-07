using UnityEngine;
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

    [Header("Local UI Damage Feedback")]
    public GameObject damageTextObject;
    public float textDisplayTime = 0.8f;
    public float flashDuration = 0.2f;

    private float currentHealth;
    private float attackTimer;
    private AntBrain currentTarget;
    private FollowNav followNav;
    private LeadNav leadNav;
    private AntMover mover;

    private Renderer antRenderer;
    private Color originalColor;
    private Color originalEmissionColor;
    private bool hadEmission;

    private EnemyBuilding currentBuildingTarget;

    void Start()
    {
        currentHealth = antType.maxHealth;

        followNav = GetComponent<FollowNav>();
        leadNav = GetComponent<LeadNav>();
        mover = GetComponent<AntMover>();

        if (mover != null)
            mover.SetAbsoluteSpeed(antType.moveSpeed);

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

        if (damageTextObject != null) damageTextObject.SetActive(false);
    }

    void Update()
    {
        switch (currentState)
        {
            case AntState.Following:
                HandleFollowing();
                CheckForEnemies();
                CheckForEnemyBuildings();
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
        
        // If LeadNav exists and has a target, the leader moves toward it.
        // Otherwise follower stays in FollowNav mode.

        if (leadNav != null && leadNav.enabled && leadNav.target != null)
        {
            if (mover != null)
                mover.SetGoal(leadNav.target.position);
        }
        else
        {
            if (followNav != null) followNav.enabled = true;
        }
    }

    private void CheckForEnemies()
    {
        Collider[] potentialTargets = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);
        foreach (var col in potentialTargets)
        {
            AntBrain enemyAnt = col.GetComponent<AntBrain>();
            if (enemyAnt != null && enemyAnt.antType.teamID != antType.teamID)
            {
                currentTarget = enemyAnt;
                currentState = AntState.Chasing;
                if (followNav != null) followNav.enabled = false;
                return;
            }
        }
    }

    private void CheckForEnemyBuildings()
    {
        Collider[] potentialTargets = Physics.OverlapSphere(transform.position, detectionRange);
        foreach (var col in potentialTargets)
        {
            if (antType.teamID == 0)
            {
                if (col.GetComponent<EnemyBuilding>())
                {
                    currentBuildingTarget = col.GetComponent<EnemyBuilding>();
                    currentState = AntState.Chasing;
                    if (followNav != null) followNav.enabled = false;
                    return;
                }
            }
            else
            {
                if (col.CompareTag("Building") && col.GetComponent<EnemyBuilding>() == null)
                {
                    currentBuildingTarget = col.GetComponent<EnemyBuilding>();
                    currentState = AntState.Chasing;
                    if (followNav != null) followNav.enabled = false;
                    return;
                }
            }
        }
    }

    private void HandleChasing()
    {
        if (currentTarget == null && currentBuildingTarget == null) { ReturnToTask(); return; }

        float dist = 0.0f;

        if (currentTarget != null)
        {
            if (mover != null) mover.SetGoal(currentTarget.transform.position);
            dist = Vector3.Distance(transform.position, currentTarget.transform.position);
        }
        else if (currentBuildingTarget != null)
        {
            if (mover != null) mover.SetGoal(currentBuildingTarget.transform.position);
            dist = Vector3.Distance(transform.position, currentBuildingTarget.transform.position);
        }

        if (dist <= attackRange) currentState = AntState.Attacking;
        else if (dist > detectionRange * 1.5f) ReturnToTask();
    }

    private void HandleAttacking()
    {
        if (currentTarget == null && currentBuildingTarget == null) { ReturnToTask(); return; }

        if (currentTarget != null)
            transform.LookAt(currentTarget.transform);
        else if (currentBuildingTarget != null)
            transform.LookAt(currentBuildingTarget.transform);

        // Old: agent.velocity = Vector3.zero;
        // New: stop moving by clearing mover goal
        if (mover != null) mover.ClearGoal();

        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f)
        {
            if (currentTarget != null)
                Attack(currentTarget);
            else if (currentBuildingTarget != null)
                Attack(currentBuildingTarget);

            attackTimer = 0;
        }

        
        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.transform.position) > attackRange)
            currentState = AntState.Chasing;

        if (currentBuildingTarget != null && Vector3.Distance(transform.position, currentBuildingTarget.transform.position) > attackRange)
            currentState = AntState.Chasing;
    }

    private void ReturnToTask()
    {
        currentState = AntState.Following;
        currentTarget = null;
        currentBuildingTarget = null;
        if (followNav != null) followNav.enabled = true;
    }

    public void Attack(AntBrain target) { target.TakeDamage(antType.damage); }
    public void Attack(EnemyBuilding target) { target.TakeDamage((int)antType.damage); }

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
        LeadNav newLeader = candidate.gameObject.GetComponent<LeadNav>();

        if (newLeader != null)
        {
            newLeader.enabled = true;
            newLeader.target = oldLeader.target;
            newLeader.home = oldLeader.home;
            newLeader.recentObjective = oldLeader.recentObjective;
            newLeader.crumbs = new List<Vector3>(oldLeader.crumbs);
            newLeader.antTier = candidate.antTier;
            newLeader.task = oldLeader.task;

            // ollowers list is FollowNav now, not NavMeshAgent
            newLeader.followers = new List<FollowNav>();

            EnemyCommanderAI commander = Object.FindFirstObjectByType<EnemyCommanderAI>();
            if (commander != null)
            {
                commander.RegisterNewSquad(newLeader);
            }

            ReassignFollowers(oldLeader, newLeader);

            Debug.Log($"<color=green>PROMOTION: {candidate.gameObject.name} enabled as new leader.</color>");
        }
    }

    private void ReassignFollowers(LeadNav oldLeader, LeadNav newLeader)
    {
        oldLeader.crumbs.Clear();
        newLeader.crumbs.Clear();

        FollowNav[] allFollowers = Object.FindObjectsByType<FollowNav>(FindObjectsSortMode.None);
        int reassignedCount = 0;

        foreach (FollowNav f in allFollowers)
        {
            if (f.leader == oldLeader)
            {
                f.leader = newLeader;
                f.crumbTrack = 0;

                // Old: f.myAgent.SetDestination(newLeader.transform.position); newLeader.followers.Add(f.myAgent);
                // New: just add the follower component, and let FollowNav handle crumb movement.
                if (!newLeader.followers.Contains(f))
                {
                    newLeader.followers.Add(f);
                    reassignedCount++;
                }
            }
        }

        Debug.Log($"<color=cyan>SQUAD SYNC: {reassignedCount} ants successfully moved to new leader {newLeader.name}.</color>");
    }
}
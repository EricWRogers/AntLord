using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public enum EnemyState { Patrolling, Chasing, Attacking }

public class EnemyAntBrain : MonoBehaviour
{
    [Header("Data & State")]
    public AntDataType antType; 
    public EnemyState currentState = EnemyState.Patrolling;

    [Header("Patrol Settings")]
    public List<Transform> patrolWaypoints;
    public float waypointWaitTime = 2f;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;

    [Header("Detection Settings")]
    public float detectionRange = 7f; 
    public float attackRange = 1.5f;
    public LayerMask playerLayer; 

    private float currentHealth;
    private float attackTimer;
    private AntBrain currentTarget;
    private NavMeshAgent agent;

    void Start()
    {
        currentHealth = antType.maxHealth;
        agent = GetComponent<NavMeshAgent>();
        agent.speed = antType.moveSpeed;
        
        if (patrolWaypoints.Count > 0)
        {
            agent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                HandlePatrolling();
                CheckForPlayerAnts();
                break;
            case EnemyState.Chasing:
                HandleChasing();
                break;
            case EnemyState.Attacking:
                HandleAttacking();
                break;
        }
    }

    private void HandlePatrolling()
    {
        if (patrolWaypoints.Count == 0 || isWaiting) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    private IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waypointWaitTime);
        
        currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Count;
        agent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
        
        isWaiting = false;
    }

    private void CheckForPlayerAnts()
    {
        Collider[] potentialTargets = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);
        foreach (var col in potentialTargets)
        {
            AntBrain playerAnt = col.GetComponent<AntBrain>();
            if (playerAnt != null) 
            {
                currentTarget = playerAnt;
                currentState = EnemyState.Chasing;
                isWaiting = false;
                StopAllCoroutines();
                return;
            }
        }
    }

    private void HandleChasing()
    {
        if (currentTarget == null) { ReturnToPatrol(); return; }
        
        agent.SetDestination(currentTarget.transform.position);

        float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (dist <= attackRange) 
            currentState = EnemyState.Attacking;
        else if (dist > detectionRange * 1.5f) 
            ReturnToPatrol();
    }

    private void HandleAttacking()
    {
        if (currentTarget == null) { ReturnToPatrol(); return; }
        
        transform.LookAt(currentTarget.transform);
        agent.velocity = Vector3.zero;

        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f)
        {
            currentTarget.TakeDamage(antType.damage);
            attackTimer = 0;
        }

        if (Vector3.Distance(transform.position, currentTarget.transform.position) > attackRange)
            currentState = EnemyState.Chasing;
    }

    private void ReturnToPatrol()
    {
        currentState = EnemyState.Patrolling;
        currentTarget = null;
        if (patrolWaypoints.Count > 0)
            agent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Destroy(gameObject);
    }

    


}
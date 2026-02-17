using UnityEngine;
using UnityEngine.AI;

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

    private float currentHealth;
    private float attackTimer;
    private AntBrain currentTarget;
    private NavMeshAgent agent;
    private FollowNav followNav;

    void Start()
    {
        currentHealth = antType.maxHealth;
        agent = GetComponent<NavMeshAgent>();
        followNav = GetComponent<FollowNav>();
        agent.speed = antType.moveSpeed;
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

    // --- State Logic Methods ---

    private void HandleFollowing()
    {
        // FollowNav handles the movement logic automatically in its own Update
        // We just ensure the script is enabled
        followNav.enabled = true;
    }

    private void CheckForEnemies()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);
        if (enemies.Length > 0)
        {
            currentTarget = enemies[0].GetComponent<AntBrain>();
            if (currentTarget != null)
            {
                currentState = AntState.Chasing;
                followNav.enabled = false; // Stop following leader crumbs
            }
        }
    }

    private void HandleChasing()
    {
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
        else if (dist > detectionRange * 1.5f) // Lose interest
        {
            currentTarget = null;
            currentState = AntState.Following;
        }
    }

    private void HandleAttacking()
    {
        if (currentTarget == null)
        {
            currentState = AntState.Chasing;
            return;
        }

        // Keep rotated towards enemy
        transform.LookAt(currentTarget.transform);
        agent.velocity = Vector3.zero; // Stop moving while biting

        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f) // 1 attack per second
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
        Debug.Log(antType.typeName + " deals " + antType.damage + " damage!");
        target.TakeDamage(antType.damage);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    void Die() => Destroy(gameObject);
}
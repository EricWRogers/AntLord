using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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
        
        // Try to get renderer from this object or child objects
        antRenderer = GetComponent<Renderer>();
        if (antRenderer == null)
            antRenderer = GetComponentInChildren<Renderer>();
        
        if (antRenderer != null)
        {
            // operate on material instance so we don't tint shared asset
            Material mat = antRenderer.material;
            originalColor = mat.color;
            
            // check for emission property
            if (mat.HasProperty("_EmissionColor"))
            {
                originalEmissionColor = mat.GetColor("_EmissionColor");
                hadEmission = originalEmissionColor.maxColorComponent > 0f;
            }
            Debug.Log("Renderer found: " + antRenderer.name + " (emission? " + hadEmission + ")");
        }
        else
            Debug.LogWarning("No Renderer found on ant!");
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
        Collider[] potentialTargets = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);
        foreach (var col in potentialTargets)
        {
            AntBrain enemyAnt = col.GetComponent<AntBrain>();
            if (enemyAnt != null && enemyAnt.antType.teamID != antType.teamID)
            {
                currentTarget = enemyAnt;
                currentState = AntState.Chasing;
                followNav.enabled = false; // Stop following leader crumbs
                return;
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
        StartCoroutine(FlashRed());
        if (currentHealth <= 0) Die();
        Debug.Log(antType.typeName + " takes " + amount + " damage! Remaining health: " + currentHealth);
    }

    private IEnumerator FlashRed()
    {
        if (antRenderer != null)
        {
            Material mat = antRenderer.material;
            // set base color red so we see immediate tint
            mat.color = Color.red;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", Color.red);
                mat.EnableKeyword("_EMISSION");
            }
            Debug.Log("Flashing " + gameObject.name + " red with emission");

            yield return new WaitForSeconds(flashDuration);

            mat.color = originalColor;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", originalEmissionColor);
                if (!hadEmission)
                    mat.DisableKeyword("_EMISSION");
            }
            Debug.Log("Color/emission restored");
        }
        else
        {
            Debug.LogWarning("Cannot flash - no renderer found!");
            yield return null;
        }
    }



    void Die() => Destroy(gameObject);
}
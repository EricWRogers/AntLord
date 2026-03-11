using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AntSlopeController : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public LayerMask groundMask;

    [Header("Slope Behavior")]
    public float maxSlowSlopeDeg = 45f;        
    public float minSpeedMultiplier = 0.35f;   // speed at maxSlowSlopeDeg
    public float rayHeight = 0.5f;
    public float rayDistance = 3f;

    [Header("Optional: heavy-carry rule")]
    public bool isCarryingHeavy = false;
    public float heavyMaxSlopeDeg = 25f;       

    float baseSpeed;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        baseSpeed = agent.speed;
    }

    
    public void SyncBaseSpeed(float newBaseSpeed)
    {
        baseSpeed = newBaseSpeed;
    }

    void Update()
    {
        if (agent == null) return;

        Vector3 origin = transform.position + Vector3.up * rayHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance, groundMask))
        {
            float slopeDeg = Vector3.Angle(hit.normal, Vector3.up);

            if (isCarryingHeavy && slopeDeg > heavyMaxSlopeDeg)
            {
                
                agent.isStopped = true;
                return;
            }

            agent.isStopped = false;

            float t = Mathf.Clamp01(slopeDeg / maxSlowSlopeDeg);
            float mul = Mathf.Lerp(1f, minSpeedMultiplier, t);
            agent.speed = baseSpeed * mul;
        }
        else
        {
           
            agent.speed = baseSpeed;
        }
    }
}
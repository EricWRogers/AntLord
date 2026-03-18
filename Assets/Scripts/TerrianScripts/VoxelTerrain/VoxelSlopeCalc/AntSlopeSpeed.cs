using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AntSlopeSpeed : MonoBehaviour
{
    public LayerMask groundMask;

    [Header("Speed")]
    public float baseSpeed = 3.5f;
    public float minSpeed = 1.0f;

    [Header("Slope Slowdown")]
    public float slowStartDeg = 15f;     
    public float slowMaxDeg = 45f;       

    [Header("Sampling")]
    public float rayStartHeight = 1.0f;
    public float rayLength = 3.0f;
    public float sampleInterval = 0.12f;

    NavMeshAgent agent;
    float timer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        baseSpeed = agent.speed;
    }

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        timer += Time.deltaTime;
        if (timer < sampleInterval) return;
        timer = 0f;

        
        Vector3 start = transform.position + Vector3.up * rayStartHeight;
        if (!Physics.Raycast(start, Vector3.down, out RaycastHit hit, rayLength, groundMask, QueryTriggerInteraction.Ignore))
        {
            agent.speed = baseSpeed;
            return;
        }

        float slope = Vector3.Angle(hit.normal, Vector3.up);

        // Map slope to multiplier
        float t = Mathf.InverseLerp(slowStartDeg, slowMaxDeg, slope);
        float mul = Mathf.Lerp(1f, minSpeed / baseSpeed, Mathf.Clamp01(t));

        agent.speed = Mathf.Max(minSpeed, baseSpeed * mul);
    }
}
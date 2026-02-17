using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SwarmAnt : MonoBehaviour
{
    [HideInInspector] public SwarmManager manager;

    //temporary hardcoding
    [Header("Motion")]
    public float maxSpeed = 2.6f;
    public float maxSteer = 10f;
    public float rotationSpeed = 16f;
    public float groundStickHeight = 0.03f;

    [Header("Neighbors")]
    public float neighborRadius = 1.3f;
    public float separationRadius = 0.35f;

    [Header("Swarm Target")]
    public float swarmRadius = 2.0f;        // ring radius around target
    public float swarmRadiusSoftness = 1.0f; // how strongly it tries to stay on the ring
    public float orbitStrength = 1.2f;      // circling behaviorhigher equals more circling
    public float arriveStrength = 1.0f;     // move toward target
    public float centerAvoidStrength = 1.1f; // avoid standing on exact center

    [Header("Weights")]
    public float separationWeight = 1.3f;  //pushes ants away from nearby ants and if set too high will probably cause ants to spread too much
    public float alignmentWeight = 0.9f; //matches velocity of ants with their neghbors
    public float cohesionWeight = 0.8f;  //pulls the ants towards a local group center

    private Vector3 velocity;
    private Vector3 groundNormal = Vector3.up;

    void Start()
    {
        Vector3 dir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        velocity = dir * (maxSpeed * 0.7f);
    }

    void Update()
    {
        if (manager == null) return;

        // locks to ground
        Vector3 fromAbove = transform.position + Vector3.up * 2f;
        if (manager.TryGetGroundInfo(fromAbove, out Vector3 groundPoint, out Vector3 normal))
        {
            groundNormal = normal;
            transform.position = groundPoint + groundNormal * groundStickHeight;
        }
        else
        {
            groundNormal = Vector3.up;
        }

        Vector3 pos = transform.position;

        // local flocking
        Vector3 sep = Vector3.zero;
        Vector3 align = Vector3.zero;
        Vector3 cohSum = Vector3.zero;
        int count = 0;

        var ants = manager.ants;
        for (int i = 0; i < ants.Count; i++)
        {
            SwarmAnt other = ants[i];
            if (other == null || other == this) continue;

            Vector3 d = ProjectOnGround(other.transform.position - pos);
            float dist = d.magnitude;
            if (dist <= 0.0001f || dist > neighborRadius) continue;

            count++;

            if (dist < separationRadius)
                sep -= d / (dist * dist); // strong push if very close

            align += ProjectOnGround(other.velocity);
            cohSum += other.transform.position;
        }

        Vector3 coh = Vector3.zero;
        if (count > 0)
        {
            sep = sep.normalized;
            align = (align / count).normalized;

            Vector3 center = cohSum / count;
            coh = ProjectOnGround(center - pos).normalized;
        }

        // swarming target
        Vector3 target = manager.GetTargetPosition();
        Vector3 toTarget = ProjectOnGround(target - pos);
        float distToTarget = toTarget.magnitude;

        Vector3 dirToTarget = (distToTarget > 0.0001f) ? (toTarget / distToTarget) : Vector3.zero;

        // once arrive, pull toward target
        Vector3 seek = dirToTarget;

        // Orbit Math: tangent around the target on the ground plane
        // tangent = cross(up, radial) gives perpendicular direction
        Vector3 tangent = Vector3.Cross(groundNormal, dirToTarget).normalized;

        // Ring behavior:
        // If too far outside swarmRadius, pull inward
        // If too far inside , push outward
        float ringError = distToTarget - swarmRadius; // positive: outside, negative: inside
        Vector3 ringForce = dirToTarget * Mathf.Clamp(ringError, -1f, 1f) * swarmRadiusSoftness;

        // Avoid the center: if very close to target, push away
        Vector3 centerAvoid = Vector3.zero;
        if (distToTarget < swarmRadius * 0.5f && distToTarget > 0.0001f)
            centerAvoid = -dirToTarget * (1f - (distToTarget / (swarmRadius * 0.5f)));

        // Combined swarming direction
        Vector3 swarmDir =
            (seek * arriveStrength) +
            (tangent * orbitStrength) +
            (ringForce) +
            (centerAvoid * centerAvoidStrength);

        swarmDir = (swarmDir == Vector3.zero) ? Vector3.zero : swarmDir.normalized;

        Vector3 accel = Vector3.zero;

        if (count > 0)
        {
            accel += SteerTowards(sep * maxSpeed) * separationWeight;
            accel += SteerTowards(align * maxSpeed) * alignmentWeight;
            accel += SteerTowards(coh * maxSpeed) * cohesionWeight;
        }

        if (swarmDir != Vector3.zero)
            accel += SteerTowards(swarmDir * maxSpeed);

        // Apply acceleration
        velocity += accel * Time.deltaTime;
        velocity = ProjectOnGround(velocity);

        // Clamp speed
        float spd = velocity.magnitude;
        if (spd > maxSpeed) velocity = (velocity / spd) * maxSpeed;

        // Move
        transform.position += velocity * Time.deltaTime;

        // Rotate (aligned to ground normal)
        if (velocity.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(velocity.normalized, groundNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    private Vector3 SteerTowards(Vector3 desiredVel)
    {
        desiredVel = ProjectOnGround(desiredVel);
        Vector3 steer = desiredVel - velocity;

        float m = steer.magnitude;
        if (m > maxSteer) steer = (steer / m) * maxSteer;

        return ProjectOnGround(steer);
    }

    private Vector3 ProjectOnGround(Vector3 v) => Vector3.ProjectOnPlane(v, groundNormal);

    public Vector3 Velocity => velocity;
}

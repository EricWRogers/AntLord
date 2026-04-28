using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AntMover : MonoBehaviour
{
    public float baseSpeed = 3.5f;
    public float minSpeed = 1.0f;
    public float turnSpeedDeg = 540f;
    public float arriveDist = 0.25f;

    
    [Header("Idle Scoot")]
    public float idleScootSpeed = 0.8f;     
    public float idleSteerDeadzone = 0.05f; 

    CharacterController cc;
    Vector3 goal;
    bool hasGoal;

    float speedMul = 1f;
    float currentSpeed;

    
    Vector3 steering;

    public bool HasGoal => hasGoal;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        currentSpeed = baseSpeed;
    }

    public void SetGoal(Vector3 g) { goal = g; hasGoal = true; }
    public void ClearGoal() => hasGoal = false;

    public void SetAbsoluteSpeed(float s)
    {
        baseSpeed = Mathf.Max(minSpeed, s);
        currentSpeed = Mathf.Max(minSpeed, baseSpeed * speedMul);
    }

    public void SetSpeedMultiplier(float mul)
    {
        speedMul = Mathf.Clamp01(mul);
        currentSpeed = Mathf.Max(minSpeed, baseSpeed * speedMul);
    }

    public void ResetSpeedMultiplier()
    {
        speedMul = 1f;
        currentSpeed = Mathf.Max(minSpeed, baseSpeed);
    }

    public void SetSteering(Vector3 steerXZ)
    {
        steerXZ.y = 0f;
        steering = Vector3.ClampMagnitude(steerXZ, 1f);
    }

    void Update()
    {
        // if no goal, still allow scoot movement
        if (!hasGoal)
    {
        Vector3 steer = steering;
        steer.y = 0f;

        if (steer.magnitude > idleSteerDeadzone)
        {
            Vector3 dir = steer.normalized;

            
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            if (Physics.Raycast(origin, dir, out RaycastHit fHit, 0.25f))
            {
                bool isWall = Vector3.Dot(fHit.normal, Vector3.up) < 0.35f;
                if (isWall)
                {
                    Vector3 left = Quaternion.Euler(0, -45f, 0) * dir;
                    Vector3 right = Quaternion.Euler(0, 45f, 0) * dir;

                    if (!Physics.Raycast(origin, left, 0.25f)) dir = left;
                    else if (!Physics.Raycast(origin, right, 0.25f)) dir = right;
                }
            }

            
            cc.SimpleMove(dir * idleScootSpeed);
        }

        return;
    }

        
        Vector3 to = goal - transform.position;
        to.y = 0f;

        float dist = to.magnitude;
        if (dist <= arriveDist)
        {
            hasGoal = false;
            return;
        }

        Vector3 dirGoal = to / dist;

        // apply steering before obstacle checks so ants naturally slide around each other
        if (steering.sqrMagnitude > 0.0001f)
            dirGoal = (dirGoal + steering).normalized;

        Vector3 originGoal = transform.position + Vector3.up * 0.1f;
        if (Physics.Raycast(originGoal, dirGoal, out RaycastHit fHitGoal, 0.25f))
        {
            // If the surface normal is mostly vertical, then nudge.
            //If mostly upward, don't nudge
            bool isWall = Vector3.Dot(fHitGoal.normal, Vector3.up) < 0.35f; // tweak 0.2-0.5
            if (isWall)
            {
                Vector3 left = Quaternion.Euler(0, -45f, 0) * dirGoal;
                Vector3 right = Quaternion.Euler(0, 45f, 0) * dirGoal;

                if (!Physics.Raycast(originGoal, left, 0.25f)) dirGoal = left;
                else if (!Physics.Raycast(originGoal, right, 0.25f)) dirGoal = right;
            }
        }

        // manual rotation
        Quaternion targetRotGoal = Quaternion.LookRotation(dirGoal, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotGoal, turnSpeedDeg * Time.deltaTime);

        // keeps movement stable on uneven surfaces
        cc.SimpleMove(dirGoal * currentSpeed);
    }
}
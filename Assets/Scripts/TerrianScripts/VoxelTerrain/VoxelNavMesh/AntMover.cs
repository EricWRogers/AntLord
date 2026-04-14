using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AntMover : MonoBehaviour
{
    public float baseSpeed = 3.5f;
    public float minSpeed = 1.0f;
    public float turnSpeedDeg = 540f;
    public float arriveDist = 0.25f;

    CharacterController cc;
    Vector3 goal;
    bool hasGoal;

    
    float speedMul = 1f;
    float currentSpeed;

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

    void Update()
    {
        if (!hasGoal) return;

        Vector3 to = goal - transform.position;
        to.y = 0f;

        float dist = to.magnitude;
        if (dist <= arriveDist)
        {
            hasGoal = false;
            return;
        }

        Vector3 dir = to / dist;

        Vector3 origin = transform.position + Vector3.up * 0.1f;
        if (Physics.Raycast(origin, dir, out RaycastHit fHit, 0.25f))
        {
            // If the surface normal is mostly vertical, then nudge.
            //If mostly upward, don't nudge
            bool isWall = Vector3.Dot(fHit.normal, Vector3.up) < 0.35f; // tweak 0.2-0.5
            if (isWall)
            {
                Vector3 left = Quaternion.Euler(0, -45f, 0) * dir;
                Vector3 right = Quaternion.Euler(0, 45f, 0) * dir;

                if (!Physics.Raycast(origin, left, 0.25f)) dir = left;
                else if (!Physics.Raycast(origin, right, 0.25f)) dir = right;
            }
        }

        // manual rotation
        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDeg * Time.deltaTime);

        // keeps movement stable on uneven surfaces
        cc.SimpleMove(dir * currentSpeed);
    }
}
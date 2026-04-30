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

    Vector3 steering;

    // movement reporting
    public bool IsMoving { get; private set; }
    public AntBrain brain; 
    Vector3 lastPos;
    public float moveEpsilon = 0.003f; 

    public bool HasGoal => hasGoal;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        currentSpeed = baseSpeed;

        if (!brain) brain = GetComponent<AntBrain>();
        lastPos = transform.position;
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
        
        if (hasGoal)
        {
            Vector3 to = goal - transform.position;
            to.y = 0f;

            float dist = to.magnitude;
            if (dist <= arriveDist)
            {
                hasGoal = false;
            }
            else
            {
                Vector3 dir = to / dist;

                if (steering.sqrMagnitude > 0.0001f)
                    dir = (dir + steering).normalized;

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

                Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDeg * Time.deltaTime);

                cc.SimpleMove(dir * currentSpeed);
            }
        }

        // movement reporting
        UpdateMovingState();
        //MarchMusicController.Report(this);
    }

    void UpdateMovingState()
    {
        Vector3 p = transform.position;
        float dx = p.x - lastPos.x;
        float dz = p.z - lastPos.z;

        IsMoving = (dx * dx + dz * dz) > (moveEpsilon * moveEpsilon);
        lastPos = p;
    }
}
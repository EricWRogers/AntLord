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

    public bool HasGoal => hasGoal;

    void Awake() => cc = GetComponent<CharacterController>();

    public void SetGoal(Vector3 g) { goal = g; hasGoal = true; }
    public void ClearGoal() => hasGoal = false;

    public void SetAbsoluteSpeed(float s) => baseSpeed = Mathf.Max(minSpeed, s);

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

        // manual rotation 
        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDeg * Time.deltaTime);

        // keeps movement stable on uneven surfaces
        cc.SimpleMove(dir * baseSpeed);
    }
}
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AntGroundAlign : MonoBehaviour
{
    public LayerMask groundMask;
    public float rayStartHeight = 1.5f;
    public float rayLength = 5f;

    [Header("Snap")]
    public float hoverOffset = 0.03f;   
    public float maxSnapDown = 1.0f;    
    public float maxSnapUp = 1.0f;
    public float snapSpeed = 20f;

    [Header("Tilt")]
    public bool tiltToGround = true;
    public float tiltLerp = 10f;

    CharacterController cc;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void LateUpdate()
    {
        Vector3 start = transform.position + Vector3.up * rayStartHeight;

        if (!Physics.Raycast(start, Vector3.down, out RaycastHit hit, rayLength, groundMask, QueryTriggerInteraction.Ignore))
            return;

        float targetY = hit.point.y + hoverOffset;
        float currentY = transform.position.y;
        float deltaY = targetY - currentY;

        // clamp snap 
        deltaY = Mathf.Clamp(deltaY, -maxSnapDown, maxSnapUp);

        // smooth
        float step = deltaY * Mathf.Clamp01(Time.deltaTime * snapSpeed);

        // move vertically using CharacterController 
        cc.Move(new Vector3(0f, step, 0f));

        // tilt
        if (tiltToGround)
        {
            Vector3 fwd = transform.forward;
            fwd = Vector3.ProjectOnPlane(fwd, hit.normal).normalized;
            if (fwd.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(fwd, hit.normal);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * tiltLerp);
            }
        }
    }
}
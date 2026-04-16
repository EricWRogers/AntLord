using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AntGroundAlignCC : MonoBehaviour
{
    public LayerMask groundMask;

    [Header("Raycast")]
    public float rayStartHeight = 1.5f;
    public float rayLength = 5f;

    [Header("Snap")]
    public float hoverOffset = 0.03f;
    public float snapSpeed = 12f;
    public float maxSnapPerFrame = 0.05f;   
    public float snapDeadzone = 0.01f;      

    [Header("Smoothing")]
    public float groundYSmoothing = 20f;   

    [Header("Tilt")]
    public bool tiltToGround = false;      
    public Transform modelToTilt;
    public float tiltLerp = 10f;
    public float maxTiltDeg = 35f;

    CharacterController cc;
    float smoothedGroundY;
    bool hasGround;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!modelToTilt) modelToTilt = transform;
    }

    void LateUpdate()
    {
        Vector3 start = transform.position + Vector3.up * rayStartHeight;

        if (!Physics.Raycast(start, Vector3.down, out RaycastHit hit, rayLength, groundMask, QueryTriggerInteraction.Ignore))
            return;

        float rawGroundY = hit.point.y + hoverOffset;

        
        if (!hasGround)
        {
            smoothedGroundY = rawGroundY;
            hasGround = true;
        }
        else
        {
            smoothedGroundY = Mathf.Lerp(smoothedGroundY, rawGroundY, Time.deltaTime * groundYSmoothing);
        }

        float deltaY = smoothedGroundY - transform.position.y;

        
        if (Mathf.Abs(deltaY) < snapDeadzone)
            deltaY = 0f;

        if (deltaY != 0f)
        {
            
            float step = deltaY * Mathf.Clamp01(Time.deltaTime * snapSpeed);
            step = Mathf.Clamp(step, -maxSnapPerFrame, maxSnapPerFrame);

            cc.Move(new Vector3(0f, step, 0f));
        }

        if (tiltToGround && modelToTilt != null)
        {
            Vector3 up = hit.normal;

            float tiltAngle = Vector3.Angle(Vector3.up, up);
            if (tiltAngle > maxTiltDeg)
            {
                float t = maxTiltDeg / tiltAngle;
                up = Vector3.Slerp(Vector3.up, up, t);
            }

            Vector3 fwd = transform.forward;
            Quaternion targetTilt = Quaternion.LookRotation(Vector3.ProjectOnPlane(fwd, up).normalized, up);
            modelToTilt.rotation = Quaternion.Slerp(modelToTilt.rotation, targetTilt, Time.deltaTime * tiltLerp);
        }
    }
}
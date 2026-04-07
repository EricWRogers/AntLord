using UnityEngine;

[RequireComponent(typeof(AntMover))]
public class AntSlopeSpeed : MonoBehaviour
{
    public LayerMask groundMask; 

    [Header("Sampling")]
    public float probeOffset = 0.25f;
    public float rayStartHeight = 1.0f;
    public float rayLength = 3.0f;

    [Header("Slowdown Rules")]
    public float noSlowBelowDeg = 8f;        
    public float maxSlowSlopeDeg = 45f;      
    public float minSpeedMultiplier = 0.35f; 
    public float smoothing = 10f;

    AntMover mover;
    float currentMul = 1f;

    void Awake() => mover = GetComponent<AntMover>();

    void Update()
    {
        float slopeDeg = EstimateSlopeDegrees();

        float targetMul;
        if (slopeDeg <= noSlowBelowDeg)
        {
            targetMul = 1f;
        }
        else
        {
            float t = Mathf.InverseLerp(noSlowBelowDeg, maxSlowSlopeDeg, slopeDeg);
            targetMul = Mathf.Lerp(1f, minSpeedMultiplier, t);
        }

        currentMul = Mathf.Lerp(currentMul, targetMul, Time.deltaTime * smoothing);
        mover.SetSpeedMultiplier(currentMul);
    }

    float EstimateSlopeDegrees()
    {
        Vector3 p = transform.position;

        if (!SampleHeight(p + new Vector3(probeOffset, 0, 0), out float hA)) return 0f;
        if (!SampleHeight(p + new Vector3(-probeOffset, 0, 0), out float hB)) return 0f;
        if (!SampleHeight(p + new Vector3(0, 0, probeOffset), out float hC)) return 0f;
        if (!SampleHeight(p + new Vector3(0, 0, -probeOffset), out float hD)) return 0f;

        float dHx = Mathf.Abs(hA - hB) / (2f * probeOffset);
        float dHz = Mathf.Abs(hC - hD) / (2f * probeOffset);

        float grad = Mathf.Sqrt(dHx * dHx + dHz * dHz);
        return Mathf.Atan(grad) * Mathf.Rad2Deg;
    }

    bool SampleHeight(Vector3 world, out float y)
    {
        Vector3 start = world + Vector3.up * rayStartHeight;
        if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, rayLength, groundMask, QueryTriggerInteraction.Ignore))
        {
            y = hit.point.y;
            return true;
        }
        y = 0f;
        return false;
    }
}
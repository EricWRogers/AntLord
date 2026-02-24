using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SandImprinter : MonoBehaviour
{
    [Header("Footprint")]
    public float radius = 0.6f;          
    public float maxDepth = 1.0f;        
    public float strength = 6f;          // how fast it makes imppressions
    [Header("Displacement")]
    public bool makeBerm = true;
    public float bermRadiusMultiplier = 1.6f; 
    public float bermStrength = 0.5f;         //amount of sand that piles

    [Header("Performance")]
    public bool onlyWhenMoving = true;
    public float minMoveSpeed = 0.05f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public bool ShouldImprint()
    {
        if (!onlyWhenMoving) return true;
        if (rb == null) return true;
        return rb.linearVelocity.magnitude > minMoveSpeed;
    }

    
    public float BottomY()
    {
        var c = GetComponent<Collider>();
        return c.bounds.min.y;
    }
}

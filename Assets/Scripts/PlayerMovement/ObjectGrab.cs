using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObjectGrab : MonoBehaviour
{
    private Rigidbody rb;

    private Vector3 lastPosition;
    private Vector3 velocity;

    [SerializeField] private float throwMultiplier = 1.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // track velocity 
        if (!rb.useGravity)
        {
            velocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
            lastPosition = transform.position;
        }
    }

    public void StartDrag()
    {
        rb.useGravity = false;
        rb.linearDamping = 10f;
        rb.angularDamping = 10f;
        rb.freezeRotation = true;

        lastPosition = transform.position;
        velocity = Vector3.zero;
    }

    public void DragTo(Vector3 position)
    {
        rb.MovePosition(position);
    }

    public void EndDrag()
    {
        rb.useGravity = true;
        rb.freezeRotation = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;

        rb.linearVelocity = velocity * throwMultiplier;
    }
}
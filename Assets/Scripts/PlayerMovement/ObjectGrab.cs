using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObjectGrab : MonoBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void StartDrag()
    {
        rb.useGravity = false;
        rb.linearDamping = 10f;
        rb.angularDamping = 10f;
        rb.freezeRotation = true;
    }

    public void DragTo(Vector3 position)
    {
        rb.MovePosition(position);
    }

    public void EndDrag()
    {
        rb.useGravity = true;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.freezeRotation = false;
    }
}
using UnityEngine;

public class PhysicsGrabber : MonoBehaviour
{
    public Camera cam;
    public LayerMask grabbableMask;     
    public float maxGrabDistance = 50f;

    [Header("Joint Settings")]
    public float spring = 5000f;
    public float damper = 80f;
    public float maxDistance = 0.05f;   
    public float grabDepth = 10f;       

    Rigidbody grabbedRb;
    SpringJoint joint;
    GameObject grabPointObj;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        grabPointObj = new GameObject("GrabPoint");
    }

    void Update()
    {
        // Left click to grab
        if (Input.GetMouseButtonDown(0))
            TryGrab();

        // Release
        if (Input.GetMouseButtonUp(0))
            Release();

 
        if (grabbedRb != null)
            MoveGrabPoint();
    }

    void TryGrab()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance, grabbableMask))
            return;

        Rigidbody rb = hit.rigidbody;
        if (rb == null) return;

        grabbedRb = rb;

        
        grabPointObj.transform.position = hit.point;

        joint = grabbedRb.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = null; 
        joint.connectedAnchor = grabPointObj.transform.position;

        joint.spring = spring;
        joint.damper = damper;
        joint.maxDistance = maxDistance;

        //stop the spinning
        grabbedRb.angularDamping = 4f;
        grabbedRb.linearDamping = 0.5f;
    }

    void MoveGrabPoint()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // consistent depth
        Vector3 target = ray.origin + ray.direction * grabDepth;

        grabPointObj.transform.position = target;

        if (joint != null)
            joint.connectedAnchor = target;
    }

    void Release()
    {
        if (joint != null) Destroy(joint);
        joint = null;
        grabbedRb = null;
    }
}
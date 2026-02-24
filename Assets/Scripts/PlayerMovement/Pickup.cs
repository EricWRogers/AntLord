using UnityEngine;

public class Pickup : MonoBehaviour
{
    private ObjectGrab currentObject;
    private float objectDistance;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        // CLICK to grab
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent(out ObjectGrab grab))
                {
                    currentObject = grab;
                    objectDistance = hit.distance;
                    currentObject.StartDrag();
                }
            }
        }

        // HOLD to drag
        if (Input.GetMouseButton(0) && currentObject != null)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Vector3 targetPosition = ray.GetPoint(objectDistance);
            currentObject.DragTo(targetPosition);
        }

        // RELEASE to drop
        if (Input.GetMouseButtonUp(0) && currentObject != null)
        {
            currentObject.EndDrag();
            currentObject = null;
        }
    }
}
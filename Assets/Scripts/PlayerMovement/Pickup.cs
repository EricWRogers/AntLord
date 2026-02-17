using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private Transform objectGrabPointTransform;
    [SerializeField] private LayerMask pickupLayerMask;

    private ObjectGrab objectGrab;

    private void Update(){
        if (Input.GetKeyDown(KeyCode.E)){
            if(objectGrab != null){
                float pickUpDistance = 2f;
                if(Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out raycastHit, pickUpDistance, pickupLayerMask)){
                    if(raycastHit.transform.TryGetComponent(out objectGrab)){
                        objectGrab.Grab(objectGrabPointTransform);
                    }
                }
            } else {
                objectGrab.Drop();
                objectGrab = null;
            }
        }
    }
}

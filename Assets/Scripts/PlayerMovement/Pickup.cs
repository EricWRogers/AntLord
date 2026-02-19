using UnityEngine;

public class Pickup : MonoBehaviour{

    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private Transform objectGrabPointTransform;
    [SerializeField] private LayerMask pickupLayerMask;

    private ObjectGrab objectGrab;

    private void Update(){
        if(Input.GetKeyDown(KeyCode.E)){
            if(objectGrab == null){
                float pickUpDistance = 2f;
                if(Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out RaycastHit raycastHit, pickUpDistance, pickupLayerMask)){
                    if(raycastHit.collider.TryGetComponent(out objectGrab)){
                        objectGrab.Grab(objectGrabPointTransform);
                    }
                }
            } else{
                objectGrab.Drop();
                objectGrab = null;
            }
            
        }
    }
}
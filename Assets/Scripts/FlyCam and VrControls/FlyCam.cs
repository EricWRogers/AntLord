using UnityEngine;

//requires a camera to use
[RequireComponent(typeof(Camera))]
public class FlyCam : MonoBehaviour
{

    private bool active = true; //set the camera to active
    private bool allowRotation = true; //let the camera rotate
    private float mouseSenseitivity = 1f; //how sisitive the mouse is for cam rotation
    private bool scrollWheel = true; //let the camera zoom in/out with scroll wheel
    private float zoomSpeed = 50f; //how fast camera zooms
    private bool allowMovement = true; //let player move
    private float movementSpeed = 10f; //camera movement speed (W,A,S,D,Q,E)
    private bool acceleration = true; //to make camera movement feel better
    private float accelRate = 1.5f; //rate of acceleration
    private KeyCode ResetPosition = KeyCode.R; //resset the camera position

    private CursorLockMode wantedMode;
    private float increase = 1;
    private float increaseMem = 0;

    private Vector3 initPosition;
    private Vector3 initRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initPosition = transform.position;
        initRotation = transform.eulerAngles;
        
    }

// lock and unlock cursor for camera movement
    private void onEnable(){
        if(active)
            wantedMode = CursorLockMode.Locked; //lock the cursor if active
    }

    private void SetCursorState(){
        if(Input.GetKeyDown(KeyCode.Escape)){
            Cursor.lockState = wantedMode =  CursorLockMode.None; //unlock cursor when esc key pressed
        }
        if(Input.GetMouseButtonDown(0)){
            wantedMode = CursorLockMode.Locked; //lock cursor when game clicked
        }

        Cursor.lockState = wantedMode;
        Cursor.visible = (CursorLockMode.Locked != wantedMode); //hide cursor when locked
    }

    private void CalculateAcceleration(bool moving){
        increase = Time.deltaTime;
        if(!acceleration || acceleration && !moving){
            increaseMem = 0;
            return;
        }

        increaseMem += Time.deltaTime * (accelRate - 1);
        increase = Time.deltaTime + Mathf.Pow(increaseMem, 3) * Time.deltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(!active) //if camera not active, do nothing
            return;

        SetCursorState();
        if(Cursor.visible)
            return;

        if(scrollWheel){    //if scroll zoom is acive
            transform.Translate(Vector3.forward * Input.mouseScrollDelta.y * Time.deltaTime * zoomSpeed);
        }
        
        //movement
        if(allowMovement){
            Vector3 deltaPosition = Vector3.zero;
            float currentSpeed = movementSpeed;

            if(Input.GetKey(KeyCode.W)) //go forward
                deltaPosition += transform.forward;
            
            if(Input.GetKey(KeyCode.S)) //go back
                deltaPosition -=transform.forward;
            
            if(Input.GetKey(KeyCode.D)) //go right
                deltaPosition += transform.right;
            
            if(Input.GetKey(KeyCode.A)) //go left
                deltaPosition -= transform.right;
            
            if(Input.GetKey(KeyCode.Q)) //go up
                deltaPosition += transform.up;
            
            if(Input.GetKey(KeyCode.E)) //go down
                deltaPosition -= transform.up;
            
            CalculateAcceleration(deltaPosition != Vector3.zero); //caculate acceleration
            transform.position += deltaPosition * currentSpeed * increase;
        }
    }
}

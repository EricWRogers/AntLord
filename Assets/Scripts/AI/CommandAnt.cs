using UnityEngine;
using UnityEngine.InputSystem;


public class CommandAnt : CommandParent
{
    public InputActionAsset inputActions;
    private InputAction leftClick;
    private InputAction rightClick;
    private InputAction shift;
    private InputAction Num1;
    private InputAction Num2;
    private InputAction Deselect;
    private InputAction R;
    private InputAction Lasso;


    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!selectionRing) selectionRing = FindFirstObjectByType<SelectionRingController>();

        leftClick = InputSystem.actions.FindAction("LeftClick");
        rightClick = InputSystem.actions.FindAction("RightClick");
        shift = InputSystem.actions.FindAction("Shift");
        Num1 = InputSystem.actions.FindAction("Num1");
        Num2 = InputSystem.actions.FindAction("Num2");
        Deselect = InputSystem.actions.FindAction("Deselect");
        R = InputSystem.actions.FindAction("R");
        Lasso = InputSystem.actions.FindAction("Lasso");
    }

    void OnEnable()
    {
        inputActions.FindActionMap("Controls").Enable();
    }

    void OnDisable()
    {
        inputActions.FindActionMap("Controls").Disable();
    }

    void Update()
    {
        CheckLassoSelect(Lasso, false);

        if (leftClick.WasPressedThisFrame())
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, 500f))
                SimpleAntSelect(hit);
        }

        // -- Set a waypoint or set a task
        if (rightClick.WasPressedThisFrame())
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit hit, 500f))
                DirectAnt(hit);
        }

        // Deselect all with shift + R
        else if (shift.IsPressed() && R.WasPressedThisFrame())
        {
            DeselectAll();
        }

        // Switch to manual with 1
        else if (Num1.WasPressedThisFrame())
        {
            SwitchToManual();
        }

        // Switch to food with 2   
        else if (Num2.WasPressedThisFrame())
        {
            SwitchToFood();
        }
    }
}
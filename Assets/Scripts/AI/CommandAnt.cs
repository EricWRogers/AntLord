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
        // THIS SHOULD USE THE LASSO FUNCTION BUT UNITY INPUT LITERALLY DOES NOTHING
        bool shiftHeld = shift.IsPressed();

        // Start drag only when shift is held and LMB pressed
        if (shiftHeld && leftClick.WasPressedThisFrame())
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundMask))
            {
                shiftDragStart = hit.point;
                shiftDragging = true;

                if (selectionRing) selectionRing.Show(shiftDragStart, 0.1f);
            }
        }


        if (shiftDragging)
        {
            // If shift was released mid-drag, cancel 
            if (!shiftHeld && leftClick.IsPressed())
            {
                shiftDragging = false;
                if (selectionRing) selectionRing.Hide();
            }
            else
            {
                // Update ring while LMB held
                if (leftClick.IsPressed())
                {
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundMask))
                    {
                        
                        // center moves toward mouse, radius is half the distance
                        Vector3 current = hit.point;
                        Vector3 center = (shiftDragStart + current) * 0.5f;
                        float radius = Vector3.Distance(shiftDragStart, current) * 0.5f;

                        if (selectionRing) selectionRing.Show(center, radius);
                    }
                    else
                    {
                        
                        if (selectionRing) selectionRing.Hide();
                    }
                }

                // Release drag on mouse up, even if shift isn't held
                if (leftClick.WasReleasedThisFrame())
                {
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundMask))
                    {
                        Vector3 end = hit.point;
                        Vector3 center = (shiftDragStart + end) * 0.5f;
                        float radius = Vector3.Distance(shiftDragStart, end) * 0.5f;

                        // hide ring
                        if (selectionRing) selectionRing.Hide();

                    
                        ClearSelectionVisualsOnly();
                        selectedAnts.Clear();
                        selectedLeader = null;

                        Collider[] hits = Physics.OverlapSphere(center, radius);
                        foreach (var col in hits)
                        {
                            if (!col.CompareTag("Ant")) continue;
                            var brain = col.GetComponent<AntBrain>();
                            if (brain == null) continue;

                            if (brain.antType.teamID == 0)
                            {
                                selectedAnts.Add(col.gameObject);
                                SetGlow(col.gameObject, selectedColor, selectedIntensity);
                            }
                        }
                    }
                    else
                    {
                        if (selectionRing) selectionRing.Hide();
                    }

                    shiftDragging = false;
                }
            }
        }

        // END LASSO FUNCTION

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
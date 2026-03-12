using UnityEngine;
using UnityEngine.InputSystem;


public class CommandAnt : CommandParent
{
    public InputActionAsset inputActions;
    private InputAction leftClick;
    private InputAction shift;


    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!selectionRing) selectionRing = FindFirstObjectByType<SelectionRingController>();

        leftClick = InputSystem.actions.FindAction("LeftClick");
        shift = InputSystem.actions.FindAction("Shift");
    }

    void OnEnable()
    {
        inputActions.FindActionMap("Mouse + Keyboard").Enable();
    }

    void OnDisable()
    {
        inputActions.FindActionMap("Mouse + Keyboard").Disable();
    }

    void Update()
    {
        // THIS SHOULD USE THE LASSO FUNCTION BUT UNITY INPUT LITERALLY DOES NOTHING
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Start drag only when shift is held and LMB pressed
        if (shiftHeld && Input.GetMouseButtonDown(0))
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
            if (!shiftHeld && Input.GetMouseButton(0))
            {
                shiftDragging = false;
                if (selectionRing) selectionRing.Hide();
            }
            else
            {
                // Update ring while LMB held
                if (Input.GetMouseButton(0))
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
                if (Input.GetMouseButtonUp(0))
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

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, 500f))
                SimpleAntSelect(hit);
        }

        // -- Set a waypoint or set a task
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit hit, 500f))
                DirectAnt(hit);
        }


        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            DeselectAll();
        }

        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchToManual();
        }
            
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchToFood();
        }
    }
}
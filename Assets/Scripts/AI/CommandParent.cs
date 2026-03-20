using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public abstract class CommandParent : MonoBehaviour
{
    public Camera cam;
    public List<GameObject> selectedAnts;
    public LeadNav selectedLeader = null;
    public GameObject wayPointPrefab;
    private bool antSelect { get {return selectedAnts.Count > 0;} }

    [Header("Drag Select")]
    public float sphereCastRadius = 2;
    protected Vector3 shiftDragStart;
    protected bool shiftDragging = false;

    [Header("Selection Glow")]
    public Color selectedColor = Color.green;
    public float selectedIntensity = 2.5f;
    public Color leaderColor = Color.yellow;
    public float leaderIntensity = 3.0f;
    public AntTask taskToAssign = AntTask.Manual;
    public SelectionRingController selectionRing;
    public LayerMask groundMask;

    // void Awake()
    // {
    //     if (!cam) cam = Camera.main;
    //     if (!selectionRing) selectionRing = FindFirstObjectByType<SelectionRingController>();
    // }

    public void SimpleAntSelect(RaycastHit hit)
    {

        if (selectedLeader == null || selectedLeader.target != selectedLeader.home){

            Collider[] hits = Physics.OverlapSphere(hit.point, sphereCastRadius);

            // If clicked directly on an Ant, toggle selection
            if (hit.transform.CompareTag("Ant") && hit.transform.GetComponent<AntBrain>().antType.teamID == 0)
            {
                ToggleSelection(hit.transform.gameObject);
            }
            else
            {
                
                foreach (Collider col in hits)
                {
                    if (col.CompareTag("Ant"))
                    {
                        ToggleSelection(col.transform.gameObject);
                    }
                }
            }
        }
    }

    public void DirectAnt(RaycastHit hit)
    {
        //Debug.LogWarning("SelectedLeader: " + selectedLeader.gameObject.name);
        if(selectedLeader == null || selectedLeader.target != selectedLeader.home){
            //Debug.LogWarning("1st pass");
            if(taskToAssign == AntTask.Manual)
            {
                //Debug.LogWarning("2nd pass");
                //Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                if (antSelect)
                {
                    //Debug.LogWarning("3rd pass");
                    ElectLeader();
                    selectedLeader.task = AntTask.Manual;

                    if (hit.transform.CompareTag("Food"))
                        selectedLeader.target = hit.transform;
                    else
                        selectedLeader.target = Instantiate(wayPointPrefab, hit.point, Quaternion.identity).transform;
                }
            }
            else if(taskToAssign == AntTask.Food && selectedAnts.Count != 0 && !antSelect) // what does antSelect mean?
            {
                float sphereRadius = 25f;
                Collider[] hits = Physics.OverlapSphere(selectedAnts[0].transform.position, sphereRadius);
                bool targetYet = false;
                int tries = 1;


                while(!targetYet && sphereRadius <= 500)
                {

                    foreach(Collider col in hits)
                    {
                        if (col.CompareTag("Food"))
                        {
                            Debug.Log("Found food to target!");
                            ElectLeader();
                            selectedLeader.target = col.transform;
                            selectedLeader.task = AntTask.Food;
                            targetYet = true;

                            break;
                        }
                    }

                    if(!targetYet)
                    {
                        sphereRadius *= 2;

                        if(sphereRadius <= 500) // should be relative to map size
                        {
                            Debug.Log($"Going for try {++tries}"); 
                            hits = Physics.OverlapSphere(selectedAnts[0].transform.position, sphereRadius);
                        }
                        else
                            Debug.Log("Gave up on finding food");
                    }

                }
            }
        }
    }

    public void DeselectAll()
    {
        ClearSelectionVisualsOnly();
        selectedAnts.Clear();
        selectedLeader = null;
    }

    public void SwitchToManual()
    {
        Debug.Log("Switching to Manual");
        taskToAssign = AntTask.Manual;
    }

    public void SwitchToFood()
    {
        Debug.Log("Switching to Food");
        taskToAssign = AntTask.Food;
    }

    // public void LassoSelect()
    // {
    //     // Convenience wrapper: use configured action references.
    //     LassoSelect(shiftAction?.action, primaryClick?.action);
    // }

    public bool LassoSelect(InputAction shiftInput, InputAction primaryInput)
    {
        // --- shift + drag selection (fixed) ---
        bool shiftHeld = shiftInput != null && shiftInput.ReadValue<float>() > 0.1f;
        bool primaryDown = primaryInput != null && primaryInput.WasPerformedThisFrame();
        bool primaryHold = primaryInput != null && primaryInput.ReadValue<float>() > 0.1f;
        bool primaryUp = primaryInput != null && primaryInput.WasReleasedThisFrame();

        // Start drag only when shift is held and LMB pressed
        if (shiftHeld && primaryDown)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
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
            if (!shiftHeld && primaryHold)
            {
                shiftDragging = false;
                if (selectionRing) selectionRing.Hide();
                return false;
            }
            else
            {
                // Update ring while LMB held
                if (primaryHold)
                {
                    Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
                    if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundMask))
                    {
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
                if (primaryUp)
                {
                    Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
                    if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundMask))
                    {
                        Vector3 end = hit.point;
                        Vector3 center = (shiftDragStart + end) * 0.5f;
                        float radius = Vector3.Distance(shiftDragStart, end) * 0.5f;

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

        if(!(shiftHeld && shiftDragging))
            return false;
        else
            return true;
    }

    public void ElectLeader()
    {
        //Debug.Log("Selecting new leader!");

        foreach (GameObject ant in selectedAnts)
        {
            ant.GetComponent<LeadNav>().enabled = false;
            ant.GetComponent<FollowNav>().enabled = true;
        }

        selectedAnts[0].GetComponent<FollowNav>().enabled = false;
        selectedLeader = selectedAnts[0].GetComponent<LeadNav>();
        selectedAnts[0].GetComponent<LeadNav>().enabled = true;

        selectedLeader.home = FindFirstObjectByType<SpawnerBuilding>().transform; // TEMP

        
        selectedLeader.followers.Clear();

        for (int i = 1; i < selectedAnts.Count; i++)
        {
            selectedAnts[i].GetComponent<FollowNav>().leader = selectedLeader;
            selectedAnts[i].GetComponent<FollowNav>().crumbTrack = 0;

            
            var follow = selectedAnts[i].GetComponent<FollowNav>();
            if (follow.myAgent == null) follow.myAgent = selectedAnts[i].GetComponent<NavMeshAgent>();

            selectedLeader.followers.Add(follow.myAgent);
        }

        selectedLeader.GetComponent<FollowNav>().enabled = false;

        selectedLeader.crumbs.Clear();

        selectedLeader.myAgent = selectedLeader.GetComponent<NavMeshAgent>();
        selectedLeader.myAgent.isStopped = false;

        for (int i = 0; i < selectedLeader.followers.Count; i++)
            selectedLeader.followers[i].isStopped = false;

        // Make leader a different color before clearing
        SetGlow(selectedAnts[0], leaderColor, leaderIntensity);

    }

    //visualizzation methods

    public void ToggleSelection(GameObject ant)
    {
        if (ant == null) return;

        if (selectedAnts.Contains(ant))
        {
            // deselect
            selectedAnts.Remove(ant);
            DisableGlow(ant);
        }
        else
        {
            // select
            selectedAnts.Add(ant);
            SetGlow(ant, selectedColor, selectedIntensity);
        }
    }

    public void SetGlow(GameObject ant, Color c, float intensity)
    {
        var glow = ant.GetComponent<AntSelectGlow>();
        if (glow != null) glow.EnableGlow(c, intensity);

        // TEMP
        if(glow != null) glow.marker.SetActive(true);
    }

    public void DisableGlow(GameObject ant)
    {
        var glow = ant.GetComponent<AntSelectGlow>();
        if (glow != null) glow.DisableGlow();

        // TEMP
        if(glow != null) glow.marker.SetActive(false);
    }

    public void ClearSelectionVisualsOnly()
    {
        for (int i = 0; i < selectedAnts.Count; i++)
        {
            if (selectedAnts[i] == null) continue;
            DisableGlow(selectedAnts[i]);
        }
    }
}
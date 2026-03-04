using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CommandAnt : MonoBehaviour
{
    public Camera cam;
    public List<GameObject> selectedAnts = new List<GameObject>();
    public LeadNav selectedLeader;
    public GameObject wayPointPrefab;
    private bool antSelect = false;

    [Header("Drag Select")]
    public float sphereCastRadius = 2;
    private Vector3 shiftDragStart;
    private bool shiftDragging = false;

    [Header("Selection Glow")]
    public Color selectedColor = Color.green;
    public float selectedIntensity = 2.5f;
    public Color leaderColor = Color.yellow;
    public float leaderIntensity = 3.0f;
    public AntTask taskToAssign = AntTask.Manual;
    public SelectionRingController selectionRing;
    public LayerMask groundMask;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!selectionRing) selectionRing = FindFirstObjectByType<SelectionRingController>();
    }

    void Update()
    {
        // --- shift + drag selection ---
        if (Input.GetKey(KeyCode.LeftShift))
        {
            // start drag
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundMask))
                {
                    shiftDragStart = hit.point;
                    shiftDragging = true;

                    // start ring (tiny)
                    if (selectionRing) selectionRing.Show(shiftDragStart, 0.1f);
                }
            }

            // while dragging
            if (shiftDragging && Input.GetMouseButton(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundMask))
                {
                    float radius = Vector3.Distance(shiftDragStart, hit.point);
                    if (selectionRing) selectionRing.Show(shiftDragStart, radius);
                }
            }

            // release drag
            if (shiftDragging && Input.GetMouseButtonUp(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundMask))
                {
                    float radius = Vector3.Distance(shiftDragStart, hit.point);

                    // hide ring once selection made
                    if (selectionRing) selectionRing.Hide();

                    
                    ClearSelectionVisualsOnly();
                    selectedAnts.Clear();
                    selectedLeader = null;

                    Collider[] hits = Physics.OverlapSphere(shiftDragStart, radius);
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
                    // couldn't raycast to ground on release
                    if (selectionRing) selectionRing.Hide();
                }

                shiftDragging = false;
            }
}

        // normal click selection
        else if (Input.GetMouseButtonDown(0))
        {
            antSelect = false;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if ((selectedLeader == null || selectedLeader.target != selectedLeader.home) &&
                Physics.Raycast(ray, out RaycastHit hit, 500f))
            {
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
                            antSelect = true;
                        }
                    }
                }
            }
        }

        // -- Set a waypoint or set a task
        if (Input.GetMouseButtonDown(1) && (selectedLeader == null || selectedLeader.target != selectedLeader.home))
        {
            if(taskToAssign == AntTask.Manual)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                if (!antSelect && selectedAnts.Count != 0 && Physics.Raycast(ray, out RaycastHit hit, 500f))
                {
                    ElectLeader();

                    if (hit.transform.CompareTag("Food"))
                        selectedLeader.target = hit.transform;
                    else
                        selectedLeader.target = Instantiate(wayPointPrefab, hit.point, Quaternion.identity).transform;

                    
                }
            }
            else if(taskToAssign == AntTask.Food && selectedAnts.Count != 0)
            {
                float sphereRadius = 25f;
                Collider[] hits = Physics.OverlapSphere(selectedAnts[0].transform.position, sphereRadius);
                bool targetYet = false;
                int tries = 1;

                // what does antSelect mean?
                if (!antSelect)
                {
                    while(!targetYet && sphereRadius <= 500)
                    {

                        foreach(Collider col in hits)
                        {
                            if (col.CompareTag("Food"))
                            {
                                Debug.Log("Found food to target!");
                                ElectLeader();
                                selectedLeader.target = col.transform;
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


        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            ClearSelectionVisualsOnly();
            selectedAnts.Clear();
            selectedLeader = null;
        }

        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Switching to Manual");
            taskToAssign = AntTask.Manual;
        }
            
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Switching to Food");
            taskToAssign = AntTask.Food;
        }
    }

    void ElectLeader()
    {
        Debug.Log("Selecting new leader!");

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

    void ToggleSelection(GameObject ant)
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

    void SetGlow(GameObject ant, Color c, float intensity)
    {
        var glow = ant.GetComponent<AntSelectGlow>();
        if (glow != null) glow.EnableGlow(c, intensity);

        // TEMP
        if(glow != null) glow.marker.SetActive(true);
    }

    void DisableGlow(GameObject ant)
    {
        var glow = ant.GetComponent<AntSelectGlow>();
        if (glow != null) glow.DisableGlow();

        // TEMP
        if(glow != null) glow.marker.SetActive(false);
    }

    void ClearSelectionVisualsOnly()
    {
        for (int i = 0; i < selectedAnts.Count; i++)
        {
            if (selectedAnts[i] == null) continue;
            DisableGlow(selectedAnts[i]);
        }
    }
}
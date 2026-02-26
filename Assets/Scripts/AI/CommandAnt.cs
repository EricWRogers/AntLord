using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CommandAnt : MonoBehaviour
{
    public Camera cam;
    public List<GameObject> selectedAnts = new List<GameObject>();
    public LeadNav selectedLeader;
    public float sphereCastRadius = 2;
    public GameObject wayPointPrefab;
    private bool antSelect = false;

    // for shift-drag selection
    private Vector3 shiftDragStart;
    private bool shiftDragging = false;

    [Header("Selection Glow")]
    public Color selectedColor = Color.green;
    public float selectedIntensity = 2.5f;
    public Color leaderColor = Color.yellow;
    public float leaderIntensity = 3.0f;

    void Awake()
    {
        if (!cam) cam = Camera.main;
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
                if (Physics.Raycast(ray, out RaycastHit hit, 500f))
                {
                    shiftDragStart = hit.point;
                    shiftDragging = true;
                }
            }

            // release drag
            if (shiftDragging && Input.GetMouseButtonUp(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 500f))
                {
                    float radius = Vector3.Distance(shiftDragStart, hit.point);

                    // clear existing selection
                    ClearSelectionVisualsOnly();
                    selectedAnts.Clear();
                    selectedLeader = null;

                    Collider[] hits = Physics.OverlapSphere(shiftDragStart, radius);
                    foreach (var col in hits)
                    {
                        if (col.CompareTag("Ant"))
                        {
                            selectedAnts.Add(col.gameObject);
                            SetGlow(col.gameObject, selectedColor, selectedIntensity);
                        }
                    }
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
                if (hit.transform.CompareTag("Ant"))
                {
                    ToggleSelection(hit.transform.gameObject);
                }
                else
                {
                    
                    foreach (var col in hits)
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

        // -- Set a waypoint
        if (Input.GetMouseButtonDown(1) && (selectedLeader == null || !selectedLeader.amCarryingFood))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (!antSelect && selectedAnts.Count != 0 && Physics.Raycast(ray, out RaycastHit hit, 500f))
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

                selectedLeader.crumbs.Clear();

                if (hit.transform.CompareTag("Food"))
                    selectedLeader.target = hit.transform;
                else
                    selectedLeader.target = Instantiate(wayPointPrefab, hit.point, Quaternion.identity).transform;

                selectedLeader.myAgent = selectedLeader.GetComponent<NavMeshAgent>();
                selectedLeader.myAgent.isStopped = false;

                for (int i = 0; i < selectedLeader.followers.Count; i++)
                    selectedLeader.followers[i].isStopped = false;

                // Make leader a different color before clearing
                SetGlow(selectedAnts[0], leaderColor, leaderIntensity);

                // Going into action removes highlight
                // ClearSelectionVisualsOnly();
                // selectedAnts.Clear();
                selectedLeader = null;
            }
        }


        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            ClearSelectionVisualsOnly();
            selectedAnts.Clear();
            selectedLeader = null;
        }
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
    }

    void DisableGlow(GameObject ant)
    {
        var glow = ant.GetComponent<AntSelectGlow>();
        if (glow != null) glow.DisableGlow();
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
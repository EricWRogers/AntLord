using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CommandAnt : MonoBehaviour
{
    public Camera cam;
    public List<GameObject> selectedAnts = new List<GameObject>();
    public LeadNav selectedLeader;
    public float sphereCastRadius = 2f;
    public GameObject wayPointPrefab;

    private bool antSelect = false;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
       
        if (Input.GetMouseButtonDown(0))
        {
            antSelect = false;

            
            ClearSelection();

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if ((selectedLeader == null || selectedLeader.target != selectedLeader.home) &&
                Physics.Raycast(ray, out RaycastHit hit, 500f))
            {
               
                if (hit.transform.CompareTag("Ant"))
                {
                    AddToSelection(hit.transform.gameObject);
                }
                else
                {
                    
                    Collider[] hits = Physics.OverlapSphere(hit.point, sphereCastRadius);
                    foreach (var col in hits)
                    {
                        if (col.CompareTag("Ant"))
                        {
                            AddToSelection(col.transform.gameObject);
                            antSelect = true;
                        }
                    }
                }
            }
        }

        
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
                    var f = selectedAnts[i].GetComponent<FollowNav>();
                    f.leader = selectedLeader;
                    f.crumbTrack = 0;

                    
                    if (f.myAgent == null) f.myAgent = selectedAnts[i].GetComponent<NavMeshAgent>();
                    selectedLeader.followers.Add(f.myAgent);
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

                
                SetLeaderGlow(selectedAnts[0], true);
            }
        }
        else if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.R))
        {
            ClearSelection();
        }
    }

    void ClearSelection()
    {
       
        foreach (var ant in selectedAnts)
        {
            if (ant == null) continue;
            var glow = ant.GetComponent<AntSelectGlow>();
            if (glow != null) glow.SetSelected(false);
        }

        selectedAnts.Clear();
        selectedLeader = null;
    }

    void AddToSelection(GameObject ant)
    {
        if (ant == null) return;
        if (selectedAnts.Contains(ant)) return;

        selectedAnts.Add(ant);

        var glow = ant.GetComponent<AntSelectGlow>();
        if (glow != null) glow.SetSelected(true);
    }

    void SetLeaderGlow(GameObject leaderAnt, bool isLeader)
    {
        var glow = leaderAnt.GetComponent<AntSelectGlow>();
        if (glow == null) return;

        if (isLeader)
        {
            
            glow.EnableGlow(Color.yellow, 3f);
        }
        else
        {
            glow.SetSelected(true);
        }
    }
}
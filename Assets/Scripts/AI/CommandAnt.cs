using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CommandAnt : MonoBehaviour
{
    public Camera cam;
    public List<GameObject> selectedAnts;
    public LeadNav selectedLeader;
    public float sphereCastRadius = 2;
    public GameObject wayPointPrefab;
    private bool antSelect = false;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        // if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        // {

        if (Input.GetMouseButtonDown(0))
        {
            antSelect = false;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            
            if ((selectedLeader == null || selectedLeader.target != selectedLeader.home) && Physics.Raycast(ray, out RaycastHit hit, 500f))
            {
                Collider[] hits = Physics.OverlapSphere(hit.point, sphereCastRadius);
                // Add ants
                if(hit.transform.CompareTag("Ant") && !selectedAnts.Contains(hit.transform.gameObject))
                {
                    selectedAnts.Add(hit.transform.gameObject);
                }

                else
                {
                    foreach(var col in hits)
                    {
                        if(col.CompareTag("Ant") && !selectedAnts.Contains(col.transform.gameObject))
                        {
                            selectedAnts.Add(col.transform.gameObject);
                            antSelect = true;
                        }
                    }
                }
            }
        }
        
        // -- Set a waypoint
        if(Input.GetMouseButtonDown(1) && (selectedLeader == null || !selectedLeader.amCarryingFood))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if(!antSelect && selectedAnts.Count != 0 && Physics.Raycast(ray, out RaycastHit hit, 500f))
            {
                Debug.Log("Selecting new leader!");
                
                foreach(GameObject ant in selectedAnts)
                {
                    ant.GetComponent<LeadNav>().enabled = false;
                    ant.GetComponent<FollowNav>().enabled = true;
                }

                selectedAnts[0].GetComponent<FollowNav>().enabled = false;
                selectedLeader = selectedAnts[0].GetComponent<LeadNav>();
                selectedAnts[0].GetComponent<LeadNav>().enabled = true;

                selectedLeader.home = FindFirstObjectByType<SpawnerBuilding>().transform; // TEMP

                for(int i = 1; i < selectedAnts.Count; i++)
                {
                    selectedAnts[i].GetComponent<FollowNav>().leader = selectedLeader;
                    selectedAnts[i].GetComponent<FollowNav>().crumbTrack = 0;
                    selectedLeader.followers.Add(selectedAnts[i].GetComponent<FollowNav>().myAgent);
                }

                selectedLeader.crumbs.Clear();
                
                if(hit.transform.CompareTag("Food"))
                    selectedLeader.target = hit.transform;
                else
                    selectedLeader.target = Instantiate(wayPointPrefab, hit.point, Quaternion.identity).transform;

                selectedLeader.myAgent = selectedLeader.GetComponent<NavMeshAgent>();
                selectedLeader.myAgent.isStopped = false;

                for (int i = 0; i < selectedLeader.followers.Count; i++)
                    selectedLeader.followers[i].isStopped = false;
        }
        }

        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            selectedAnts.Clear();
            selectedLeader = null;
        }

        //  }
    }
}
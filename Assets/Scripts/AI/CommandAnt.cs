using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CommandAnt : MonoBehaviour
{
    public Camera cam;
    public List<GameObject> selectedAnts;
    public LeadNav selectedLeader;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if ((selectedLeader == null || selectedLeader.target != selectedLeader.home) && Physics.Raycast(ray, out RaycastHit hit, 500f))
                {
                    // Add ants
                    if(hit.transform.tag == "Ant" && !selectedAnts.Contains(hit.transform.gameObject))
                    {
                        selectedAnts.Add(hit.transform.gameObject);
                    }

                    // Target food
                    else if(hit.transform.tag == "Food")
                    {
                        // handle leader selection
                        if(selectedLeader == null)
                        {
                            Debug.Log("Selecting new leader!");
                            
                            foreach(GameObject ant in selectedAnts)
                            {
                                if(ant.GetComponent<LeadNav>() != null)
                                {
                                    Destroy(ant.GetComponent<LeadNav>());
                                    ant.AddComponent<FollowNav>();
                                }
                            }

                            Destroy(selectedAnts[0].GetComponent<FollowNav>());
                            selectedLeader = selectedAnts[0].AddComponent<LeadNav>();
                            selectedLeader.crumbs = new List<Vector3>();
                            selectedLeader.followers = new List<NavMeshAgent>();
                            selectedLeader.home = GameObject.Find("Home").transform; // TEMP

                            for(int i = 1; i < selectedAnts.Count; i++)
                            {
                                selectedAnts[i].GetComponent<FollowNav>().leader = selectedLeader;
                                selectedLeader.followers.Add(selectedAnts[i].GetComponent<FollowNav>().myAgent);
                            }
                        }

                        selectedLeader.target = hit.transform;
                        selectedLeader.myAgent.isStopped = false;

                        foreach(NavMeshAgent navAnt in selectedLeader.followers)
                            navAnt.isStopped = false;

                    }
                }
            }
            // else if (Input.GetMouseButtonDown(1))
            // {

            // }

            else if (Input.GetKeyDown(KeyCode.R))
            {
                selectedAnts.Clear();
                selectedLeader = null;
            }

        }
    }
}

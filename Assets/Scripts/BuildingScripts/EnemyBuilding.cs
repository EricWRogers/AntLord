using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class EnemyBuilding : Buildings
{
    [SerializeField] BuildingSO spawnerSO;
    public Transform spawnPoint;
    public float spawnPadding = 5.0f;
    float timer = 0.0f;
    public float spawnCooldown = 10.0f;
    public float radius = 5.0f;
    public int maxAnts = 10;
    List<GameObject> ants = new List<GameObject>();
    public GameObject antPrefab;
    public int maxHealth = 10;

    public TextMeshProUGUI timerText;

    [Tooltip("TEMPORARY! Win screen")]
    public GameObject winScreen;

    
    private bool hasWon = false;

    void Start()
    {
        teamID = 1;
        currentHealth = maxHealth;
        slider.maxValue = currentHealth;
        slider.value = currentHealth;

        winScreen.SetActive(false);
    }

    void FixedUpdate()
    {
        if (!hasWon)
        {
            if (ants.Count < maxAnts && this.currentHealth > 0)
            {
                timer += Time.deltaTime;
                if (timer >= spawnCooldown)
                {
                    SpawnAnt();
                    timer = 0;
                }
            }
            else if (timerText != null)
            {
                timerText.text = "All Ants Spawned";
            }

            // WIN CONDITION
            if (this.currentHealth <= 0)
            {
                hasWon = true;

                FindFirstObjectByType<MM>()?.Pause();
                winScreen.SetActive(true);

                
                AudioManager2.instance?.Stop("AntMarch");
                AudioManager2.instance?.Stop("MenuMusic");

                // Play win music once
                AudioManager2.instance?.Play("WinMusic");
            }
        }

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timerText != null && !hasWon)
        {
            float timeRemaining = Mathf.Max(0, spawnCooldown - timer);
            timerText.text = $"Next Spawn: {timeRemaining:F1}s";
        }
    }

    void SpawnAnt()
    {
        float randomX = 0.0f;
        float randomZ = 0.0f;
        while (randomX < transform.localScale.x && randomX > -transform.localScale.x)
        {
            randomX = transform.position.x + transform.localScale.x + Random.Range(-(transform.localScale.x + radius), transform.localScale.x + radius);
        }
        while (randomZ < transform.localScale.z && randomZ > -transform.localScale.z)
        {
            randomZ = transform.position.z + transform.localScale.z + Random.Range(-(transform.localScale.z + radius), transform.localScale.z + radius);
        }

        Vector3 spawn = new Vector3(randomX, transform.position.y, randomZ);

        GameObject newAnt = Instantiate(antPrefab, spawn, Quaternion.identity);

        AntBrain brain = newAnt.GetComponent<AntBrain>();
        LeadNav ln = newAnt.GetComponent<LeadNav>();
        FollowNav fn = newAnt.GetComponent<FollowNav>();

        LeadNav existingLeader = FindActiveEnemyLeader(brain.antType.teamID);

        if (existingLeader == null)
        {
            ln.enabled = true;
            if (fn != null) fn.enabled = false;

            ln.home = this.transform;

            Debug.Log($"<color=orange>Enemy Spawner: First ant is now a Leader.</color>");
        }
        else
        {
            ln.enabled = false;
            if (fn != null)
            {
                fn.enabled = true;
                fn.leader = existingLeader;

                if (existingLeader.followers == null) existingLeader.followers = new List<FollowNav>();
                if (!existingLeader.followers.Contains(fn))
                    existingLeader.followers.Add(fn);
            }
        }

        ants.Add(newAnt);
    }

    private LeadNav FindActiveEnemyLeader(int teamID)
    {
        LeadNav[] allLeaders = Object.FindObjectsByType<LeadNav>(FindObjectsSortMode.None);
        foreach (LeadNav leader in allLeaders)
        {
            AntBrain b = leader.GetComponent<AntBrain>();
            if (leader.enabled && b != null && b.antType.teamID == teamID)
            {
                return leader;
            }
        }
        return null;
    }
}
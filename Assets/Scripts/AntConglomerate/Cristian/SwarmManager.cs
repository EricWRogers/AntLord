using UnityEngine;
using System.Collections.Generic;

public class SwarmManager : MonoBehaviour
{
    public SwarmAnt antPrefab;
    public int antCount = 80;

    [Header("Spawn Area (XZ), y comes from ground snap")]
    public Vector2 spawnExtents = new Vector2(15f, 15f);
    public LayerMask groundMask; //the ground layer that the terrain or layer mech collkider or whatever on
 
    [Header("Swarm Target")]
    public Transform swarmTarget; // enemy or point to swarm

    [HideInInspector] public List<SwarmAnt> ants = new List<SwarmAnt>();

    private static Dictionary<Transform, SwarmManager> activeSwarms = new Dictionary<Transform, SwarmManager>(); // Track swarms by target

    void Start()
    {
        // Only spawn if no target is set (for manual swarms)
        if (swarmTarget == null)
        {
            ants.Clear();

            for (int i = 0; i < antCount; i++)
            {
                Vector3 p = transform.position + new Vector3(
                    Random.Range(-spawnExtents.x, spawnExtents.x),
                    25f,
                    Random.Range(-spawnExtents.y, spawnExtents.y)
                );

                p = SnapToGround(p);

                var a = Instantiate(antPrefab, p, Quaternion.identity);
                a.manager = this;
                ants.Add(a);
            }
        }
    }

    public Vector3 GetTargetPosition()
    {
        if (swarmTarget != null) return swarmTarget.position;
        return transform.position;
    }

    public bool TryGetGroundInfo(Vector3 fromAbove, out Vector3 point, out Vector3 normal)
    {
        if (Physics.Raycast(fromAbove, Vector3.down, out RaycastHit hit, 200f, groundMask, QueryTriggerInteraction.Ignore))
        {
            point = hit.point;
            normal = hit.normal;
            return true;
        }
        point = fromAbove;
        normal = Vector3.up;
        return false;
    }

    public Vector3 SnapToGround(Vector3 fromAbove)
    {
        if (TryGetGroundInfo(fromAbove, out var p, out _)) return p;
        return fromAbove;
    }

    // Adds a static method to get or create a SwarmManager for a target
    public static SwarmManager GetOrCreateSwarm(Transform target, SwarmAnt antPrefab, LayerMask groundMask, Vector2 spawnExtents)
    {
        if (activeSwarms.TryGetValue(target, out SwarmManager manager))
        {
            return manager;
        }
        
        // Create a new SwarmManager
        GameObject swarmObj = new GameObject("SwarmManager_" + target.name);
        manager = swarmObj.AddComponent<SwarmManager>();
        manager.antPrefab = antPrefab;
        manager.groundMask = groundMask;
        manager.spawnExtents = spawnExtents;
        manager.swarmTarget = target;
        activeSwarms[target] = manager;
        return manager;
    }

    // A method to add an existing SwarmAnt
    public void AddAnt(SwarmAnt ant)
    {
        if (!ants.Contains(ant))
        {
            ant.manager = this;
            ants.Add(ant);
        }
    }

    // A method to remove an ant (when swarm ends / self exaplianatory they just kinda spawn)
    public void RemoveAnt(SwarmAnt ant)
    {
        ants.Remove(ant);
        if (ants.Count == 0)
        {
            activeSwarms.Remove(swarmTarget);
            Destroy(gameObject);
        }
    }
}

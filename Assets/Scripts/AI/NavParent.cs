using UnityEngine;

public enum AntTask {Manual, Food, Materials, Fight}

// NOTE: Old version required NavMeshAgent. New system uses AntMover + CharacterController instead.
// [RequireComponent(typeof(NavMeshAgent))]
public abstract class NavParent : MonoBehaviour
{
    // Old: public NavMeshAgent myAgent;
    public AntMover mover;

    public Transform recentCollision = null;
    public float separationRadius = 2f;
    public float separationForce = 5f;
    public bool amCarryingFood = false;
    public int antTier = 1;

    
    public float separationFalloff = 2.0f;

    
    public float separationSteerScale = 1.0f;

   
    public float separationSteerSmoothing = 12f;

    
    Vector3 smoothSteer = Vector3.zero;

    public virtual void Start()
    {
        // Old:
        // myAgent = GetComponent<NavMeshAgent>();
        // if(myAgent.navMeshOwner != null) myAgent.isStopped = false;

        mover = GetComponent<AntMover>();
        if (mover == null) mover = gameObject.AddComponent<AntMover>();
    }

    
    public void HandleAgentCollisions()
    {
        // OLD BEHAVIOR:
        // This used to physically shove the CharacterController sideways using Move().
        

        // NEW BEHAVIOR:
        // Compute a small steering bias and feed that into AntMover
        if (mover == null) return;

        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, separationRadius);
        Vector3 separationVector = Vector3.zero;

        foreach (Collider collider in nearbyColliders)
        {
            if (collider.transform == transform) continue;
            if (!collider.CompareTag("Ant")) continue;

            Vector3 awayFromAgent = (transform.position - collider.transform.position);
            awayFromAgent.y = 0f;

            float d2 = awayFromAgent.sqrMagnitude;
            if (d2 < 0.0001f) continue;

            float d = Mathf.Sqrt(d2);

            // Weight: closer ants push more, farther ants push less
            float t = Mathf.Clamp01(1f - (d / separationRadius));
            float w = Mathf.Pow(t, separationFalloff);

            separationVector += (awayFromAgent / d) * w;
        }

        Vector3 targetSteer = Vector3.zero;

        if (separationVector.sqrMagnitude > 0f)
        {
            // New version nudges movement direction slightly
            targetSteer = separationVector.normalized * Mathf.Clamp01(separationForce * 0.2f) * separationSteerScale;
        }

        // smooth the steering so it doesn't flip directions every frame
        smoothSteer = Vector3.Lerp(smoothSteer, targetSteer, Time.deltaTime * separationSteerSmoothing);

        mover.SetSteering(smoothSteer);
    }
}
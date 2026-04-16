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
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, separationRadius);
        Vector3 separationVector = Vector3.zero;

        foreach (Collider collider in nearbyColliders)
        {
            if (collider.transform == transform) continue;
            if (!collider.CompareTag("Ant")) continue;

            Vector3 awayFromAgent = (transform.position - collider.transform.position);
            awayFromAgent.y = 0f;
            if (awayFromAgent.sqrMagnitude > 0.0001f)
                separationVector += awayFromAgent.normalized;
        }

        if (separationVector.sqrMagnitude > 0f)
        {
            // Old version offset the NavMeshAgent destination.
            // New version nudges position slightly; movement still follows path/crumb goals.
            //transform.position += separationVector.normalized * (separationForce * Time.deltaTime * 0.1f);
            transform.GetComponent<CharacterController>().Move(separationVector.normalized * (separationForce * Time.deltaTime * 0.1f));
        }
    }
}
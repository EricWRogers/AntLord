using UnityEngine;

public class PlayerAnimate : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private string parameterName = "isTelepathy"; // Name of the Animator parameter to toggle

    [Header("Timing (Seconds)")]
    [SerializeField] private float minTime = 5.0f; // Minimum time before toggling the animation
    [SerializeField] private float maxTime = 15.0f; // Maximum time before toggling the animation
    [SerializeField] private float telepathDuration = 3.0f; // Duration of the telepathy animation

    private Animator animator;
    private float timer;
    private float currentThreshold;
    private bool isCurrentlyTelepathic = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        bool parameterExists = false;
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == parameterName) 
            {
                parameterExists = true;
                break;
            }
        }

        if (animator == null)
        {
            Debug.LogError($"<color=red>Critical:</color> No Animator found on {gameObject.name}!");
        }
        else 
        {
            Debug.Log($"<color=green>Success:</color> Connected to Animator on {gameObject.name}.");
        }

        if (parameterExists)
        {
            Debug.Log($"<color=green>Confirmed:</color> Parameter '{parameterName}' found in {animator.runtimeAnimatorController.name}");
        }
        else
        {
            Debug.LogError($"<color=red>Error:</color> Parameter '{parameterName}' NOT found! Check for hidden spaces or typos.");
        }

        ResetTimer();
    }

    void Update()
    {
        if (animator == null) return;

        timer += Time.deltaTime;

        if (!isCurrentlyTelepathic)
        {
            // We are waiting to START
            if (timer >= currentThreshold)
            {
                StartTelepathy();
            }
        }
        else
        {
            // We are currently playing, waiting to STOP
            if (timer >= telepathDuration)
            {
                StopTelepathy();
            }
        }
    }

    private void StartTelepathy()
    {
        Debug.Log("<color=cyan>Animation Logic:</color> Triggering Telepathy Animation now.");
        isCurrentlyTelepathic = true;
        timer = 0f;
        animator.SetBool("isTelepathy", true);
    }

    private void StopTelepathy()
    {
        Debug.Log("<color=yellow>Animation Logic:</color> Duration ended. Returning to Idle.");
        isCurrentlyTelepathic = false;
        animator.SetBool("isTelepathy", false);
        ResetTimer();
    }

    private void ResetTimer()
    {
        timer = 0f;
        // Roll a new random time to wait
        currentThreshold = Random.Range(minTime, maxTime);
        if (currentThreshold <= 0) currentThreshold = 0.1f;

        Debug.Log($"<color=white>Timer Reset:</color> Waiting {currentThreshold:F2} seconds until next animation.");
    }
}

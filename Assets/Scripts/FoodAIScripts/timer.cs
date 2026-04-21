using UnityEngine;
using UnityEngine.Events;

public class timer : MonoBehaviour
{
   public UnityEvent timeout;
    public float timeLeft;
    public float timeSet;
    public bool hasTime;

    void Start()
    {
        timeLeft = timeSet;
        hasTime = true;
    }
    public void StartTime()
    {
        hasTime = true;
        if (timeLeft > 0.0f){
            timeLeft -= Time.deltaTime;
            

            if (timeLeft <= 0.0f){
                timeout.Invoke();
                hasTime = false;
            }
        }
    }
    public void ResetTime()
    {
        if (timeLeft <= 0.0f)
        {
            timeLeft += timeSet;
            hasTime = true;
        }
    }
   
}

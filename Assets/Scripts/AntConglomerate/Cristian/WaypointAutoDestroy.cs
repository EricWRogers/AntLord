using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaypointAutoDestroy : MonoBehaviour
{
    [Header("Lifetime")]
    public float lifetime = 7f;

    private void OnEnable()
    {
        CancelInvoke();
        Invoke(nameof(DestroySelf), lifetime);
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}
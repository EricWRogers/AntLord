using UnityEngine;
using System;

public class CollisionDetectorForPrefabs : MonoBehaviour
{
    public Action<Collision> OnHit;

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag != "Tree") {
            OnHit?.Invoke(collision); 
        }
    }    
}

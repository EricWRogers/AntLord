using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTargetFrameRate : MonoBehaviour
{
    public int targetFrameRate = 60;
    
    void Start()
    {
        Application.targetFrameRate = targetFrameRate;
    }

    void OnValidate()
    {
        Application.targetFrameRate = targetFrameRate;
    }
}

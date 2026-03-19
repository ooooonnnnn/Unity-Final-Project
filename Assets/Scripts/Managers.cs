using System;
using UnityEngine;

public class Managers : MonoBehaviour
{
    public static Managers Instance;

    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);
    }
}

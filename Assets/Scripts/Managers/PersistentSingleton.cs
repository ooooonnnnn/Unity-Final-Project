using System;
using UnityEngine;
using System.Collections.Generic;

public class PersistentSingleton : MonoBehaviour
{
    [SerializeField, 
     Tooltip("A newly loaded object with this component will destroy itself if another one with the same id exists")] 
    private int id;
    
    private static Dictionary<int, PersistentSingleton> instances = new Dictionary<int, PersistentSingleton>();

    private void Awake()
    {
        if (!instances.ContainsKey(id))
        {
            instances.Add(id, this);
            DontDestroyOnLoad(gameObject);
            return;
        }
        
        if (instances[id] != this) Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (instances[id] == this) instances.Remove(id);
    }
}

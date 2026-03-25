using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

[Tooltip("A singleton that will be destroyed if an older one with the same name exists")]
public class PersistentSingleton : MonoBehaviour
{
    private static Dictionary<string, PersistentSingleton> singletonInstances = new();
    public static PersistentSingleton Instance => _thisInstance;
    protected static PersistentSingleton _thisInstance;

    protected virtual void Awake()
    {
        print($"Awake called on PersistentSingleton on {gameObject.name}");
        
        var gameObjectName = gameObject.name;
        
        if (!singletonInstances.ContainsKey(gameObjectName))
        {
            singletonInstances.Add(gameObjectName, this);
            DontDestroyOnLoad(gameObject);
            _thisInstance = this;
            return;
        }
        
        if (singletonInstances[gameObjectName] != this) Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (singletonInstances[gameObject.name] == this) singletonInstances.Remove(gameObject.name);
    }
}

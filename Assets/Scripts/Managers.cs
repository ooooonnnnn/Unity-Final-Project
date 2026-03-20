using System;
using UnityEngine;

public class Managers : MonoBehaviour
{
    public static Managers Instance;
    [SerializeField, HideInInspector] private MagicManager magicManager;

    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);
    }
}

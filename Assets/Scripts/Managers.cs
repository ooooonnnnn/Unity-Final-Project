using System;
using UnityEngine;

public class Managers : MonoBehaviour
{
    public static Managers Instance;
    [SerializeField, HideInInspector] private MagicManager magicManager;
    [SerializeField, HideInInspector] private VoiceInputPipeline voiceInputPipeline;
    
    public MagicManager MagicManager => magicManager;

    private void OnValidate()
    {
        magicManager = GetComponentInChildren<MagicManager>();
        voiceInputPipeline = GetComponentInChildren<VoiceInputPipeline>();
        
        voiceInputPipeline.SetInferenceLabels(magicManager.GetInferenceLabels());
    }

    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);
    }
}

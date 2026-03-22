using System;
using UnityEngine;
using Whisper.Utils;

public class ManagersMaster : MonoBehaviour
{
    public static ManagersMaster Instance;
    [SerializeField, HideInInspector] private MagicManager magicManager;
    [SerializeField, HideInInspector] private VoiceInputPipeline voiceInputPipeline;
    [SerializeField, HideInInspector] private MicrophoneRecord recorder;
    
    public MagicManager MagicManager => magicManager;
    public MicrophoneRecord Recorder => recorder;

    private void OnValidate()
    {
        magicManager = GetComponentInChildren<MagicManager>();
        voiceInputPipeline = GetComponentInChildren<VoiceInputPipeline>();
        recorder = GetComponentInChildren<MicrophoneRecord>();
        
        voiceInputPipeline.SetInferenceLabels(magicManager.GetInferenceLabels());
    }

    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);
    }
}

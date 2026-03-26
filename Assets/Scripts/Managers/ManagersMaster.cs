using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Whisper.Utils;

public class ManagersMaster : PersistentSingleton
{
    public new static ManagersMaster Instance => _instance;
    private static ManagersMaster _instance;
    [SerializeField, HideInInspector] private MagicManager magicManager;
    [SerializeField, HideInInspector] private VoiceInputPipeline voiceInputPipeline;
    [SerializeField, HideInInspector] private MicrophoneRecord recorder;
    [SerializeField, HideInInspector] private PlayerInput playerInput;
    
    public MagicManager MagicManager => magicManager;
    public MicrophoneRecord Recorder => recorder;
    public VoiceInputPipeline VoiceInputPipeline => voiceInputPipeline;
    public PlayerInput PlayerInput => playerInput;

    private void OnValidate()
    {
        magicManager = GetComponentInChildren<MagicManager>();
        voiceInputPipeline = GetComponentInChildren<VoiceInputPipeline>();
        recorder = GetComponentInChildren<MicrophoneRecord>();
        playerInput = GetComponentInChildren<PlayerInput>();
        voiceInputPipeline.SetInferenceLabels(magicManager.GetInferenceLabels());
    }

    protected override void Awake()
    {
        base.Awake();
        
        // print($"Awake called on ManagersMaster on {gameObject.name}");
        _instance = this;
    }
}

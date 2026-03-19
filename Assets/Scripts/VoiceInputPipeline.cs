using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Whisper.Utils;

public class VoiceInputPipeline : MonoBehaviour
{
    [SerializeField, HideInInspector] MicrophoneRecord microphoneRecord;
    private AudioChunk lastRecordedChunk;
    private bool isRecording;
    
    private void OnValidate()
    {
        microphoneRecord = GetComponentInChildren<MicrophoneRecord>();
    }

    private void Awake()
    {
        microphoneRecord.OnRecordStop += SaveAudioChunk; 
    }

    private void SaveAudioChunk(AudioChunk chunk)
    {
        lastRecordedChunk = chunk;
    }

    private void Start()
    {
        var pipeline = StartCoroutine(Pipeline());
    }

    private IEnumerator Pipeline()
    {
        while (true)
        {
            //Wait for start record
            while (!isRecording) yield return null;
            //Wait for stop record
            while (isRecording) yield return null;
            print(lastRecordedChunk.Data.Length);
            
            yield return null;
        }
    }

    public void RecordVoiceWithInputAction(InputAction.CallbackContext ctx)
    {
        if (ctx.started) StartRecording();
        
        if (ctx.canceled) StopRecording();
    }

    private void StartRecording()
    {
        microphoneRecord.StartRecord();
        isRecording = true;
    }
    
    private void StopRecording()
    {
        microphoneRecord.StopRecord();
        isRecording = false;
    }
}

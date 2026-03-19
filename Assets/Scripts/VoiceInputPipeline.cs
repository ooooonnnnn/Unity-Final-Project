using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Whisper.Utils;

public class VoiceInputPipeline : MonoBehaviour
{
    [SerializeField, HideInInspector] MicrophoneRecord microphoneRecord;
    [SerializeField, HideInInspector] Transcription transcription;
    private AudioChunk lastRecordedChunk;
    private bool isRecording;
    
    private void OnValidate()
    {
        microphoneRecord = GetComponentInChildren<MicrophoneRecord>();
        transcription = GetComponentInChildren<Transcription>();
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
        //run pipeline endlessly
        while (true)
        {
            //First record voice
            
            //Wait for start record
            while (!isRecording) yield return null;
            //Wait for stop record
            while (isRecording) yield return null;
            print(lastRecordedChunk.Length);
            
            //Transcribe with whisper TODO: than infer with sentis
            var analysisTask = TranscribeAndInfer(lastRecordedChunk);
            //Wait for the task
            while (!analysisTask.IsCompleted) yield return null;
            print(analysisTask.Result);
            
            
            yield return null;
        }
    }

    private async Task<string> TranscribeAndInfer(AudioChunk recordedChunk)
    {
        return await transcription.Transcribe(recordedChunk);
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

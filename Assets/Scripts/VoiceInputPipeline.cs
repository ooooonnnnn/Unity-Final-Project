using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Unity.InferenceEngine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Whisper.Utils;

public class VoiceInputPipeline : MonoBehaviour
{
    [SerializeField, HideInInspector] MicrophoneRecord microphoneRecord;
    [SerializeField, HideInInspector] Transcription transcription;
    [SerializeField, HideInInspector] Inference inference;

    [Tooltip("Called whenever the pipeline is done with the label scores and sentence")] 
    public UnityEvent<float[], string> OnPipelineDone;
    
    private AudioChunk lastRecordedChunk;
    private string lastTranscribedSentence;
    private bool isRecording;
    
    private void OnValidate()
    {
        microphoneRecord = GetComponentInChildren<MicrophoneRecord>();
        transcription = GetComponentInChildren<Transcription>();
        inference = GetComponentInChildren<Inference>();
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
            
            //Transcribe with whisper than infer with sentis
            var analysisTask = TranscribeAndInfer(lastRecordedChunk);
            //Wait for the task
            while (!analysisTask.IsCompleted) yield return null;
            
            //get scores
            var logits = analysisTask.Result;
            float[] scores = new float[logits.shape[0]];
            {
                for (int i = 0; i < logits.shape[0]; i++)
                {
                    float[] falseTrueLogits = { logits[i,0], logits[i,2] };
                    scores[i] = MathHelper.Softmax(falseTrueLogits)[1];
                }
            }
            OnPipelineDone.Invoke(scores, lastTranscribedSentence);
            
            print(string.Join("\n", scores.Select(s => s.ToString("F2"))));
            
            yield return null;
        }
    }

    private async Task<Tensor<float>> TranscribeAndInfer(AudioChunk recordedChunk)
    {
        lastTranscribedSentence = await transcription.Transcribe(recordedChunk);
        var tensor = inference.Infer(lastTranscribedSentence);
        return await tensor.ReadbackAndCloneAsync();
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

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

    [Header("Settings")] [SerializeField] private bool keepMicrophoneOn;
    
    [Tooltip("Called whenever the pipeline is done with the label scores and sentence")] 
    public UnityEvent<ElementType?, SpellShape?, string> OnPipelineDone;
    [Tooltip("Called whenever transcription is done")]
    public UnityEvent<string> OnTranscriptionDone;
    
    private const string InferenceWarmupSentence = "my name is inigo montoya, you killed my father, prepare to die";
    private AudioChunk lastRecordedChunk;
    private string lastTranscribedSentence;
    private bool isRecording;
    private Tensor<float> inferenceResult;
    private Task<Tensor<float>> analysisTask;
    private Coroutine pipelineCoroutine;
    
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

    private void OnDestroy()
    {
        microphoneRecord.OnRecordStop -= SaveAudioChunk; 
        inferenceResult.Dispose();
        analysisTask.Dispose();
    }

    public void SetInferenceLabels(string[] labels)
    {
        inference.labels = labels;
    }

    private void SaveAudioChunk(AudioChunk chunk)
    {
        lastRecordedChunk = chunk;
    }

    private IEnumerator Start()
    {
        yield return WarmUpInference();
        pipelineCoroutine = StartCoroutine(Pipeline());
        if (keepMicrophoneOn) microphoneRecord.StartRecord();
    }

    private IEnumerator WarmUpInference()
    {
        lastTranscribedSentence = InferenceWarmupSentence;
        yield return InferCoroutine();
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
            yield return TranscribeAndInferCor();
            
            //get scores
            inferenceResult?.Dispose();
            inferenceResult = analysisTask.Result;
            float[] scores = new float[inferenceResult.shape[0]];
            {
                for (int i = 0; i < inferenceResult.shape[0]; i++)
                {
                    float[] falseTrueLogits = { inferenceResult[i,0], inferenceResult[i,2] };
                    scores[i] = MathHelper.Softmax(falseTrueLogits)[1];
                }
            }
            
            //Get Best element and type
            Managers.Instance.MagicManager.GetBestFitElementAndType(scores, out ElementType? element, out SpellShape? spellShape);
            OnPipelineDone.Invoke(element, spellShape, lastTranscribedSentence);
            
            print($"Element: {element}, SpellShape: {spellShape}");
            print(string.Join("\n", scores.Select(s => s.ToString("F2"))));
            
            yield return null;
        }
    }

    private IEnumerator TranscribeAndInferCor()
    {
        //Transcribe with whisper than infer with sentis
        analysisTask = TranscribeAndInferAsync(lastRecordedChunk);
        //Wait for the task
        while (!analysisTask.IsCompleted) yield return null;
    }

    private async Task<Tensor<float>> TranscribeAndInferAsync(AudioChunk recordedChunk)
    {
        lastTranscribedSentence = await transcription.Transcribe(recordedChunk);
        OnTranscriptionDone.Invoke(lastTranscribedSentence);
        print("Called event with text: " + lastTranscribedSentence);
        return await InferAsync();
    }

    private async Task<Tensor<float>> InferAsync()
    {
        var tensor = inference.Infer(lastTranscribedSentence);
        return await tensor.ReadbackAndCloneAsync();
    }

    private IEnumerator InferCoroutine()
    {
        var inferenceTask = InferAsync();
        while (!inferenceTask.IsCompleted) yield return null;
    }

    public void RecordVoiceWithInputAction(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            StartRecording();
        }

        if (ctx.canceled)
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        if (keepMicrophoneOn) microphoneRecord.StopRecord();
        microphoneRecord.StartRecord();
        isRecording = true;
    }
    
    private void StopRecording()
    {
        microphoneRecord.StopRecord();
        isRecording = false;
        if (keepMicrophoneOn) microphoneRecord.StartRecord();
    }
}

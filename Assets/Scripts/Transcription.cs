using System.Threading.Tasks;
using UnityEngine;
using Whisper;
using Whisper.Utils;

[RequireComponent(typeof(WhisperManager))]
public class Transcription : MonoBehaviour
{
    [SerializeField, HideInInspector] private WhisperManager whisper;

    private void OnValidate()
    {
        whisper = GetComponent<WhisperManager>();
    }

    public async Task<string> Transcribe(AudioChunk audioChunk)
    {
        var res = await whisper.GetTextAsync(audioChunk.Data, audioChunk.Frequency, audioChunk.Channels);
        return res.Result;
    }
}

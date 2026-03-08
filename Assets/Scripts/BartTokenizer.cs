using System.Collections.Generic;
using System.Linq;
using Unity.InferenceEngine.Tokenization;
using Unity.InferenceEngine.Tokenization.Parsers.HuggingFace;
using UnityEngine;

class BartTokenizer : MonoBehaviour
{
    [SerializeField] private TextAsset tokenizerJson;
    private ITokenizer tokenizer;
    [SerializeField] private string sentence;
    [SerializeField] private string label;

    private void Awake()
    {
        BuildTokenizer();
    }

    [ContextMenu(nameof(BuildTokenizer))]
    private void BuildTokenizer()
    {
        tokenizer = HuggingFaceParser.GetDefault().Parse(tokenizerJson.text);
    }

    [ContextMenu(nameof(TokenizeInput))]
    private void TokenizeInput()
    {
        string input = $"{sentence}</s></s>{label}";
        
        print(string.Join(", ", 
            tokenizer.Encode(input).GetIds().
                Select(i => i.ToString()).ToArray()));
    }
    
    public IReadOnlyList<int> Tokenize(string input)
    {
        return tokenizer.Encode(input).GetIds();
    }
}

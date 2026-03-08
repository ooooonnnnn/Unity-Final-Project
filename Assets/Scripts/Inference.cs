using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.InferenceEngine;
using Unity.InferenceEngine.Tokenization; 

public class Inference : MonoBehaviour
{
    [SerializeField] private ModelAsset modelAsset;
    private Model runtimeModel;
    private Worker worker;
    
    [SerializeField,HideInInspector] private BartTokenizer tokenizer;

    [SerializeField] private string sentence, label;

    private void OnValidate()
    {
        tokenizer = GetComponent<BartTokenizer>();
    }

    [ContextMenu(nameof(BuildModel))]
    private void BuildModel()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
    }

    [ContextMenu(nameof(InferInput))]
    private void InferInput()
    {
        ValidateRuntimeModel();
        
        string premiseAndHypothesis = $"The mage said \"{sentence}\".</s></s>The result was {label}.";
        IReadOnlyList<int> tokenized = tokenizer.Tokenize(premiseAndHypothesis);
        
        Tensor<int> inputIdsTensor = new Tensor<int>(new TensorShape(1, tokenized.Count), tokenized.ToArray());
        int[] mask = tokenized.Select(i => 1).ToArray();
        Tensor<int> attentionMaskTensor = new Tensor<int>(new TensorShape(1, tokenized.Count), mask);

        worker = new Worker(runtimeModel, BackendType.GPUCompute);
        worker.SetInput("input_ids", inputIdsTensor);
        worker.SetInput("attention_mask", attentionMaskTensor);

        worker.Schedule();
        var output = (worker.PeekOutput() as Tensor<float>).DownloadToArray();
        if (output != null)
        {
            print(premiseAndHypothesis);
            
            print("Raw scores (contradiction, neutral, entailment\n" + 
                  string.Join(" ", output.Select(num => num.ToString("F2"))));
            
            print("After softmax:\n" + 
                  string.Join(" ", MathHelper.Softmax(output).Select(num => num.ToString("F2"))));
            
            var noNeutral = output.Where((num, i) => i != 1).ToArray();
            
            print($"Contradiction and entailment scores:\n{noNeutral[0]} {noNeutral[1]}");
            
            print($"Probabilities:\n" +
                  string.Join(" ", MathHelper.Softmax(noNeutral).Select(num => num.ToString("F2"))));
            
        }
        
        worker.Dispose();
        inputIdsTensor.Dispose();
        attentionMaskTensor.Dispose();
    }

    private void ValidateRuntimeModel()
    {
        if (runtimeModel == null)
        {
            BuildModel();
        }
    }

    public void Infer(string sentence)
    {
        this.sentence = sentence;
        InferInput();
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Unity.InferenceEngine;
using UnityEngine.Rendering;

public class Inference : MonoBehaviour
{
    [SerializeField] private ModelAsset modelAsset;
    private Model runtimeModel;
    private Worker worker;
    private Tensor<int> inputIdsTensor;
    private Tensor<int> attentionMaskTensor;
    
    [SerializeField,HideInInspector] private BartTokenizer tokenizer;

    [Header("")]
    [SerializeField] private string inputSentence;
    [SerializeField, 
     Tooltip("The premise is created by inserting the input sentence into this pattern")] private string premisePattern;
    [SerializeField] private string[] labels;
    [SerializeField, 
     Tooltip("The hypothesis is created by inserting each label into this pattern")] private string hypothesisPattern;

    private void OnValidate()
    {
        tokenizer = GetComponent<BartTokenizer>();
    }

    [ContextMenu(nameof(BuildModel))]
    private void BuildModel()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
        print("Built model");
    }

    [ContextMenu(nameof(InferInput))]
    private void InferInput()
    {
        ValidateRuntimeModel();

        inputIdsTensor?.Dispose();
        attentionMaskTensor?.Dispose();
        
        MakeInputAndMask();

        worker ??= new Worker(runtimeModel, BackendType.GPUCompute);
        worker.SetInput("input_ids", inputIdsTensor);
        worker.SetInput("attention_mask", attentionMaskTensor);

        worker.Schedule();
        var output = (worker.PeekOutput() as Tensor<float>)?.DownloadToArray();
        if (output != null)
        {
            for (int i = 0; i < output.Length / 3; i++)
            {
                float[] falseTrueLogits = { output[i * 3], output[i * 3 + 2] };
                float[] probabilities = MathHelper.Softmax(falseTrueLogits);
                print(labels[i] + "\n" +
                      string.Join(" ", probabilities[1].ToString("F2")));
            }
        }
    }

    private void MakeInputAndMask()
    {
        string[] input = labels.Select(label =>
                $"The mage said \"{inputSentence}\".</s></s>The result was {label}.")
            .ToArray();
        
        List<int>[] tokenized = input.Select(text => tokenizer.Tokenize(text).ToList()).ToArray();
        
        int maxLength = tokenized.Max(list => list.Count);

        //pad others and create masks
        int[,] masks = new int[tokenized.Length, maxLength];
        for (int i = 0; i < tokenized.Length; i++)
        {
            int seqLength = tokenized[i].Count;
            for (int j = 0; j < maxLength; j++)
            {
                if (j >= seqLength)
                {
                    tokenized[i].Add(1);
                    masks[i, j] = 0;
                }
                else
                {
                    masks[i, j] = 1;
                }
            }
        }
        
        int[] flattenedTokens = tokenized.SelectMany(list => list).ToArray();
        int[] flattenedMasks = masks.Cast<int>().ToArray();
        
        inputIdsTensor = new Tensor<int>(new TensorShape(tokenized.Length, maxLength), flattenedTokens);
        attentionMaskTensor = new Tensor<int>(new TensorShape(tokenized.Length, maxLength), flattenedMasks);
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
        var sw = new Stopwatch();
        sw.Start();
        
        this.inputSentence = sentence;
        InferInput();
        
        sw.Stop();
        print($"Inference time: {sw.ElapsedMilliseconds} ms.");
    }

    private void OnDestroy()
    {
        worker.Dispose();
        inputIdsTensor.Dispose();
        attentionMaskTensor.Dispose();
    }
}
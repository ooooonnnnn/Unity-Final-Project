using UnityEngine;

public static class MathHelper
{
    public static float[] Softmax(float[] logits)
    {
        float[] exponents = new float[logits.Length];
        float sum = 0;
        for (int i = 0; i < logits.Length; i++)
        {
            exponents[i] = Mathf.Exp(logits[i]);
            sum += exponents[i];
        }
        
        
        float[] probabilities = new float[logits.Length];
        for (int i = 0; i < logits.Length; i++)
        {
            probabilities[i] = exponents[i] / sum;
        }
        
        return probabilities;
    }
}

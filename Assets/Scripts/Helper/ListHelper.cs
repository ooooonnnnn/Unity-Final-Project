using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ListHelper
{
    public static int MaxIndex(this List<float> list, out float maxValue)
    {
        maxValue = float.MinValue;
        var maxIndex = -1;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] > maxValue)
            {
                maxValue = list[i];
                maxIndex = i;
            }
        }
        
        return maxIndex;
    }
    
    public static int MaxIndex(this List<float> list)
    {
        return list.MaxIndex(out _);
    }
}

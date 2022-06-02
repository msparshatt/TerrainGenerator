using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class ArrayHelper 
{
    //convert a 1D float array to a 2D Square height array so it can be applied to a terrain
    static public float[,] ConvertTo2DArray(float[] heightData, int borderSize = 0)
    {
        int size = (int)Mathf.Sqrt(heightData.Length) - borderSize * 2;

        return ConvertTo2DArray(heightData, size, size, borderSize);
    }

    //convert a 1D float array into a 2D rectangular array
    static public float[,] ConvertTo2DArray(float[] heightData, int width, int length, int borderSize = 0)
    {
        int outerWidth = width + borderSize * 2;

        float[,] unityHeights = new float[length, width];

        int index = (outerWidth) * borderSize + borderSize;

        for(int i = 0; i < length; i++) {
            for(int j = 0; j < width; j++) {
                unityHeights[i, j] = heightData[index];
                index++;
            }            
            index += borderSize * 2;
        }

        return unityHeights;
    }

    //convert a 2D array into a 1D array of floats
    static public float[] ConvertTo1DFloatArray(float[,] nmbs)
    {
        float[] result = new float[nmbs.GetLength(0) * nmbs.GetLength(1)];
        int k = 0;

        for (int i = 0; i < nmbs.GetLength(0); i++)
        {
            for (int j = 0; j < nmbs.GetLength(1); j++)
            {
               result[k++] = nmbs[i, j];
            }
        }
        return result;
    }    
}

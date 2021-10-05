using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Erosion
{
    //tweakable parameters
    private float talus; //maximum inclination
    public bool isOn;
    public float factor; //amount to multiply displacement by
    public int iterationCount; //number of iterations

    private Vector2Int[] offsets;

    public Erosion()
    {
        iterationCount = 0;
        isOn = true;
        talus = 0.016f;
        offsets = new Vector2Int[] {new Vector2Int(-1, 0), new Vector2Int(+1, 0), new Vector2Int(0, -1), new Vector2Int(0, +1)};
        factor = 0.75f;
    }

    public float[,] Erode(float[,] heights, int size)
    {
        if(isOn) {
            for(int iteration = 0; iteration < iterationCount; iteration++) {
                float height;
                for(int i = 0; i < size; i++) {
                    for(int j = 0; j < size; j++) {
                        height = heights[i, j];

                        float totalDelta = 0f;
                        float maxDelta = 0f;
                        float[] delta = new float[] {0f, 0f, 0f, 0f};
                        int lowestIndex = 0;

                        for(int k = 0; k < 4; k++) {
                            if((i + offsets[k].x >= 0) && (i + offsets[k].x < size) && (j + offsets[k].y >= 0) && (j + offsets[k].y < size)) {
                                delta[k] = height - heights[i + offsets[k].x, j + offsets[k].y];
                                if(delta[k] > maxDelta) {
                                    maxDelta = delta[k];
                                    lowestIndex = k;
                                }
                            }
                        }

                        if(maxDelta > 0 && maxDelta <= talus) {
                            heights[i,j] -= maxDelta * factor;
                            heights[i + offsets[lowestIndex].x, j + offsets[lowestIndex].y] += maxDelta * factor;
                        }
                    }
                }
            }
        }

        return heights;
    }
}

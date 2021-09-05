using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration
{
    public Vector2 perlinOffset;

    public int size;
    public float scale;
    public bool clampEdges;
    public int iterations;

    public int seed;

    public GeneratorMode mode;

    private float[,] baseHeights;

    public ProceduralGeneration(GeneratorMode _mode, int _size)
    {
        mode = _mode;
        size = _size;

        clampEdges = false;
        iterations = 1;
        seed = 1;
        scale = 1;
        perlinOffset = new Vector2(1,1);
    }

    public float[,] GenerateHeightMap()
    {
        float[,] heights = null;

        heights = Perlin();


        baseHeights = heights;

        return heights;
    }

    public float[,] Perlin()
    {
        float[,] heights = new float[size, size];
        float xcoord = 0f;
        float ycoord = 0f;
        Vector2[] iterationOffsets = new Vector2[iterations];

        Random.InitState(seed);

        for(int i = 0; i < iterations; i++) {
            iterationOffsets[i] = new Vector2(Random.Range(-10000, 10000), Random.Range(-10000, 10000));
        }

        //for ensuring the outer edge has a value of 0.5
        float center = size / 2.0f;

        //used to normalise the results into the range 0.25..0.75
        float minHeight = 1;
        float maxHeight = 0;


        for(int x = 0; x < size; x++) {
            for(int y = 0; y < size; y++) {
                float height = 0f;
                float amplitude = 1;
                float perlinScale = scale /500.0f;

                for(int i = 0; i < iterations; i++) {
                    xcoord = (perlinOffset.x + x + iterationOffsets[i].x) * perlinScale + 0.1f;
                    ycoord = (perlinOffset.y + y + iterationOffsets[i].x) * perlinScale + 0.1f;
                    height += (Mathf.PerlinNoise(xcoord, ycoord) / amplitude);

                    perlinScale *= 2;
                    amplitude *= 2;
                }

                if (height > maxHeight)
                    maxHeight = height;
                else if (height < minHeight)
                    minHeight = height;

                heights[x, y] = height;
            }
        }

        //normalise the heights to be between 0.25 and 0.75
        for(int x = 0; x < size; x++) {
            for(int y = 0; y < size; y++) {
                heights[x, y] = 0.1f + Mathf.InverseLerp(minHeight, maxHeight, heights[x, y]) * 0.8f;

                float edgeClamp = 0f;

                if(clampEdges) {

                    float distance = Mathf.Sqrt(Mathf.Pow((x - center),  2) + Mathf.Pow((y - center), 2));

                    edgeClamp = distance / center;

                    if(edgeClamp > 1) {
                        edgeClamp = 1.0f;
                    }

                    edgeClamp = Mathf.Pow(edgeClamp, 2);

                    heights[x, y] = Mathf.Lerp(heights[x, y], 0.5f, edgeClamp);                    
                }
            }
        }

        //DisplayHeights(heights);
        return heights;
    }

    private void DisplayHeights(float[,] heights)
    {
        string line;
        for(int x = 0; x < size; x++) {
            line = x + ": ";
            for(int y = 0; y < size; y++) {
                line += heights[x, y] + " ";
                //if (heights[x,y] > 0.5)
                //    line += "+";
                //else
                //    line += "-";
            }

            Debug.Log(line);
        }
    }
}

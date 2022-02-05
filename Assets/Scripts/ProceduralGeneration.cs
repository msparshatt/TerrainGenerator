using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Layer
{
    public int terraceCount;
    public float shape;

    public Layer(int _terraceCount, float _shape)
    {
        terraceCount = _terraceCount;
        shape = _shape;
    }
}

public class ProceduralGeneration
{
    public Vector2 perlinOffset;

    public float scale;
    public int iterations;


    public float cellSize;
    public float noiseAmplitude;
    public Vector2 voronoiOffset;

    public float factor;
    public bool clampEdges;
    public bool toggle;

    public float minHeight;
    public float heightscale;

    public ComputeShader proceduralGenerationShader;
    public ComputeShader erosionShader;

    public bool erosionIsOn;
    public float erosionFactor;
    public int erosionIterations;

    private int defaultTerrainResolution;
    private int noCells;

    private float perlinTime;
    private float voronoiTime;
    private float totalTime;

    private int counter = 0;

    private bool shaderRunning;

    private List<Layer> layerList = new List<Layer>();

    public ProceduralGeneration(int _defaultTerrainResolution)
    {
        defaultTerrainResolution = _defaultTerrainResolution;
        cellSize = 400;
        factor = 0.9f;
        noiseAmplitude = 0.01f;
        minHeight = 0f;

        iterations = 1;
        scale = 1;
        perlinOffset = new Vector2(1,1);

        noCells = 100;
        shaderRunning = false;
    }

    public void AddLayer(int _terraceCount, float _shape)
    {
        layerList.Add(new Layer(_terraceCount, _shape));
    }

    public void ClearLayers()
    {
        layerList.Clear();
    }

    public float[] GenerateHeightMap(int size)
    {
        if(shaderRunning)
            return null;

        shaderRunning = true;

        ComputeBuffer heightBuffer = new ComputeBuffer(size * size, sizeof(float));
        int kernelHandle = proceduralGenerationShader.FindKernel("GenerateTerrain");

        proceduralGenerationShader.SetFloat("PerlinScale", scale);
        proceduralGenerationShader.SetFloat("PerlinXOffset", perlinOffset.x);
        proceduralGenerationShader.SetFloat("PerlinYOffset", perlinOffset.y);
        proceduralGenerationShader.SetInt("PerlinIterations", iterations);
        proceduralGenerationShader.SetInt("Resolution", size);

        proceduralGenerationShader.SetFloat("VoronoiXOffset", voronoiOffset.x);
        proceduralGenerationShader.SetFloat("VoronoiYOffset", voronoiOffset.y);
        proceduralGenerationShader.SetInt("NoCells", noCells);
        proceduralGenerationShader.SetFloat("CellSize", cellSize);

        proceduralGenerationShader.SetFloat("Factor", factor);

        proceduralGenerationShader.SetFloat("MinHeight", minHeight);
        proceduralGenerationShader.SetFloat("HeightScale", heightscale);
        proceduralGenerationShader.SetBool("ClampEdges", clampEdges);

        float[] terraceParameters = {-1, -1, 0, 0
        -1, -1, 0, 0,
        -1, -1, 0, 0};

        if(layerList.Count != 0) {
            int count = 0;
            foreach (Layer layer in layerList) {
                terraceParameters[4 * count] = layer.terraceCount;
                terraceParameters[4 * count + 1] = layer.shape;
                count++;
            }
        }
        proceduralGenerationShader.SetFloats("TerraceParameters", terraceParameters);

        proceduralGenerationShader.SetBuffer(kernelHandle, "Heights", heightBuffer);
        int groups = Mathf.CeilToInt(size * size / 64f);
		proceduralGenerationShader.Dispatch(0, groups, 1, 1);

        float[] data = new float[heightBuffer.count];
        heightBuffer.GetData(data);

        heightBuffer.Release();
        heightBuffer = null;

        shaderRunning = false;
        return data;
    }

    public float[] Erosion(float[] heights, int size)
    {
        Debug.Log("Eroding");

        ComputeBuffer heightBuffer = new ComputeBuffer(size * size, sizeof(float));
        heightBuffer.SetData(heights);

        int kernelHandle = erosionShader.FindKernel("Erode");
        erosionShader.SetInt("IterationCount", erosionIterations);
        erosionShader.SetInt("Resolution", size);
        erosionShader.SetFloat("Factor", erosionFactor);

        erosionShader.SetBuffer(kernelHandle, "Heights", heightBuffer);
        int groups = Mathf.CeilToInt(size * size / 64f);
        erosionShader.Dispatch(kernelHandle, groups, 1, 1);

        float[] data  = new float[heightBuffer.count];
        heightBuffer.GetData(data);

        heightBuffer.Release();
        heightBuffer = null;

        return data;
    }
}

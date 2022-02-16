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
    public float iterationFactor;


    public float cellSize;
    public float noiseAmplitude;
    public Vector2 voronoiOffset;
    public float voronoiValleys;

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

    public int lifetime;
    public float startSpeed;
    public float inertia;
    public  int seed = 257;
    public float startWater;
    public float gravity = 10;

    public float sedimentCapaFactor;
    public float depSpeed;
    public float eroSpeed;
    public float evaporateSpeed;

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
        proceduralGenerationShader.SetFloat("PerlinIterationFactor", iterationFactor);

        proceduralGenerationShader.SetInt("Resolution", size);

        proceduralGenerationShader.SetFloat("VoronoiXOffset", voronoiOffset.x);
        proceduralGenerationShader.SetFloat("VoronoiYOffset", voronoiOffset.y);
        proceduralGenerationShader.SetFloat("CellSize", cellSize);
        proceduralGenerationShader.SetFloat("VoronoiValleys", voronoiValleys);

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

    public float[] Erosion(float[] mapArray, int size)
    {
        int numIter = erosionIterations * 10000;
        Vector2[] StartPos = new Vector2[numIter];
        int seed = 578945;

        Random.InitState(seed);
        for (int i = 0; i < numIter; i++)
        {
            StartPos[i].x = Random.Range(0,size); 
            StartPos[i].y = Random.Range(0,size); 
        }

        int kernelHandle = erosionShader.FindKernel("Erosion");
        ComputeBuffer heightMapBuffer = new ComputeBuffer(mapArray.Length, sizeof(float));
        heightMapBuffer.SetData(mapArray);
        erosionShader.SetBuffer(kernelHandle, "map", heightMapBuffer);

        ComputeBuffer startPosBuffer = new ComputeBuffer(numIter, 2*sizeof(float));
        startPosBuffer.SetData(StartPos);
        erosionShader.SetBuffer(kernelHandle, "startPos", startPosBuffer);

        int brushSize = 5;
        Vector3[] brush = new Vector3[brushSize * brushSize * 4];
        float normalizeC = 0;
        for(int i=0;i< brushSize;i++)
        {
            for(int j=0;j<brushSize;j++)
            {
                normalizeC += 1.1f * (i + j + 1);
                brush[i * brushSize * 2 + j].z = (i + j + 1);
                brush[(2 * brushSize - 1 - i) * brushSize * 2 + j].z = (i + j + 1);
                brush[(2 * brushSize - 1 - i) * brushSize * 2 + (2 * brushSize - 1 - j)].z = (i + j + 1);
                brush[i * brushSize * 2 + (2 * brushSize - 1 - j)].z = (i + j + 1);
            }
        }
        for (int i = 0; i < brushSize*2; i++)
        {
            for (int j = 0; j < brushSize*2; j++)
            {
                brush[i * brushSize * 2 + j].z /= normalizeC;
                brush[i * brushSize * 2 + j].x = i - (brushSize - 1);
                brush[i * brushSize * 2 + j].y = j - (brushSize - 1);
            }
        }

        ComputeBuffer brushBuffer = new ComputeBuffer(brush.Length, 3 * sizeof(float));
        brushBuffer.SetData(brush);
        erosionShader.SetBuffer(kernelHandle, "brush", brushBuffer);

        erosionShader.SetInt("brushSize", brushSize);
        erosionShader.SetInt("size", size);
        erosionShader.SetInt("lifetime", lifetime);
        erosionShader.SetFloat("startSpeed", startSpeed);
        erosionShader.SetFloat("inertia", inertia);
        erosionShader.SetFloat("startWater", startWater);
        erosionShader.SetFloat("sedimentCapaFactor", sedimentCapaFactor);
        erosionShader.SetFloat("depositSpeed", depSpeed);
        erosionShader.SetFloat("erosionSpeed", eroSpeed);
        erosionShader.SetFloat("gravity", gravity);
        erosionShader.SetFloat("evaporateSpeed", evaporateSpeed);

        erosionShader.Dispatch(kernelHandle, numIter/1000, 1, 1);

        heightMapBuffer.GetData(mapArray);
        heightMapBuffer.Release();
        startPosBuffer.Release();
        brushBuffer.Release();

        return mapArray;
    }
}

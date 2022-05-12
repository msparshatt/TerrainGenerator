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
    public float iterationRotation;


    public float cellSize;
    public float noiseAmplitude;
    public Vector2 voronoiOffset;
    public float voronoiValleys;

    public float factor;
    public bool clampEdges;
    public float clampHeight;
    public bool toggle;

    public float minHeight;
    public float heightscale;

    public ComputeShader proceduralGenerationShader;
    public ComputeShader erosionShader;

    public bool erosionIsOn;
    public int erosionIterations;

    public int lifetime;
    public float startSpeed;
    public float inertia;
    public  int seed = 257;
    public float startWater;
    public float gravity = 10;

    public float sedimentCapacityFactor;
    public float depositSpeed;
    public float erodeSpeed;
    public float evaporateSpeed;

    private int defaultTerrainResolution;
    private int noCells;

    private float perlinTime;
    private float voronoiTime;
    private float totalTime;

    private int counter = 0;

    private bool shaderRunning;

    private int erosionBrushRadius = 3;

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

        int resolution = size + erosionBrushRadius * 2;

        ComputeBuffer heightBuffer = new ComputeBuffer(resolution * resolution, sizeof(float));
        int kernelHandle = proceduralGenerationShader.FindKernel("GenerateTerrain");

        proceduralGenerationShader.SetFloat("PerlinScale", scale);
        proceduralGenerationShader.SetFloat("PerlinXOffset", perlinOffset.x);
        proceduralGenerationShader.SetFloat("PerlinYOffset", perlinOffset.y);
        proceduralGenerationShader.SetInt("PerlinIterations", iterations);
        proceduralGenerationShader.SetFloat("PerlinIterationFactor", iterationFactor);
        proceduralGenerationShader.SetFloat("PerlinIterationRotation", iterationRotation);

        proceduralGenerationShader.SetInt("Resolution", resolution);

        proceduralGenerationShader.SetFloat("VoronoiXOffset", voronoiOffset.x);
        proceduralGenerationShader.SetFloat("VoronoiYOffset", voronoiOffset.y);
        proceduralGenerationShader.SetFloat("CellSize", cellSize);
        proceduralGenerationShader.SetFloat("VoronoiValleys", voronoiValleys);

        proceduralGenerationShader.SetFloat("Factor", factor);

        proceduralGenerationShader.SetFloat("MinHeight", minHeight);
        proceduralGenerationShader.SetFloat("HeightScale", heightscale);
        proceduralGenerationShader.SetBool("ClampEdges", clampEdges);
        proceduralGenerationShader.SetFloat("ClampHeight", clampHeight);

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
        int groups = Mathf.CeilToInt(resolution * resolution / 64f);
		proceduralGenerationShader.Dispatch(0, groups, 1, 1);

        float[] data = new float[heightBuffer.count];
        heightBuffer.GetData(data);

        heightBuffer.Release();
        heightBuffer = null;

        shaderRunning = false;
        return data;
    }

    public float[] Erosion (float[] map, int mapSize) 
    {
        //int erosionBrushRadius = 5;
        float minSedimentCapacity = 0.01f;

        int numThreads = erosionIterations / 1024;

        // Create brush
        List<int> brushIndexOffsets = new List<int> ();
        List<float> brushWeights = new List<float> ();

        float weightSum = 0;
        for (int brushY = -erosionBrushRadius; brushY <= erosionBrushRadius; brushY++) {
            for (int brushX = -erosionBrushRadius; brushX <= erosionBrushRadius; brushX++) {
                float sqrDst = brushX * brushX + brushY * brushY;
                if (sqrDst < erosionBrushRadius * erosionBrushRadius) {
                    brushIndexOffsets.Add (brushY * mapSize + brushX);
                    float brushWeight = 1 - Mathf.Sqrt (sqrDst) / erosionBrushRadius;
                    weightSum += brushWeight;
                    brushWeights.Add (brushWeight);
                }
            }
        }
        for (int i = 0; i < brushWeights.Count; i++) {
            brushWeights[i] /= weightSum;
        }

        // Send brush data to compute shader
        ComputeBuffer brushIndexBuffer = new ComputeBuffer (brushIndexOffsets.Count, sizeof (int));
        ComputeBuffer brushWeightBuffer = new ComputeBuffer (brushWeights.Count, sizeof (int));
        brushIndexBuffer.SetData (brushIndexOffsets);
        brushWeightBuffer.SetData (brushWeights);
        erosionShader.SetBuffer (0, "brushIndices", brushIndexBuffer);
        erosionShader.SetBuffer (0, "brushWeights", brushWeightBuffer);

        // Generate random indices for droplet placement
        int[] randomIndices = new int[erosionIterations];
        for (int i = 0; i < erosionIterations; i++) {
            int randomX = Random.Range (erosionBrushRadius, mapSize + erosionBrushRadius);
            int randomY = Random.Range (erosionBrushRadius, mapSize + erosionBrushRadius);
            randomIndices[i] = randomY * mapSize + randomX;
        }

        // Send random indices to compute shader
        ComputeBuffer randomIndexBuffer = new ComputeBuffer (randomIndices.Length, sizeof (int));
        randomIndexBuffer.SetData (randomIndices);
        erosionShader.SetBuffer (0, "randomIndices", randomIndexBuffer);

        // Heightmap buffer
        ComputeBuffer mapBuffer = new ComputeBuffer (map.Length, sizeof (float));
        mapBuffer.SetData (map);
        erosionShader.SetBuffer (0, "map", mapBuffer);

        // Settings
        erosionShader.SetInt ("borderSize", erosionBrushRadius);
        erosionShader.SetInt ("mapWidth", mapSize + erosionBrushRadius * 2);
        erosionShader.SetInt ("mapLength", mapSize + erosionBrushRadius * 2);
        erosionShader.SetInt ("brushLength", brushIndexOffsets.Count);
        erosionShader.SetInt ("maxLifetime", lifetime);
        erosionShader.SetFloat ("inertia", inertia);
        erosionShader.SetFloat ("sedimentCapacityFactor", sedimentCapacityFactor);
        erosionShader.SetFloat ("minSedimentCapacity", minSedimentCapacity);
        erosionShader.SetFloat ("depositSpeed", depositSpeed);
        erosionShader.SetFloat ("erodeSpeed", erodeSpeed);
        erosionShader.SetFloat ("evaporateSpeed", evaporateSpeed);
        erosionShader.SetFloat ("gravity", gravity);
        erosionShader.SetFloat ("startSpeed", startSpeed);
        erosionShader.SetFloat ("startWater", startWater);

        // Run compute shader
        erosionShader.Dispatch (0, numThreads, 1, 1);
        mapBuffer.GetData (map);

        // Release buffers
        mapBuffer.Release ();
        randomIndexBuffer.Release ();
        brushIndexBuffer.Release ();
        brushWeightBuffer.Release ();

        return map;
    }

}

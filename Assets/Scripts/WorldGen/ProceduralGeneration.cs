using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private int defaultTerrainResolution;
    private int noCells;
    private Vector2[,] voronoiCells;

    private float perlinTime;
    private float voronoiTime;
    private float totalTime;

    private int counter = 0;

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
        voronoiCells = new Vector2[noCells, noCells];
        InitialiseCells(noCells);
    }

    public float[,] GenerateHeightMap(int size, int multiplier = 1)
    {
        perlinTime = 0;
        voronoiTime = 0;
        totalTime = 0;

        float start = 0f;

        float[,] heights = new float[size, size];
        float center = size / 2.0f;

        float edgeClamp = 0f;
        float height = 0f;
/*
        float minHeight = 10f;
        float maxHeight = 0f;*/

        for(int x = 0; x < size; x++) {
            for(int y = 0; y < size; y++) {
                start = Time.realtimeSinceStartup;

                int xcoord = x * multiplier;
                int ycoord = y * multiplier;
                height = (VoronoiNoise(xcoord, ycoord) * (1 - factor) +  PerlinNoise(xcoord, ycoord) *  factor) * 0.8f;

                if(clampEdges) {

                    float xDistance = x - center;
                    float yDistance = y - center;
                    float distance = xDistance * xDistance + yDistance * yDistance;

                    edgeClamp = distance / (center * center);

                    if(edgeClamp > 1) {
                        edgeClamp = 1.0f;
                    }

                    height = Mathf.Lerp(height, 0.5f, edgeClamp);                    

                }

                if(height < minHeight + 0.05f) {
                    if(height < minHeight - 0.05f) {
                        height = minHeight;                        
                    } else {
                        height = Mathf.Lerp(minHeight, minHeight + 0.05f, (height + 0.05f - minHeight) * 10);
                    }
                }

                heights[x, y] = height;

                totalTime += Time.realtimeSinceStartup - start;
            }
        }

        Debug.Log("  perlin = " + perlinTime);
        Debug.Log("  voronoi = " + voronoiTime);
        Debug.Log("  total = " + totalTime);

//        Debug.Log(minHeight + ":" + maxHeight);
        return heights;
    }

    private void InitialiseCells(int noCells)
    {
        for(int i = 0; i < noCells; i++) {
            for(int j = 0; j < noCells; j++) {
                voronoiCells[i, j] = new Vector2(i,j) + Rand2D(i, j);
            }
        }
    }

    public float PerlinNoise(float x, float y)
    {
        float start = Time.realtimeSinceStartup;
        float height = 0f;
        float amplitude = 1;
        float perlinScale = scale / 500.0f;
        float xcoord = (perlinOffset.x + x);
        float ycoord = (perlinOffset.y + y);

        for(int i = 0; i < iterations; i++) {
            height += (Mathf.PerlinNoise(xcoord * perlinScale + 0.1f, ycoord * perlinScale + 0.1f) / amplitude);

            perlinScale *= 2;
            amplitude *= 2;
        }

        perlinTime += Time.realtimeSinceStartup - start;
        return height;
    }

    private float VoronoiNoise(float xPos, float yPos)
    {
        float start = Time.realtimeSinceStartup;
        float xcoord = Mathf.Abs((xPos + voronoiOffset.x) % (noCells * cellSize));
        float ycoord = Mathf.Abs((yPos + voronoiOffset.y) % (noCells * cellSize));
        Vector2 pos = new Vector2(xcoord / cellSize, ycoord / cellSize);

        pos.x += noiseAmplitude * Random.value;
        pos.y += noiseAmplitude * Random.value;

        Vector2Int baseCell = Vector2Int.FloorToInt(pos);

        float minDistance = 10f;
        float minDistance2 = 11f;

        for(int x=-1; x<=1; x++) {
            for(int y=-1; y<=1; y++) {
                Vector2Int cell = baseCell + new Vector2Int(x, y);

                //don't try to read outside the bounds of the array
                if(cell.x < 0 || cell.x >= voronoiCells.GetLength(0)) {
                    continue;
                }
                if(cell.y < 0 || cell.y >= voronoiCells.GetLength(1)) {
                    continue;
                }

                float distToCell = Vector2.Distance(voronoiCells[cell.x, cell.y], pos);

                if(distToCell < minDistance){
                    minDistance2 = minDistance;
                    minDistance = distToCell;
                } else if(distToCell < minDistance2) {
                    minDistance2 = distToCell;
                }   
            }
        }


        voronoiTime += Time.realtimeSinceStartup - start;

        float result = 0;
//        if(toggle) {
            result = minDistance * -1 + minDistance2;
//        } else {
//            result = minDistance;
//        }

        return result;
    }

    private Vector2 Rand2D(float xPos, float yPos)
    {
        Vector2 smallValue = new Vector2(Mathf.Sin(xPos), Mathf.Sin(yPos));

        Vector2 dotDir = new Vector2(12.989f, 78.233f);
        float random = Vector2.Dot(smallValue, dotDir);

        float x = Mathf.Sin(random) * 143758.5453f;
        x -= Mathf.Floor(x);

        dotDir = new Vector2(39.346f, 11.135f);
        random = Vector2.Dot(smallValue, dotDir);

        float y = Mathf.Sin(random) * 143758.5453f;
        y -= Mathf.Floor(y);

        return new Vector2(x, y);
    }
}

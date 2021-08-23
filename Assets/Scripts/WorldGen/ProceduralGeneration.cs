using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldKit.api.procedural.Builders;
using WorldKit.api.procedural.Layers;
using WorldKit.api.procedural.Utils;
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


    public void SetupHeightmap(HeightMapBuilder height)
    {
        // <param name="frequency">How frequent the noise should be. Higher means smaller shapes and more noise</param>
        // <param name="octaves">The amount of octaves in the perlin noise layer. Minimal input is 1</param>
        // <param name="octavesStrength">How much of the octaves should shine through</param>
        // <param name="offset">Offset the noise position</param>
        height.AddLayer(new PerlinNoise(PerlinNoise.PerlinType.Standard, scale, iterations, 0.5f, perlinOffset));
    }
}

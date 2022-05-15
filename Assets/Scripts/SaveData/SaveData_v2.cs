using UnityEngine;

public struct SaveData_v2
{
    public int version;
    public int terrainResolution;
    public byte[] heightmap;

    public int[] baseTexture;
    public byte[][] baseTexture_colors;
    public int[] mixType;
    public float[] mixFactor;
    public float[] mixOffset;

    public float tiling;
    public bool aoActive;
    public byte[] overlayTexture;
    public float paintTiling;

    //sky panel
    public bool lightTerrain;
    public float sunHeight;
    public float sunDirection;
    public bool automaticColor;
    public Color sunColor;
    public bool cloudActive;
    public float cloudXoffset;
    public float cloudYOffset;
    public float cloudScale;
    public float cloudStart;
    public float cloudEnd;
    public float windDirection;
    public float windSpeed;
    public float cloudIterations;

    //water panel
    public bool oceanActive;
    public float oceanHeight;
    public float waveDirection;
    public float waveSpeed;
    public float waveHeight;
    public float waveChoppyness;
    public float foamAmount;
    public bool shoreLineActive;
    public float shorelineFoamAmount;

} 

public struct Version
{
    public int version;
}
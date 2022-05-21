using UnityEngine;
using System.Collections;
using System.Collections.Generic;


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
    public Color[] colors;
    public bool[] useTexture;

    public float tiling;
    public bool aoActive;
    public byte[] overlayTexture;
    public float paintTiling;

    public List<string> panelData;
} 

public struct Version
{
    public int version;
}
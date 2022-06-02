using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MaterialSaveData_v1
{
    public bool ambientOcclusion;
    public int[] currentMaterialIndices;
    public float materialScale;
    public bool[] useTexture;
    public int[] mixTypes;
    public float[] mixFactors;
    public float[] mixOffsets;
    public Color[] colors;
}
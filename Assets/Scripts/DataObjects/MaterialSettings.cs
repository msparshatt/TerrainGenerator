using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialSettings", menuName = "material settings", order = 1)]
public class MaterialSettings : ScriptableObject
{
    public const int NUMBER_MATERIALS = 5;
    public bool ambientOcclusion;
    public int[] currentMaterialIndices;
    public float materialScale;
    public bool[] useTexture;
    public int[] mixTypes;
    public float[] mixFactors;
    public float[] mixOffsets;
    public Color[] colors;
}
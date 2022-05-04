using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InternalData", menuName = "internal data", order = 1)]
public class InternalDataScriptable : ScriptableObject
{
    public enum Modes {System, Materials, Sculpt, Paint, Stamp, Sky, Water, Erode};
    public const int NUMBER_MATERIALS = 5;

    public bool sliderChanged = false;  //used to update the base textures after one of the sliders has changed value
    public bool ProcGenOpen = false;    //is the procedural generation panel open
    public bool unsavedChanges = false; //Record if there have been any changes
    public bool detectMaximaAndMinima = false; //Do we need to check for maxima and minima

    public Modes mode;

    //imported data
    public List<string> customMaterials;
    public List<string> customPaintBrushes;
    public List<string> customSculptBrushes;
    public List<string> customStampBrushes;
    public List<string> customTextures;



    //material panel
    public bool ambientOcclusion;
    public int[] currentMaterialIndices;
    public float materialScale;

    public int[] mixTypes;
    public float[] mixFactors;

    //sculpt panel

    //stamp panel

    //paint panel
    public float paintScale;

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

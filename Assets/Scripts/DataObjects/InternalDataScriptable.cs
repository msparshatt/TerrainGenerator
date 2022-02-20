using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InternalData", menuName = "internal data", order = 1)]
public class InternalDataScriptable : ScriptableObject
{
    public enum Modes {System, Materials, Sculpt, Paint};

    public bool sliderChanged = false;  //used to update the base textures after one of the sliders has changed value
    public bool ProcGenOpen = false;    //is the procedural generation panel open
    public bool unsavedChanges = false; //Record if there have been any changes
    public bool detectMaximaAndMinima = false; //Do we need to check for maxima and minima

    public bool ambientOcclusion;
    public Modes mode;

    public List<string> customMaterials;
    public List<string> customPaintBrushes;
    public List<string> customSculptBrushes;
    public List<string> customTextures;

    public float materialScale;
    public float paintScale;

    public int[] currentMaterialIndices;

}

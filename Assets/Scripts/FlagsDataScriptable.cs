using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FlagsData", menuName = "flags data", order = 1)]
public class FlagsDataScriptable : ScriptableObject
{
    public bool sliderChanged = false;  //used to update the base textures after one of the sliders has changed value
    public bool ProcGenOpen = false;    //is the procedural generation panel open
    public bool unsavedChanges = false; //Record if there have been any changes
}

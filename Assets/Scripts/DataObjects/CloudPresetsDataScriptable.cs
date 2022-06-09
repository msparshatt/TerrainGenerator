using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CloudPresets", menuName = "cloud presets", order = 1)]
public class CloudPresetsDataScriptable : ScriptableObject
{
    public string presetName;
    public float cloudStart;
    public float cloudEnd;
    public float cloudBrightness;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SettingsData", menuName = "settings data", order = 1)]
public class SettingsDataScriptable : ScriptableObject
{
    public float movementSpeed = 40.0f;
    public float cameraSensitivity = 1.0f;
}
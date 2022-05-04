using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Erosion Data", menuName = "erosion data", order = 1)]
public class ErosionDataScriptable : ScriptableObject
{
    public int erosionBrushRadius = 5;
    public int lifetime = 30;
    public float sedimentCapacityFactor = 9f;
    public float minSedimentCapacity = 0.01f;
    public float inertia = 0.3f;
    public float depositSpeed = 0.5f;
    public float erodeSpeed = 0.5f;
    public float startSpeed = 1f;
    public float evaporateSpeed = 0.01f;
    public float startWater = 1f;
    public float gravity = 10f;
}

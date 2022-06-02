using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErosionSaveData_v1: BrushSaveData_v1
{
    public int erosionBrushRadius;
    public int lifetime;
    public float sedimentCapacityFactor;
    public float minSedimentCapacity;
    public float inertia;
    public float depositSpeed;
    public float erodeSpeed;
    public float startSpeed;
    public float evaporateSpeed;
    public float startWater;
    public float gravity;
}
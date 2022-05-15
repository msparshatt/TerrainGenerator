using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SkySaveData_v1
{
    //light settings
    public bool lightTerrain;
    public float sunHeight;
    public float sunDirection;
    public bool automaticColor;
    public Color sunColor;

    //cloud settings;
    public bool cloudActive;
    public float cloudXoffset;
    public float cloudYOffset;
    public float cloudScale;
    public float cloudStart;
    public float cloudEnd;
    public float windDirection;
    public float windSpeed;
    public float cloudIterations;
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public struct SaveData_v3
{
    public int version;
    public int terrainResolution;
    public byte[] heightmap;

    public byte[] overlayTexture;

    public string cameraData;
    public List<string> panelData;
} 

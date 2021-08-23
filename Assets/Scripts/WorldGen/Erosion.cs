using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldKit.api.procedural.Builders;
using WorldKit.api.procedural.Layers;
using WorldKit.api.procedural.Utils;
public class Erosion 
{
    public bool isOn;

    public Erosion()
    {
        isOn = false;
    }

    public void SetupErosion(HeightMapBuilder height, int count)
    {
        height.AddLayer(new HydraulicErosion(count, 120, 0.03f, 6f, 0f, 0.3f, 0.02f, 0.3f));
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldKit.api.procedural.Builders;
using WorldKit.api.procedural.Layers;
using WorldKit.api.procedural.Utils;
public class Layer
{
    public int terraceCount;
    public float shape;
    public bool smooth;

    public Layer(int _terraceCount, float _shape, bool _smooth)
    {
        terraceCount = _terraceCount;
        shape = _shape;
        smooth = _smooth;
    }
}

public class TerraceSettings 
{
    private List<Layer> layerList = new List<Layer>();

    public void AddLayer(int _terraceCount, float _shape, bool _smooth)
    {
        layerList.Add(new Layer(_terraceCount, _shape, _smooth));
    }

    public void ClearLayers()
    {
        layerList.Clear();
    }
    
    public void SetupTerraces(HeightMapBuilder height)
    {
        foreach(Layer l in layerList) {
            height.AddLayer(new Terrace(l.terraceCount, l.shape, l.smooth));
        }
    }
}

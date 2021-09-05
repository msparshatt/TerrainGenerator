using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    
    public float[,] AddTerraces(float [,] heights, int size)
    {
        Debug.Log("Adding terraces");
        if(layerList.Count != 0) {

            foreach (Layer layer in layerList) {

                for(int x = 0; x < size; x++) {
                    for(int y = 0; y < size; y++) {
                        float terraceHeight = heights[x,y] * layer.terraceCount;

                        int floor = Mathf.FloorToInt(terraceHeight);
                        float difference = terraceHeight - floor;

                        float shape = layer.shape;

                        float newDifference = Sigmoid(shape, difference);

                        if(layer.smooth) {
                            newDifference = (newDifference + difference) / 2;
                        }

                        float minHeight = (float)(floor) / layer.terraceCount;
                        float maxHeight = (float)(floor + 1) / layer.terraceCount;

                        heights[x, y] = Mathf.Lerp(minHeight, maxHeight, newDifference);
                    }
                }
            }

        }

        return heights;
    }

    private float Sigmoid(float k, float t)
    {
        return (k * t) / (1 + k - t);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager 
{
    private static TerrainManager _instance;
    private GameObject terrainObject;

    static public TerrainManager Instance()
    {
        if(_instance == null)
            _instance = new TerrainManager();

        return _instance;
    }

    public GameObject TerrainObject
    {
        get {return terrainObject;}
        set 
        {
            if(value.GetComponent<Terrain>() == null) {
                Debug.LogError("Object does not have a terrain component");
            } else {
                terrainObject = value;
            }
        }
    }

    public Terrain Terrain
    {
        get {return terrainObject.GetComponent<Terrain>();}
    }

    public TerrainData TerrainData
    {
        get {return terrainObject.GetComponent<Terrain>().terrainData;}
    }

    public HeightmapController HeightmapController
    {
        get {return terrainObject.GetComponent<HeightmapController>();}
    }

    public MaterialController MaterialController
    {
        get {return terrainObject.GetComponent<MaterialController>();}
    }

    public TerrainPainter TerrainPainter
    {
        get {return terrainObject.GetComponent<TerrainPainter>();}
    }

    public TerrainModifier TerrainModifier
    {
        get {return terrainObject.GetComponent<TerrainModifier>();}
    }
}

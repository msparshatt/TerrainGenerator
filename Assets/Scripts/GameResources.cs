using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


public class GameResources
{
    static private GameResources _instance;

    //material settings
    public List<Material> materials;
    public List<Texture2D> textures;
    public List<Texture2D> icons;
    
    public List<Texture2D> brushes;
    public List<Texture2D> paintBrushes;
    public List<Texture2D> stampBrushes;
    public List<Texture2D> erosionBrushes;

    static public GameResources instance {
        get {
            if(_instance == null)
                _instance = new GameResources();

            return _instance;
        }
    }
    // Start is called before the first frame update
    public GameResources()
    {
        LoadResources();
    }

    private void LoadResources()
    {
        Debug.Log("loading sculpt brushes " + Time.realtimeSinceStartup);
        brushes = new List<Texture2D>(Resources.LoadAll<Texture2D>("TerrainBrushes/Sculpt"));
        Debug.Log(brushes.Count);
        Debug.Log("loading paint brushes " + Time.realtimeSinceStartup);
        paintBrushes = new List<Texture2D>(Resources.LoadAll<Texture2D>("PaintBrushes"));
        Debug.Log("loading stamp brushes " + Time.realtimeSinceStartup);
        stampBrushes = new List<Texture2D>(Resources.LoadAll<Texture2D>("TerrainBrushes/Stamp"));
        Debug.Log("loading erosion brushes " + Time.realtimeSinceStartup);
        erosionBrushes = new List<Texture2D>(Resources.LoadAll<Texture2D>("TerrainBrushes/Erosion"));

        Debug.Log("loading materials " + Time.realtimeSinceStartup);
        Material[] loadedMaterials = Resources.LoadAll<Material>("Materials");

        materials = new List<Material>();
        textures = new List<Texture2D>();

        Vector2 scale = new Vector2(1.0f, 1.0f);
        foreach(Material mat in loadedMaterials) {
            mat.mainTextureScale = scale;
            materials.Add(new Material(mat));
            textures.Add((Texture2D)mat.mainTexture);
        }

        Debug.Log("loading material icons " + Time.realtimeSinceStartup);
        icons = new List<Texture2D>(Resources.LoadAll<Texture2D>("Icons"));        
    }

}

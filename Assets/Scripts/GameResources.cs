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
    
    //public Texture2D[] brushes;
    public List<Texture2D> brushes;

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
        Debug.Log("loading brushes " + Time.realtimeSinceStartup);
        brushes = new List<Texture2D>(Resources.LoadAll<Texture2D>("Brushes"));
        Debug.Log("loading materials " + Time.realtimeSinceStartup);
        Material[] loadedMaterials = Resources.LoadAll<Material>("Materials");

        materials = new List<Material>();
        textures = new List<Texture2D>();

        foreach(Material mat in loadedMaterials) {
            materials.Add(new Material(mat));
            textures.Add((Texture2D)mat.mainTexture);
        }

        Debug.Log("loading material icons " + Time.realtimeSinceStartup);
        icons = new List<Texture2D>(Resources.LoadAll<Texture2D>("Icons"));        

        Vector2 scale = new Vector2(1.0f, 1.0f);

        //Debug.Log("creating overlay textures " + Time.realtimeSinceStartup);
        //create the overlay texture and reset tiling values for each material
        foreach (Material mat in materials)
        {
            int width = mat.mainTexture.width;
            int height = mat.mainTexture.height;
            mat.mainTextureScale = scale;
            mat.SetTextureScale("_OverlayTexture", scale);
        }
    }

}

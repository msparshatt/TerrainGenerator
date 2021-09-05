using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


public class GameResources
{
    static private GameResources _instance;

    //material settings
    public Material[] materials;
    public Texture2D[] icons;
    
    public Texture2D[] brushes;

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
        brushes = Resources.LoadAll<Texture2D>("Brushes");
        Debug.Log("loading materials " + Time.realtimeSinceStartup);
        materials = Resources.LoadAll<Material>("Materials");
        Debug.Log("loading material icons " + Time.realtimeSinceStartup);
        icons = Resources.LoadAll<Texture2D>("Icons");        

        Vector2 scale = new Vector2(1.0f, 1.0f);

        //Debug.Log("creating overlay textures " + Time.realtimeSinceStartup);
        //create the overlay texture and reset tiling values for each material
        foreach (Material mat in materials)
        {
            int width = mat.mainTexture.width;
            int height = mat.mainTexture.height;
            CreateOverlayTexture(mat, new Vector2(width, height));
            mat.mainTextureScale = scale;
            mat.SetTextureScale("_OverlayTexture", scale);

        }

    }

    //create a new transparent texture and add it to the material in the _OverlayTexture slot
    private void CreateOverlayTexture(Material mat, Vector2 size)
    {
        Texture2D newTexture = new Texture2D((int)size.x, (int)size.y);// GraphicsFormat.R8G8B8A8_UNorm, true);

        Color[] data = new Color[(int)size.x * (int)size.y];

        int index = 0;
        //set the every pixel to be transparent
        for(int x = 0; x < size.x; x++) {
            for(int y = 0; y < size.y; y++) {                        
                data[index] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                index++;
            }
        }

        newTexture.SetPixels(0, 0, (int)size.x, (int)size.y, data);
        newTexture.Apply(true);

        mat.SetTexture("_OverlayTexture", newTexture);
    }
}

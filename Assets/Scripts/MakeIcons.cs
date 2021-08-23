using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeIcons : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Material[] materials = Resources.LoadAll<Material>("Materials");

        //iterate through every material and create a scaled down version of it's main texture
        foreach (Material mat in materials)
        {
            Texture2D source = (Texture2D)mat.mainTexture;
            Texture2D readableTexture = Resize(source, 128, 128);

            byte[] bytes;
            bytes = readableTexture.EncodeToPNG();
    
            string destinationFile = Application.dataPath + "/Resources/Icons/" + mat.name + ".png";
            Debug.Log(destinationFile);
            System.IO.File.WriteAllBytes(destinationFile, bytes);               
        }
    }

    private Texture2D Resize(Texture2D source, int width, int height)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;

        Texture2D readableTexture = new Texture2D(width, height);
        readableTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableTexture.Apply();                    

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);

        return readableTexture;
    }
}

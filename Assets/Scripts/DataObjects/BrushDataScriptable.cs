using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "brushData", menuName = "brush data", order = 1)]
public class BrushDataScriptable : ScriptableObject
{
    public int brushRadius;
    public float brushStrength;
    public float brushRotation;
    public Texture2D brush;

    public Texture2D paintTexture;

    public float textureScale;


    public float[,] getMask(int length, int width)
    {
        float scaleX = brush.width / (width * 1.0f);
        float scaleY = brush.height / (length * 1.0f);
        float[,] mask = new float[length, width];

        for(int x = 0; x < width; x++) {
            for(int y = 0; y < length; y++) {
                //Debug.Log(brush.GetPixel((int)(x * scaleX), (int)(y * scaleY)).r);
                mask[y,x] = brush.GetPixel((int)(x * scaleX), (int)(y * scaleY)).r;
            }
        }

        return mask;
    }
}

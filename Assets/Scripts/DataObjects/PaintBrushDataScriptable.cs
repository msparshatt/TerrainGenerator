using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "paintbrushData", menuName = "paint brush data", order = 1)]
public class PaintBrushDataScriptable : BrushDataScriptable
{
    public enum MixTypes  {Top = 1, Steep, Bottom, Shallow, Painted, Unpainted};

    public bool useTexture;
    public Texture2D paintTexture;
    public Color color;

    public float textureScale;

    //filters
    public bool filter;
    public MixTypes filterType;
    public float filterFactor;
    public Texture2D paintMask;
}

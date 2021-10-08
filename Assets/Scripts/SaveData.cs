public struct SaveData
{
    public int version;
    public int terrainResolution;
    public byte[] heightmap;
    public int baseTexture;

    public byte[] baseTexture_colors;
    public int baseTexture2;
    public byte[] baseTexture2_colors;    
    public int mixType;
    public float mixFactor;
    public float tiling;
    public bool aoActive;
    public byte[] overlayTexture;
    public float paintTiling;
} 

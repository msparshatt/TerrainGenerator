public struct SaveData_v2
{
    public int version;
    public int terrainResolution;
    public byte[] heightmap;

    public int[] baseTexture;
    public byte[][] baseTexture_colors;
    public int[] mixType;
    public float[] mixFactor;

    public float tiling;
    public bool aoActive;
    public byte[] overlayTexture;
    public float paintTiling;
} 

public struct Version
{
    public int version;
}
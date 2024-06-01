using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HeightmapController : MonoBehaviour
{
    [SerializeField] private InternalDataScriptable internalData;
    [SerializeField] private Texture2D busyCursor;

    private ExportHeightmap exportHeightmap;
    private ExportTerrain exportTerrain;

    private TerrainData originalData;

    //copy used for procedural generation
    private TerrainData copyData;
    private Terrain thisTerrain;
    private bool shaderRunning;
    private int heightmapResolution;

    public void Start()
    {
        thisTerrain = gameObject.GetComponent<Terrain>();

        exportHeightmap = ExportHeightmap.instance;
        exportTerrain = ExportTerrain.instance;
        exportTerrain.terrainObject = thisTerrain;
        exportTerrain.busyCursor = busyCursor;
    }

    public void SetupChanges()
    {
        originalData = thisTerrain.terrainData;

        copyData = CopyTerrain(originalData);
        copyData.size = new Vector3(1000, 1000, 1000);

        thisTerrain.terrainData = copyData;

        thisTerrain.materialTemplate.SetVector("_CursorLocation", new Vector4(0f, 0f, 0f, 0f));            
    }

    public void RevertChanges()
    {
        thisTerrain.terrainData = originalData;
        thisTerrain.GetComponent<TerrainCollider>().terrainData = originalData;    
    }

    public void ApplyChanges(ProceduralGeneration procGen, bool erosion)
    {
        int resolution = thisTerrain.terrainData.heightmapResolution;
        thisTerrain.terrainData = originalData;
        thisTerrain.GetComponent<TerrainCollider>().terrainData = originalData;    

        CreateProceduralTerrain(procGen, erosion, resolution);
        //ApplyTextures();
    }

    //copy the terrain data object so that the master file won't be modified by any changes when running the program
    private TerrainData CopyTerrain(TerrainData original)
    {
        TerrainData data = new TerrainData();

        data.alphamapResolution = original.alphamapResolution;
        data.baseMapResolution = original.baseMapResolution;

        // The resolutionPerPatch is not publicly accessible so
        // it can not be cloned properly, thus the recommendet default
        // number of 16
        data.SetDetailResolution(original.detailResolution, 16);

        data.heightmapResolution = original.heightmapResolution;
        data.size = original.size;

        data.wavingGrassAmount = original.wavingGrassAmount;
        data.wavingGrassSpeed = original.wavingGrassSpeed;
        data.wavingGrassStrength = original.wavingGrassStrength;
        data.wavingGrassTint = original.wavingGrassTint;

        data.SetAlphamaps(0, 0, original.GetAlphamaps(0, 0, original.alphamapWidth, original.alphamapHeight));
        data.SetHeights(0, 0, original.GetHeights(0, 0, original.heightmapResolution, original.heightmapResolution));

        for (int n = 0; n < original.detailPrototypes.Length; n++)
        {
            data.SetDetailLayer(0, 0, n, original.GetDetailLayer(0, 0, original.detailWidth, original.detailHeight, n));
        }

        return data;
    }

    public void CreateFlatTerrain(int resolution)
    {
        if(thisTerrain == null)
            thisTerrain = gameObject.GetComponent<Terrain>();

        thisTerrain.terrainData.heightmapResolution = resolution;

        float[,] heights = new float[resolution, resolution];

        for(int x = 0; x < resolution; x++) {
            for(int y = 0; y < resolution; y++) {
                heights[y, x] = 0.5f;
            }
        }

        CreateTerrain(heights);
    }

    public void CreateTerrainFromHeightmap(string path = "")
    {
        float[,] heights = ReadHeightmap(path);
        thisTerrain.terrainData.heightmapResolution = heights.GetLength(0);
        CreateTerrain(heights); 

        thisTerrain.materialTemplate.SetVector("_CursorLocation", new Vector4(0f, 0f, 0f, 0f));            
        //ApplyTextures();
    }

    public void CreateTerrainFromHeightmap(byte[] data)
    {
        Debug.Log("Loading heightmap");
        int arrayLength = data.Length;
        float[] result = new float[arrayLength/2];
        for(int index = 0; index < (arrayLength/2); index += 1) {
            byte byte1 = data[index * 2];
            byte byte2 = data[index * 2 + 1];
            
            float value = ((byte2 << 8) + byte1) / 65535f;

            result[index] = value;                    
        }

        float[,] heights = ArrayHelper.ConvertTo2DArray(result);
        heightmapResolution = heights.GetLength(0);
        thisTerrain.terrainData.heightmapResolution = heightmapResolution;

        CreateTerrain(heights);
    }

    public void CreateProceduralTerrain(ProceduralGeneration procGen, bool erosion, int heightmapResolution)
    {
        thisTerrain.terrainData.heightmapResolution = heightmapResolution;

        float[] heights;
        Debug.Log("generating terrain");
        
        //HeightMapBuilder heights  = new HeightMapBuilder(shader, size);
        heights = procGen.GenerateHeightMap(heightmapResolution);            

        if(heights == null)
            return;

        if(erosion) { 
            heights = procGen.Erosion(heights, heightmapResolution);
        }

        Debug.Log("Creating terrain");
        CreateTerrain(ArrayHelper.ConvertTo2DArray(heights, 3));
    }

    protected void CreateTerrain(float[,] heights)
    {
        Debug.Log("Creating Terrain " + thisTerrain.terrainData.heightmapResolution);
        if(thisTerrain != null) {
            thisTerrain.terrainData.SetHeights(0,0, heights);
        }
    }

    public int HeightmapResolution()
    {
        return thisTerrain.terrainData.heightmapResolution;
        //return heightmapResolution;
    }

    private float[,] ReadHeightmap(string fileName)
    {
        float[] result = null;
        float maxHeight = 0f;
        int length = 0;
        if (File.Exists(fileName))
        {
            if(new FileInfo(fileName).Extension == ".png") {
                Debug.Log("png");
                byte[] pngData = System.IO.File.ReadAllBytes(fileName);
                Texture2D map = new Texture2D(10,10);
                ImageConversion.LoadImage(map, pngData);
                
                Debug.Log(map.width + ":" + map.height);
                Color[] pixels = map.GetPixels(0); //0, 0, map.width, map.height);
                //var rawValues = map.GetRawTextureData<Color32>();
                Debug.Log(pixels[0] + ":" + pixels[1] + ":" + pixels[2] + ":" + pixels[3]);
                length = pixels.Length;
                Debug.Log(length);

                result = new float[length];
                for(int index = 0; index < length; index += 1) {    
                    //Debug.Log(pixels[index]);
                    float value = (pixels[index].r + pixels[index].g + pixels[index].b) / 3f;
                    result[index] = value;                    
                }

            } else {
                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    length = (int)reader.BaseStream.Length;                
                    
                    if(length % 4 == 0) {
                        result = new float[length/4];
                        for(int index = 0; index < (length/4); index += 1) {
                            float value = reader.ReadSingle();
                            if(value > maxHeight) {
                                maxHeight = value;
                            }
                            result[index] = value/100f;                    
                        }
                    } else if(length % 2 == 0) {
                        result = new float[length/2];
                        for(int index = 0; index < (length/2); index += 1) {
                            byte byte1 = reader.ReadByte();
                            byte byte2 = reader.ReadByte();
                            
                            float value = ((byte2 << 8) + byte1) / 65535f;
                            if(value > maxHeight) {
                                maxHeight = value;
                            }
                            result[index] = value;                    
                        }
                    } else {
                        result = new float[length];
                        for(int index = 0; index < length; index += 1) {
                            byte byte1 = reader.ReadByte();
                            
                            float value = byte1 / 255f;
                            if(value > maxHeight) {
                                maxHeight = value;
                            }
                            result[index] = value;                    
                        }
                    }
                }
            }                   
        }
        float[,] heights = ArrayHelper.ConvertTo2DArray(result);

        return heights;
    }
     
    public RenderTexture GetHeightmapTexture()
    {
        return thisTerrain.terrainData.heightmapTexture;
    }

    public float[,] GetHeightmap()
    {
        int resolution = thisTerrain.terrainData.heightmapResolution;
        return thisTerrain.terrainData.GetHeights(0, 0, resolution, resolution);
    }

    public byte[] GetHeightmapAsBytes()
    {
        return exportHeightmap.GetHeightmap();
    }

    public void ExportTerrainAsObj(string fileName, bool applyAO, float scalefactor)
    {
        exportTerrain.Export(fileName, applyAO, scalefactor);
    }

    public void ExportTerrainAsRaw(string fileName)
    {
        exportHeightmap.Export(fileName);
    }
}

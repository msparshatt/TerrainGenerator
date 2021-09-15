using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Crosstales.FB;

public class TerrainManager
{
    //scriptable object containing any terrain data    
    //[SerializeField] private TerrainScriptable terrainData;
    public Terrain currentTerrain = null;
    public Material currentMaterial;

    public SettingsDataScriptable settingsData;

    private static TerrainManager _instance;

    private int _heightmapresolution;

    public ComputeShader shader;

    public static TerrainManager instance {
        get {
            if(_instance == null)
                _instance = new TerrainManager();

            return _instance;
        }
    }

    //return an array of the names of all the current terrains
    public void SetTerrainMaterial(Material material)
    {
        currentMaterial = material;
        currentTerrain.materialTemplate = material;
    }

    //copy the terrain data object so that the master file won't be modified by any changes when running the program
    public void setupTerrain()
    {
        TerrainData data = new TerrainData();
        TerrainData original = currentTerrain.terrainData;

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

        currentTerrain.terrainData = data;
        currentTerrain.GetComponent<TerrainCollider>().terrainData = data;
    }

    public void CreateFlatTerrain()
    {
        _heightmapresolution = settingsData.defaultTerrainResolution;

        float[,] heights = new float[_heightmapresolution, _heightmapresolution];

        for(int x = 0; x <_heightmapresolution; x++) {
            for(int y = 0; y < _heightmapresolution; y++) {
                heights[y, x] = 0.5f;
            }
        }

        CreateTerrain(heights);
    }

    public void CreateTerrainFromHeightmap(string path = "")
    {
        float[,] heights = ReadHeightmap(path);
        _heightmapresolution = heights.GetLength(0);
        
        CreateTerrain(heights);
    }

    public void CreateTerrainFromHeightmap(byte[] data)
    {
        int arrayLength = data.Length;
        float[] result = new float[arrayLength/2];
        for(int index = 0; index < (arrayLength/2); index += 1) {
            byte byte1 = data[index * 2];
            byte byte2 = data[index * 2 + 1];
            
            float value = ((byte2 << 8) + byte1) / 65535f;

            result[index] = value;                    
        }

        float[,] heights = ConvertTo2DArray(result);
        _heightmapresolution = heights.GetLength(0);

        CreateTerrain(heights);
    }

    public void CreateProceduralTerrain(ProceduralGeneration procGen, TerraceSettings terrace, Erosion erosion)
    {
        _heightmapresolution = procGen.size;

        float[,] heights;
        Debug.Log("generating terrain");
        
        //HeightMapBuilder heights  = new HeightMapBuilder(shader, size);
        heights = procGen.GenerateHeightMap();            
        //procGen.SetupHeightmap(heights);

        if(terrace != null) {
            Debug.Log("Applying terraces");
            heights = terrace.AddTerraces(heights, _heightmapresolution);
            //terrace.SetupTerraces(heights);
        }


        if(erosion != null) { 
            Debug.Log("Eroding");

            //heights = erosion.Erode(heights, _heightmapresolution, 50000);
            //erosion.SetupErosion(heights, 10000);
        }

        Debug.Log("Creating terrain");
        CreateTerrain(heights);

        //heights.Release();
    }

    protected void CreateTerrain(float[,] heights)
    {
        currentTerrain.terrainData.heightmapResolution = _heightmapresolution;
        currentTerrain.terrainData.SetHeights(0,0, heights);
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
        float[,] heights = ConvertTo2DArray(result);

        return heights;
    }
     
    //convert a 1D float array to a 2D height array so it can be applied to a terrain
    private float[,] ConvertTo2DArray(float[] heightData)
    {
        int resolution = (int)Mathf.Sqrt(heightData.Length);
        //Debug.Log(resolution);

        float[,] unityHeights = new float[resolution, resolution];

        Vector2 pos = Vector2.zero;
            
        for (int i = 0 ; i < heightData.Length; i++) {
            unityHeights[(int)pos.y, (int)pos.x] = heightData[i];

            if (pos.x < resolution - 1)
            {
                pos.x += 1;
            }
            else
            {
                pos.x = 0;
                pos.y += 1;
                if (pos.y >= resolution)
                {
                    break;
                }
            }
                
        }

        return unityHeights;
    }

    //convert a 2D float array into a 1D byte array in order to write it to a file
    private float[] ConvertTo1DArray(float[,] nmbs)
    {
        float[] nmbsBytes = new float[nmbs.GetLength(0) * nmbs.GetLength(1) * 2];
        int k = 0;
        for (int i = 0; i < nmbs.GetLength(0); i++)
        {
            for (int j = 0; j < nmbs.GetLength(1); j++)
            {
               float value = nmbs[i, j]; //convert from float to an integer
               nmbsBytes[k++] = value; // write LSB
            }
        }
        return nmbsBytes;
    }    

}

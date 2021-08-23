using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WorldKit.api.procedural.Builders;
using WorldKit.api.procedural.Layers;
using WorldKit.api.procedural.Utils;
//using Newtonsoft.Json;
using Crosstales.FB;

public class TerrainManager
{
    //scriptable object containing any terrain data    
    //[SerializeField] private TerrainScriptable terrainData;
    public Terrain currentTerrain = null;
    public Material currentMaterial;

    private static TerrainManager _instance;

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

    public void CreateFlatTerrain(float width, float length, float height)
    {
        int _heightmapresolution = currentTerrain.terrainData.heightmapResolution;
        float[,] heights = new float[_heightmapresolution, _heightmapresolution];

        for(int x = 0; x <_heightmapresolution; x++) {
            for(int y = 0; y < _heightmapresolution; y++) {
                heights[y, x] = 0.5f;
            }
        }

        CreateTerrain(width, length, height, heights);
    }

    public void CreateTerrainFromHeightmap(float width, float length, float height, string path = "")
    {
        float[,] heights = ReadHeightmap(path);
        
        CreateTerrain(width, length, height, heights);
    }

    public void CreateTerrainFromHeightmap(float width, float length, float height, byte[] data)
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

        CreateTerrain(width, length, height, heights);
    }

    public void CreateProceduralTerrain(ProceduralGeneration procGen, TerraceSettings terrace, Erosion erosion, float width, float length, float height)
    {
        int size = procGen.size;

        //float[,] heights;
        Debug.Log("generating terrain");
        
        HeightMapBuilder heights  = new HeightMapBuilder(shader, size);
        //procGen.GenerateHeightMap();            
        procGen.SetupHeightmap(heights);

        if(terrace != null) {
            Debug.Log("Applying terraces");
            //heights = terrace.AddTerraces(heights, size);
            terrace.SetupTerraces(heights);
        }


        if(erosion != null) { 
            Debug.Log("Eroding");

            //heights = erosion.Erode(heights, size, 50000);
            erosion.SetupErosion(heights, 10000);
        }

        Debug.Log("Creating terrain");
        CreateTerrain(width, length, height, ConvertTo2DArray(heights.HeightMap()));

        heights.Release();
    }
    

    protected void CreateTerrain(float width, float length, float height, float[,] heights)
    {
        //GameObject newTerrain = new GameObject("Main Terrain");
        TerrainData _TerrainData = new TerrainData();
         
        //x and z values are being multiplied by 16?????
        width = width/16;
        length = length/16;
        //float height = 600f;

        _TerrainData.size = new Vector3(width, height, length);  // * terrainSizeValue;
        _TerrainData.heightmapResolution = 512;
        _TerrainData.baseMapResolution = 512;
        _TerrainData.SetDetailResolution(512, 32);
        _TerrainData.name = "";

        int _heightmapWidth = _TerrainData.heightmapResolution;
        int _heightmapHeight = _TerrainData.heightmapResolution;

        _TerrainData.SetHeights(0, 0, heights);
 
        TerrainCollider _TerrainCollider = currentTerrain.gameObject.GetComponent<TerrainCollider>();
//        Terrain _TerrainComponent = newTerrain.AddComponent<Terrain>();
 
        _TerrainCollider.terrainData = _TerrainData;
        currentTerrain.terrainData = _TerrainData;
        
        //SetActiveTerrain(terrainList.Count - 1);
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

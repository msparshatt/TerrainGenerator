using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TerrainManager
{
    //scriptable object containing any terrain data    
    //[SerializeField] private TerrainScriptable terrainData;
    private Terrain currentTerrain = null;
    private Material[] baseMaterials;

    private Material terrainMaterial;
    private Shader materialShader;
    private Texture2D aoTexture;
    private SettingsDataScriptable settingsData;

    //references to the export classes
    private ExportHeightmap exportHeightmap;
    private ExportTerrain exportTerrain;

    //how to mix the two base materials
    public int[] mixTypes;
    public float[] mixFactors;
    public bool doNotApply;

    private Vector2 textureScale;

    private static TerrainManager _instance;

    private int _heightmapresolution;

    private TerrainData originalData;

    //copy used for procedural generation
    private TerrainData copyData;
    private int multiplier;

    private TerrainPainter painter;
    private TerrainSculpter sculpter;

    private Texture2D busyCursor;
    private ComputeShader textureShader;

    public static TerrainManager instance {
        get {
            if(_instance == null)
                _instance = new TerrainManager();

            return _instance;
        }
    }

    public TerrainManager()
    {
        currentTerrain = Terrain.activeTerrain;

        exportHeightmap = ExportHeightmap.instance;
        exportHeightmap.terrainObject = currentTerrain;


        painter = currentTerrain.GetComponent<TerrainPainter>();
        sculpter = currentTerrain.GetComponent<TerrainSculpter>();

        baseMaterials = new Material[5];
        
        originalData = CopyTerrain(currentTerrain.terrainData);

        currentTerrain.terrainData = originalData;
        currentTerrain.GetComponent<TerrainCollider>().terrainData = originalData;    

        mixFactors = new float[]{0.5f, 0.5f, 0.5f, 0.5f, 0.5f};
        mixTypes = new int[]{0, 0, 0, 0, 0};
        textureScale = new Vector2(1,1);
    }

    public void SetupTerrain(SettingsDataScriptable _settingsData, Texture2D _busyCursor, ComputeShader _textureShader, Shader _materialShader)
    {
        materialShader = _materialShader;
        CreateTextures();

        settingsData = _settingsData;
        exportTerrain = ExportTerrain.instance;
        exportTerrain.terrainObject = currentTerrain;
        //exportTerrain.scaleDropDown = scaleDropdown;
        busyCursor = _busyCursor;
        exportTerrain.busyCursor = busyCursor;
        textureShader = _textureShader;
    }

    //create a material and set up it's textures
    private void CreateTextures()
    {
        terrainMaterial = new Material(materialShader);
        currentTerrain.materialTemplate = terrainMaterial;

        int sizeX = 2048;
        int sizeY = 2048;

        Texture2D newTexture = new Texture2D(sizeX, sizeY);
        painter.ClearTexture(newTexture);

        terrainMaterial.mainTexture = newTexture;
        terrainMaterial.mainTextureScale = new Vector2(1f, 1f);

        aoTexture = new Texture2D((int)sizeX, (int)sizeY);// GraphicsFormat.R8G8B8A8_UNorm, true);
        painter.ClearTexture(aoTexture);

        newTexture = new Texture2D((int)sizeX, (int)sizeY);// GraphicsFormat.R8G8B8A8_UNorm, true);

        painter.ClearTexture(newTexture);

        terrainMaterial.SetTexture("_OverlayTexture", newTexture);
    }

    //return an array of the names of all the current terrains
    public void SetBaseMaterials(int index, Material material)
    {
        baseMaterials[index] = material;

        if(!doNotApply)
            ApplyTextures();
    }

    public void SetMixType(int materialIndex, int type)
    {
        mixTypes[materialIndex] = type;
    }

    public void SetMixFactor(int materialIndex, float factor)
    {
        mixFactors[materialIndex] = factor;
    }

    public void ScaleMaterial(float value)
    {
        Vector2 scale = new Vector2(value, value);
        //terrainMaterial.mainTextureScale = scale;
        textureScale = scale;
    }

    public void SetAO(bool isOn)
    {
        if(isOn)
            terrainMaterial.SetTexture("_AOTexture", aoTexture);
        else
            terrainMaterial.SetTexture("_AOTexture", null);

    }

    public void ClearOverlay()
    {
        painter.ClearTexture((Texture2D)terrainMaterial.GetTexture("_OverlayTexture"));
    }

    public Texture2D GetOverlay()
    {
        return (Texture2D)terrainMaterial.GetTexture("_OverlayTexture");
    }

    public void SetOverlay(Texture texture)
    {
        terrainMaterial.SetTexture("_OverlayTexture", texture);
    }

    public void SetupChanges()
    {
        copyData = CopyTerrain(originalData);
        copyData.heightmapResolution = 257;

        copyData.size = new Vector3(1000, 1000, 1000);

        currentTerrain.terrainData = copyData;
        currentTerrain.GetComponent<TerrainCollider>().terrainData = copyData;    
        multiplier = 4;
    }

    public void RevertChanges()
    {
        currentTerrain.terrainData = originalData;
    }

    public void ApplyChanges(ProceduralGeneration procGen, TerraceSettings terraces, Erosion erosion)
    {
        currentTerrain.terrainData = originalData;
        currentTerrain.GetComponent<TerrainCollider>().terrainData = originalData;    
        multiplier = 1;

        CreateProceduralTerrain(procGen, terraces, erosion);
        ApplyTextures();
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

        if(heights.GetLength(0) > currentTerrain.terrainData.heightmapResolution) {
            _heightmapresolution = currentTerrain.terrainData.heightmapResolution;
            float[,] newHeights = new float[_heightmapresolution, _heightmapresolution];

            for(int i = 0; i < _heightmapresolution; i++) {
                for(int j = 0; j < _heightmapresolution; j++) {
                    newHeights[i, j] = heights[i, j];
                }
            }

            CreateTerrain(newHeights);
        } else {
            CreateTerrain(heights); 
        }

        ApplyTextures();
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
        currentTerrain.terrainData.heightmapResolution = _heightmapresolution;

        CreateTerrain(heights);
    }

    public void CreateProceduralTerrain(ProceduralGeneration procGen, TerraceSettings terrace, Erosion erosion)
    {
        _heightmapresolution = currentTerrain.terrainData.heightmapResolution;

        float[,] heights;
        Debug.Log("generating terrain");
        
        //HeightMapBuilder heights  = new HeightMapBuilder(shader, size);
        heights = procGen.GenerateHeightMap(_heightmapresolution, multiplier);            

        if(terrace != null) {
            Debug.Log("Applying terraces");
            heights = terrace.AddTerraces(heights, _heightmapresolution);
        }


        if(erosion != null) { 
            Debug.Log("Eroding");

            heights = erosion.Erode(heights, _heightmapresolution);
        }

        Debug.Log("Creating terrain");
        CreateTerrain(heights);

    }

    protected void CreateTerrain(float[,] heights)
    {
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
    public RenderTexture GetHeightmapTexture()
    {
        return currentTerrain.terrainData.heightmapTexture;
    }

    public void ApplyTextures()
    {
        int hmResolution = currentTerrain.terrainData.heightmapResolution;

        int width = baseMaterials[0].mainTexture.width;
        int height = baseMaterials[0].mainTexture.height;

        //create textures to hold the result
        RenderTexture tex = new RenderTexture(width,height,24);
        tex.enableRandomWrite = true;
        tex.Create();
        RenderTexture aotex = new RenderTexture(width,height,24);
        aotex.enableRandomWrite = true;
        aotex.Create();

        //send parameters to the compute shader
        textureShader.SetFloat("tiling", textureScale.x);
        textureShader.SetInt("width", width);
        textureShader.SetInt("height", height);
        float[] factors = {0f, 0f, 0f, 0f,
        mixFactors[1], 0f, 0f, 0f,
        mixFactors[2], 0f, 0f, 0f,
        mixFactors[3], 0f, 0f, 0f,
        mixFactors[4], 0f, 0f, 0f,
        0f, 0f, 0f, 0f,
        0f, 0f, 0f, 0f,
        0f, 0f, 0f, 0f, 
        0f, 0f, 0f, 0f, 
        0f, 0f, 0f, 0f};

        int[] paddedMixTypes = {0, 0, 0, 0,
        mixTypes[1], 0, 0, 0,
        mixTypes[2], 0, 0, 0,
        mixTypes[3], 0, 0, 0,
        mixTypes[4], 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 0};

        textureShader.SetFloats("factors", factors);
        textureShader.SetInts("mixTypes", paddedMixTypes);
        textureShader.SetInt("heightMapResolution", hmResolution);

        if(false) {
            int kernelHandle = textureShader.FindKernel("ApplyTextures");


            textureShader.SetTexture(kernelHandle, "Result", tex);
            textureShader.SetTexture(kernelHandle, "aoResult", aotex);

            Texture2D tempTexture;
            tempTexture = (Texture2D)(baseMaterials[0].mainTexture);

            Texture2DArray inputTextures = new Texture2DArray(tempTexture.width, tempTexture.height, 4, TextureFormat.ARGB32, false); 
            Texture2DArray inputAOs = new Texture2DArray(tempTexture.width, tempTexture.height, 4, TextureFormat.ARGB32, false);
            for(int i = 0; i < 5; i++) {
                tempTexture = (Texture2D)(baseMaterials[0].mainTexture);
                inputTextures.SetPixels(tempTexture.GetPixels(), 0, 0);

                tempTexture = (Texture2D)(baseMaterials[i].GetTexture("_OcclusionMap"));;
                inputAOs.SetPixels(tempTexture.GetPixels(), 0, 0);
            }
            inputTextures.Apply();
            inputAOs.Apply();

            textureShader.SetTexture(kernelHandle, "textures", inputTextures);
            textureShader.SetTexture(kernelHandle, "aotextures", inputAOs);
            textureShader.SetInt("textureCount", 1);
            textureShader.SetTexture(kernelHandle, "heightMap", currentTerrain.terrainData.heightmapTexture);

            textureShader.Dispatch(kernelHandle, width/8, height/8, 1);

        } else {
            int kernelHandle = textureShader.FindKernel("ApplyTextures");

            textureShader.SetTexture(kernelHandle, "Result", tex);
            textureShader.SetTexture(kernelHandle, "aoResult", aotex);

            Texture2D tempTexture;

            tempTexture = (Texture2D)(baseMaterials[0].mainTexture);
            Texture2DArray inputTextures = new Texture2DArray(tempTexture.width, tempTexture.height, 5, TextureFormat.ARGB32, false); 
            Texture2DArray inputAOs = new Texture2DArray(tempTexture.width, tempTexture.height, 5, TextureFormat.ARGB32, false);

            for(int i = 0; i < 5; i++) {
                tempTexture = (Texture2D)(baseMaterials[i].mainTexture);
                inputTextures.SetPixels(tempTexture.GetPixels(), i, 0);

                tempTexture = (Texture2D)(baseMaterials[i].GetTexture("_OcclusionMap"));;
                inputAOs.SetPixels(tempTexture.GetPixels(), i, 0);
            }
            inputTextures.Apply();
            inputAOs.Apply();

            textureShader.SetTexture(kernelHandle, "textures", inputTextures);
            textureShader.SetTexture(kernelHandle, "aotextures", inputAOs);
            textureShader.SetInt("textureCount", 5);

            textureShader.SetTexture(kernelHandle, "heightMap", currentTerrain.terrainData.heightmapTexture);
            textureShader.Dispatch(kernelHandle, width/8, height/8, 1);
        }

        Texture2D tex2D = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, true);
        RenderTexture.active = tex;
        tex2D.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex2D.Apply();
        terrainMaterial.mainTexture = tex2D;

        RenderTexture.active = aotex;
        aoTexture.ReadPixels(new Rect(0, 0, aotex.width, aotex.height), 0, 0);
        aoTexture.Apply();
    }

    public Color GetPixel(Texture2D texture, int u, int v)
    {
        u = u % texture.width;
        v = v % texture.height;
        return texture.GetPixel(u, v);
    }

    public float[,] GetHeightmap()
    {
        int resolution = currentTerrain.terrainData.heightmapResolution;
        return currentTerrain.terrainData.GetHeights(0, 0, resolution, resolution);
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

    //translate texture uv coords into heightmap coords
    private Vector2Int TranslateCoordinates(int u, int v)
    {
        int heightmapSize = currentTerrain.terrainData.heightmapResolution;
        Terrain terrain = currentTerrain;
        Texture2D texture = (Texture2D)currentTerrain.materialTemplate.mainTexture;

        int textureWidth = texture.width;
        int textureHeight = texture.height;

        int x = (int)(u * heightmapSize / (textureWidth));
        int y = (int)(v * heightmapSize / (textureHeight));

        return new Vector2Int(x, y);
    }
}

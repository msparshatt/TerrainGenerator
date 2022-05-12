using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TerrainManager
{
    //scriptable object containing any terrain data    
    //[SerializeField] private TerrainScriptable terrainData;
    private Terrain currentTerrain = null;

    private Material terrainMaterial;
    private Shader materialShader;
    private Texture2D aoTexture;
    private SettingsDataScriptable settingsData;
    private InternalDataScriptable internalData;
    private PaintBrushDataScriptable paintBrushData;

    //references to the export classes
    private ExportHeightmap exportHeightmap;
    private ExportTerrain exportTerrain;

    //how to mix the two base materials
    public Material[] baseMaterials;
    public int[] mixTypes;
    public float[] mixFactors;
    private float[] offsets;
    public bool doNotApply;

    public Color[] colors;

    private Vector2 textureScale;

    private static TerrainManager _instance;

    private int _heightmapresolution;

    private TerrainData originalData;

    //copy used for procedural generation
    private TerrainData copyData;

    private TerrainPainter painter;
    private TerrainSculpter sculpter;

    private Texture2D busyCursor;
    private ComputeShader textureShader;
    private Light sun;

    //flag to avoid running multipe compute shaders at the same time
    private bool shaderRunning;

    private List<Vector4> maxima;
    private List<Vector4> minima;

    //amount that a point in the heightmap needs to be higher/lower that the surrounding points to count as a maxima/minima
    private const float MARGIN = 0.075f;  
    
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
        offsets = new float[]{0, 0, 0, 0, 0};
        textureScale = new Vector2(1,1);

        maxima = new List<Vector4>();
        minima = new List<Vector4>();

        ClearMaximaAndMinima();
        maxima.Add(new Vector4(-1, -1, -1, -1));
        minima.Add(new Vector4(-1, -1, -1, -1));

        colors = new Color[InternalDataScriptable.NUMBER_MATERIALS];
        for(int i = 0; i < InternalDataScriptable.NUMBER_MATERIALS; i++){
            colors[i] = Color.white;
        }
    }

    public void SetupTerrain(SettingsDataScriptable _settingsData, InternalDataScriptable _internalData, Texture2D _busyCursor, ComputeShader _textureShader, Shader _materialShader, Light _sun, PaintBrushDataScriptable _paintBrushData)
    {
        materialShader = _materialShader;
        sun = _sun;
        CreateTextures();

        settingsData = _settingsData;
        internalData = _internalData;
        exportTerrain = ExportTerrain.instance;
        exportTerrain.terrainObject = currentTerrain;
        //exportTerrain.scaleDropDown = scaleDropdown;
        busyCursor = _busyCursor;
        exportTerrain.busyCursor = busyCursor;
        textureShader = _textureShader;
        paintBrushData = _paintBrushData;
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

        terrainMaterial.SetColor("_LightColor", sun.color);
        terrainMaterial.SetVector("_MainLightPosition", sun.transform.position);
        aoTexture = new Texture2D((int)sizeX, (int)sizeY);// GraphicsFormat.R8G8B8A8_UNorm, true);
        painter.ClearTexture(aoTexture);

        newTexture = new Texture2D((int)sizeX, (int)sizeY);// GraphicsFormat.R8G8B8A8_UNorm, true);

        painter.ClearTexture(newTexture);

        terrainMaterial.SetTexture("_OverlayTexture", newTexture);
    }

    public void ApplyLighting(bool apply)
    {
        int lightOn = 0;
        if(apply)
            lightOn = 1;

        terrainMaterial.SetInt("_ApplyLighting", lightOn);
    }

    public void MoveSun(float height, float position)
    {
        sun.transform.localRotation = Quaternion.Euler(height, position, 0);

        UpdateLighting();
    }

    public Vector3 GetSunPosition()
    {
        return sun.transform.rotation * Vector3.back;
    }

    public void SetSunColor(Color sunColor)
    {
        sun.color = sunColor;
        UpdateLighting();
    }
    public void UpdateLighting()
    {
        terrainMaterial.SetColor("_LightColor", sun.color);
        Vector3 sunPos = sun.transform.rotation * Vector3.back;
        terrainMaterial.SetVector("_MainLightPosition", sunPos);
    }

    public void InitialiseBaseMaterials(Material[] materials)
    {
        for(int index = 0; index < materials.Length; index++) {
            baseMaterials[index] = materials[index];
        }

        ApplyTextures();
    }

    public void SetBaseMaterials(int index, Material material)
    {
        baseMaterials[index] = material;

        if(!doNotApply)
            ApplyTextures();
    }

    public void SetBaseColor(int index, Color color)
    {
        colors[index] = color;
        baseMaterials[index] = null;

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

    public void SetOffset(int materialIndex, float offset)
    {
        offsets[materialIndex] = offset;
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

        copyData.size = new Vector3(1000, 1000, 1000);

        currentTerrain.terrainData = copyData;
        currentTerrain.GetComponent<TerrainCollider>().terrainData = copyData;    
        currentTerrain.materialTemplate.SetVector("_CursorLocation", new Vector4(0f, 0f, 0f, 0f));            
    }

    public void RevertChanges()
    {
        currentTerrain.terrainData = originalData;
        currentTerrain.GetComponent<TerrainCollider>().terrainData = originalData;    
    }

    public void ApplyChanges(ProceduralGeneration procGen, bool erosion)
    {
        currentTerrain.terrainData = originalData;
        currentTerrain.GetComponent<TerrainCollider>().terrainData = originalData;    

        CreateProceduralTerrain(procGen, erosion);
        FindMaximaAndMinima();
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
        FindMaximaAndMinima();        
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

        FindMaximaAndMinima();
        currentTerrain.materialTemplate.SetVector("_CursorLocation", new Vector4(0f, 0f, 0f, 0f));            
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
        FindMaximaAndMinima();
    }

    public void CreateProceduralTerrain(ProceduralGeneration procGen, bool erosion)
    {
        _heightmapresolution = currentTerrain.terrainData.heightmapResolution;

        float[] heights;
        Debug.Log("generating terrain");
        
        //HeightMapBuilder heights  = new HeightMapBuilder(shader, size);
        heights = procGen.GenerateHeightMap(_heightmapresolution);            

        if(heights == null)
            return;

        if(erosion) { 
            heights = procGen.Erosion(heights, _heightmapresolution);
        }

        Debug.Log("Creating terrain");
        CreateTerrain(ConvertTo2DArray(heights, 3));
        FindMaximaAndMinima();
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
    public float[,] ConvertTo2DArray(float[] heightData, int borderSize = 0)
    {
        int size = (int)Mathf.Sqrt(heightData.Length) - borderSize * 2;

        return ConvertTo2DArray(heightData, size, size, borderSize);
    }

    public float[,] ConvertTo2DArray(float[] heightData, int width, int length, int borderSize = 0)
    {
        int outerWidth = width + borderSize * 2;

        float[,] unityHeights = new float[length, width];

        int index = (outerWidth) * borderSize + borderSize;

        for(int i = 0; i < length; i++) {
            for(int j = 0; j < width; j++) {
                unityHeights[i, j] = heightData[index];
                index++;
            }            
            index += borderSize * 2;
        }

        return unityHeights;
    }

    public float[] ConvertTo1DFloatArray(float[,] nmbs)
    {
        float[] result = new float[nmbs.GetLength(0) * nmbs.GetLength(1)];
        int k = 0;

        for (int i = 0; i < nmbs.GetLength(0); i++)
        {
            for (int j = 0; j < nmbs.GetLength(1); j++)
            {
               result[k++] = nmbs[i, j];
            }
        }
        return result;
    }    

    public RenderTexture GetHeightmapTexture()
    {
        return currentTerrain.terrainData.heightmapTexture;
    }

    public void ApplyTextures()
    {
        if(shaderRunning || doNotApply)
            return;

        Cursor.SetCursor(busyCursor, Vector2.zero, CursorMode.Auto); 

        //force the cursor to update
        Cursor.visible = false;
        Cursor.visible = true;

        shaderRunning = true;

        //get the handle for the correct shader kernel
        int kernelHandle = textureShader.FindKernel("ApplyTextures");

        int hmResolution = currentTerrain.terrainData.heightmapResolution;

        int width = 2048;
        int height = 2048;
        textureShader.SetInt("width", width);
        textureShader.SetInt("height", height);

        //create textures to hold the results
        RenderTexture tex = new RenderTexture(width,height,24);
        tex.enableRandomWrite = true;
        tex.Create();
        RenderTexture aotex = new RenderTexture(width,height,24);
        aotex.enableRandomWrite = true;
        aotex.Create();

        RenderTexture mask = new RenderTexture(width,height,24);
        mask.enableRandomWrite = true;
        mask.Create();

        //send textures to compute shader
        textureShader.SetTexture(kernelHandle, "Result", tex);
        textureShader.SetTexture(kernelHandle, "aoResult", aotex);
        textureShader.SetTexture(kernelHandle, "paintMask", mask);

        //send parameters to the compute shader
        textureShader.SetFloat("tiling", textureScale.x);
        float[] factors =  new float[20];
        factors[4] = mixFactors[1];
        factors[8] = mixFactors[2];
        factors[12] = mixFactors[3];
        factors[16] = mixFactors[4];

        int[] paddedMixTypes = new int[20];
        paddedMixTypes[4] = mixTypes[1];
        paddedMixTypes[8] = mixTypes[2];
        paddedMixTypes[12] = mixTypes[3];
        paddedMixTypes[16] = mixTypes[4];

        float[] paddedOffsets = new float[20];
        paddedOffsets[4] = offsets[1];
        paddedOffsets[8] = offsets[2];
        paddedOffsets[12] = offsets[3];
        paddedOffsets[16] = offsets[4];

        textureShader.SetFloats("factors", factors);
        textureShader.SetFloats("offsets", paddedOffsets);
        textureShader.SetInts("mixTypes", paddedMixTypes);
        textureShader.SetInt("heightMapResolution", hmResolution);

        Texture2D tempTexture;

        //tempTexture = (Texture2D)(baseMaterials[0].mainTexture);
        Texture2DArray inputTextures = new Texture2DArray(2048, 2048, 5, TextureFormat.DXT5, false);
        Texture2DArray inputAOs = new Texture2DArray(2048, 2048, 5, TextureFormat.DXT1, false);

        float[] colorValues = new float[InternalDataScriptable.NUMBER_MATERIALS * 4];

        for(int i = 0; i < InternalDataScriptable.NUMBER_MATERIALS; i++) {
            if(baseMaterials[i] != null) {
                Graphics.CopyTexture(baseMaterials[i].mainTexture, 0, 0, inputTextures, i, 0);
                Graphics.CopyTexture(baseMaterials[i].GetTexture("_OcclusionMap"), 0, 0, inputAOs, i, 0);

                colorValues[i * 4] = -1;
                colorValues[i * 4 + 1] = -1;
                colorValues[i * 4 + 2] = -1;
                colorValues[i * 4 + 3] = -1;
            } else {
                colorValues[i * 4] = colors[i].r;
                colorValues[i * 4 + 1] = colors[i].g;
                colorValues[i * 4 + 2] = colors[i].b;
                colorValues[i * 4 + 3] = colors[i].a;
            }
        }

        textureShader.SetFloats("colors", colorValues);

        textureShader.SetTexture(kernelHandle, "textures", inputTextures);
        textureShader.SetTexture(kernelHandle, "aotextures", inputAOs);
        textureShader.SetInt("textureCount", InternalDataScriptable.NUMBER_MATERIALS);

        float[] mapArray = ConvertTo1DFloatArray(currentTerrain.terrainData.GetHeights(0, 0, currentTerrain.terrainData.heightmapResolution, currentTerrain.terrainData.heightmapResolution));
        ComputeBuffer heightMapBuffer = new ComputeBuffer(mapArray.Length, sizeof(float));
        heightMapBuffer.SetData(mapArray);
        textureShader.SetBuffer(kernelHandle, "heightmap", heightMapBuffer);

        //send maxima and minima
        textureShader.SetVectorArray("maxima", maxima.ToArray());
        textureShader.SetVectorArray("minima", minima.ToArray());

        //send filter settings
        textureShader.SetInt("paintMaskType", (int)paintBrushData.filterType);
        textureShader.SetFloat("paintMaskFactor", paintBrushData.filterFactor);

        //run the shader
        textureShader.Dispatch(kernelHandle, width/8, height/8, 1);

        Texture2D tex2D = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, true);
        RenderTexture.active = tex;
        tex2D.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex2D.Apply();
        terrainMaterial.mainTexture = tex2D;

        RenderTexture.active = aotex;
        aoTexture.ReadPixels(new Rect(0, 0, aotex.width, aotex.height), 0, 0);
        aoTexture.Apply();


        tex2D = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, true);
        RenderTexture.active = mask;
        tex2D.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex2D.Apply();
        paintBrushData.paintMask = tex2D;
        terrainMaterial.SetTexture("_PaintMask", tex2D);

        heightMapBuffer.Release();

        shaderRunning = false;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); 
    }

    public void ApplyMask()
    {
        if(shaderRunning || doNotApply)
            return;

        Cursor.SetCursor(busyCursor, Vector2.zero, CursorMode.Auto); 

        //force the cursor to update
        Cursor.visible = false;
        Cursor.visible = true;

        shaderRunning = true;

        //get the handle for the correct shader kernel
        int kernelHandle = textureShader.FindKernel("ApplyMask");

        int hmResolution = currentTerrain.terrainData.heightmapResolution;

        int width = 2048;
        int height = 2048;
        textureShader.SetInt("width", width);
        textureShader.SetInt("height", height);

        //create textures to hold the result
        RenderTexture mask = new RenderTexture(width,height,24);
        mask.enableRandomWrite = true;
        mask.Create();

        //send textures to compute shader
        textureShader.SetTexture(kernelHandle, "paintMask", mask);

        //send parameters to the compute shader
        textureShader.SetInt("heightMapResolution", hmResolution);

        float[] mapArray = ConvertTo1DFloatArray(currentTerrain.terrainData.GetHeights(0, 0, currentTerrain.terrainData.heightmapResolution, currentTerrain.terrainData.heightmapResolution));
        ComputeBuffer heightMapBuffer = new ComputeBuffer(mapArray.Length, sizeof(float));
        heightMapBuffer.SetData(mapArray);
        textureShader.SetBuffer(kernelHandle, "heightmap", heightMapBuffer);

        //send filter settings
        textureShader.SetInt("paintMaskType", (int)paintBrushData.filterType);
        textureShader.SetFloat("paintMaskFactor", paintBrushData.filterFactor);

        //run the shader
        textureShader.Dispatch(kernelHandle, width/8, height/8, 1);

        Texture2D tex2D = new Texture2D(mask.width, mask.height, TextureFormat.ARGB32, true);
        RenderTexture.active = mask;
        tex2D.ReadPixels(new Rect(0, 0, mask.width, mask.height), 0, 0);
        tex2D.Apply();
        paintBrushData.paintMask = tex2D;
        terrainMaterial.SetTexture("_PaintMask", tex2D);

        heightMapBuffer.Release();

        shaderRunning = false;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); 
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

    public void TogglePaintMask(bool isOn)
    {
        if(isOn) {
            terrainMaterial.SetInt("_ApplyMask", 1);
        } else {
            terrainMaterial.SetInt("_ApplyMask", 0);
        }
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

    public void ClearMaximaAndMinima()
    {
        maxima.Clear();
        minima.Clear();
    }

    public void FindMaximaAndMinima()
    {
        Cursor.SetCursor(busyCursor, Vector2.zero, CursorMode.Auto); 

        //force the cursor to update
        Cursor.visible = false;
        Cursor.visible = true;

        ClearMaximaAndMinima();

        if(internalData.detectMaximaAndMinima) {
            TerrainData data = currentTerrain.terrainData;
            int resolution = data.heightmapResolution;

            for(int x = 1; x < resolution - 1; x++) {
                for(int y = 1; y < resolution - 1; y++) {
                    if(CheckPointisMaxima(x,y)) {
                        maxima.Add(new Vector4(x, y, currentTerrain.terrainData.GetHeight(x,y) / 1000.0f, 0));
                    }

                    if(CheckPointisMinima(x,y)) {
                        minima.Add(new Vector4(x, y, currentTerrain.terrainData.GetHeight(x,y) / 1000.0f, 0));
                    }
                }
            }
            maxima = RemoveDuplicates(maxima);
            minima = RemoveDuplicates(minima);
        }
        maxima.Add(new Vector4(-1, -1, -1, -1));
        minima.Add(new Vector4(-1, -1, -1, -1));

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); 
    }

    private List<Vector4> RemoveDuplicates(List<Vector4> items)
    {
        List<Vector4> result = new List<Vector4>();
        for (int i = 0; i < items.Count; i++)
        {
            // Assume not duplicate.
            bool duplicate = false;
            for (int z = 0; z < i; z++)
            {
                if ((items[z].x >= items[i].x - 1 && items[z].x <= items[i].x + 1) && 
                    (items[z].y >= items[i].y - 1 && items[z].y <= items[i].y + 1) && 
                    (items[z].z >= items[i].z - 1 && items[z].z <= items[i].z + 1))
                {
                    // This is a duplicate.
                    duplicate = true;
                    break;
                }
            }
            // If not duplicate, add to result.
            if (!duplicate)
            {
                result.Add(items[i]);
            }
        }
        return result;
    }
    private bool CheckPointisMaxima(int x, int y)
    {
        float height = currentTerrain.terrainData.GetHeight(x,y) - MARGIN;

        if(currentTerrain.terrainData.GetHeight(x + 1,y) >= height)
            return false;
        if(currentTerrain.terrainData.GetHeight(x - 1,y) >= height)
            return false;
        if(currentTerrain.terrainData.GetHeight(x,y + 1) >= height)
            return false;
        if(currentTerrain.terrainData.GetHeight(x,y - 1) >= height)
            return false;

        return true;
    }

    private bool CheckPointisMinima(int x, int y)
    {
        float height = currentTerrain.terrainData.GetHeight(x,y) + MARGIN;

        if(currentTerrain.terrainData.GetHeight(x + 1,y) <= height)
            return false;
        if(currentTerrain.terrainData.GetHeight(x - 1,y) <= height)
            return false;
        if(currentTerrain.terrainData.GetHeight(x,y + 1) <= height)
            return false;
        if(currentTerrain.terrainData.GetHeight(x,y - 1) <= height)
            return false;

        return true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialController : MonoBehaviour
{
    [SerializeField] private Texture2D busyCursor;
    [SerializeField] private ComputeShader textureShader;
    [SerializeField] private Light sun;
    [SerializeField] private Shader materialShader;

    [SerializeField] private SettingsDataScriptable settingsData;
    [SerializeField] private MaterialSettings materialSettings;
    [SerializeField] private PaintBrushDataScriptable paintBrushData;

    private Terrain thisTerrain;
    private Material terrainMaterial;
    private Texture2D aoTexture;
    private TerrainPainter painter;
    //private TerrainSculpter sculpter;


    //flag to avoid running multipe compute shaders at the same time
    private bool shaderRunning;

    private int[] paintMaskConverter = new int[] {0, 1, 2, 3, 4, 101, 102};

    private bool materialSetup = false;
    //storage for data used by the TextureComputeShader
    private Texture2DArray inputTextures;
    private Texture2DArray inputAOs;
    private RenderTexture resultRenderTexture;
    private RenderTexture aoRenderTexture;
    private RenderTexture mask;
    private RenderTexture contours;
    private Texture2D resultTexture;
    private Texture2D maskTexture;
    private Texture2D contourTexture;
    private int textureResolution = 2048;


    // Start is called before the first frame update
    void Start()
    {
        thisTerrain = gameObject.GetComponent<Terrain>();

        inputTextures = new Texture2DArray(textureResolution, textureResolution, 5, TextureFormat.DXT5, false);
        inputAOs = new Texture2DArray(textureResolution, textureResolution, 5, TextureFormat.DXT1, false);

        resultRenderTexture = new RenderTexture(textureResolution, textureResolution, 24);
        resultRenderTexture.enableRandomWrite = true;
        resultRenderTexture.Create();

        aoRenderTexture = new RenderTexture(textureResolution, textureResolution, 24);
        aoRenderTexture.enableRandomWrite = true;
        aoRenderTexture.Create();

        mask = new RenderTexture(textureResolution, textureResolution, 24);
        mask.enableRandomWrite = true;
        mask.Create();

        contours = new RenderTexture(textureResolution, textureResolution, 24);
        contours.enableRandomWrite = true;
        contours.Create();

        resultTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.ARGB32, true);
        maskTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.ARGB32, true);
        contourTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.ARGB32, true);
        painter = gameObject.GetComponent<TerrainPainter>();

        CreateTextures();        
        materialSetup = true;
        ApplyTextures();
    }

    private void CreateTextures()
    {
        terrainMaterial = new Material(materialShader);
        thisTerrain.materialTemplate = terrainMaterial;

        Texture2D newTexture = new Texture2D(textureResolution, textureResolution);
        painter.ClearTexture(newTexture);

        terrainMaterial.mainTexture = newTexture;
        terrainMaterial.mainTextureScale = new Vector2(1f, 1f);

        terrainMaterial.SetColor("_LightColor", sun.color);
        terrainMaterial.SetVector("_MainLightPosition", sun.transform.position);
        aoTexture = new Texture2D(textureResolution, textureResolution);// GraphicsFormat.R8G8B8A8_UNorm, true);
        painter.ClearTexture(aoTexture);

        newTexture = new Texture2D(textureResolution, textureResolution);// GraphicsFormat.R8G8B8A8_UNorm, true);

        painter.ClearTexture(newTexture);

        terrainMaterial.SetTexture("_OverlayTexture", newTexture);
    }


    public void ApplyTextures()
    {
        if(shaderRunning|| ! materialSetup)
            return;

        Cursor.SetCursor(busyCursor, Vector2.zero, CursorMode.Auto); 

        //force the cursor to update
        Cursor.visible = false;
        Cursor.visible = true;

        shaderRunning = true;

        //get the handle for the correct shader kernel
        int kernelHandle = textureShader.FindKernel("ApplyTextures");

        int hmResolution = thisTerrain.terrainData.heightmapResolution;

        textureShader.SetInt("width", textureResolution);
        textureShader.SetInt("height", textureResolution);

        //send textures to compute shader
        textureShader.SetTexture(kernelHandle, "Result", resultRenderTexture);
        textureShader.SetTexture(kernelHandle, "aoResult", aoRenderTexture);
        textureShader.SetTexture(kernelHandle, "paintMask", mask);
        textureShader.SetTexture(kernelHandle, "contourMask", contours);

        //send parameters to the compute shader
        textureShader.SetFloat("tiling", materialSettings.materialScale);
        float[] factors =  new float[20];
        factors[4] = materialSettings.mixFactors[1];
        factors[8] = materialSettings.mixFactors[2];
        factors[12] = materialSettings.mixFactors[3];
        factors[16] = materialSettings.mixFactors[4];

        int[] paddedMixTypes = new int[20];
        paddedMixTypes[4] = materialSettings.mixTypes[1];
        paddedMixTypes[8] = materialSettings.mixTypes[2];
        paddedMixTypes[12] = materialSettings.mixTypes[3];
        paddedMixTypes[16] = materialSettings.mixTypes[4];

        float[] paddedOffsets = new float[20];
        paddedOffsets[4] = materialSettings.mixOffsets[1];
        paddedOffsets[8] = materialSettings.mixOffsets[2];
        paddedOffsets[12] = materialSettings.mixOffsets[3];
        paddedOffsets[16] = materialSettings.mixOffsets[4];

        textureShader.SetFloats("factors", factors);
        textureShader.SetFloats("offsets", paddedOffsets);
        textureShader.SetInts("mixTypes", paddedMixTypes);
        textureShader.SetInt("heightMapResolution", hmResolution);

        float[] colorValues = new float[MaterialSettings.NUMBER_MATERIALS * 4];

        for(int i = 0; i < MaterialSettings.NUMBER_MATERIALS; i++) {
            if(materialSettings.useTexture[i]) {
                Material mat = GameResources.instance.materials[materialSettings.currentMaterialIndices[i]];
                Graphics.CopyTexture(mat.mainTexture, 0, 0, inputTextures, i, 0);
                Graphics.CopyTexture(mat.GetTexture("_OcclusionMap"), 0, 0, inputAOs, i, 0);

                colorValues[i * 4] = -1;
                colorValues[i * 4 + 1] = -1;
                colorValues[i * 4 + 2] = -1;
                colorValues[i * 4 + 3] = -1;
            } else {
                colorValues[i * 4] = materialSettings.colors[i].r;
                colorValues[i * 4 + 1] = materialSettings.colors[i].g;
                colorValues[i * 4 + 2] = materialSettings.colors[i].b;
                colorValues[i * 4 + 3] = materialSettings.colors[i].a;
            }
        }

        textureShader.SetFloats("colors", colorValues);

        textureShader.SetTexture(kernelHandle, "textures", inputTextures);
        textureShader.SetTexture(kernelHandle, "aotextures", inputAOs);
        textureShader.SetInt("textureCount", MaterialSettings.NUMBER_MATERIALS);

        float[] mapArray = ArrayHelper.ConvertTo1DFloatArray(thisTerrain.terrainData.GetHeights(0, 0, thisTerrain.terrainData.heightmapResolution, thisTerrain.terrainData.heightmapResolution));
        ComputeBuffer heightMapBuffer = new ComputeBuffer(mapArray.Length, sizeof(float));
        heightMapBuffer.SetData(mapArray);
        textureShader.SetBuffer(kernelHandle, "heightmap", heightMapBuffer);

        //send filter settings
        textureShader.SetInt("paintMaskType", paintMaskConverter[(int)paintBrushData.filterType]);
        textureShader.SetFloat("paintMaskFactor", paintBrushData.filterFactor);
        textureShader.SetTexture(kernelHandle, "overlayTexture", terrainMaterial.GetTexture("_OverlayTexture"));

        //run the shader
        textureShader.Dispatch(kernelHandle, textureResolution/8, textureResolution/8, 1);

        //Texture2D tex2D = new Texture2D(resultRenderTexture.width, resultRenderTexture.height, TextureFormat.ARGB32, true);
        RenderTexture.active = resultRenderTexture;
        resultTexture.ReadPixels(new Rect(0, 0, resultRenderTexture.width, resultRenderTexture.height), 0, 0);
        resultTexture.Apply();
        terrainMaterial.mainTexture = resultTexture;

        RenderTexture.active = aoRenderTexture;
        aoTexture.ReadPixels(new Rect(0, 0, aoRenderTexture.width, aoRenderTexture.height), 0, 0);
        aoTexture.Apply();


        RenderTexture.active = mask;
        maskTexture.ReadPixels(new Rect(0, 0, resultRenderTexture.width, resultRenderTexture.height), 0, 0);
        maskTexture.Apply();
        paintBrushData.paintMask = maskTexture;
        terrainMaterial.SetTexture("_PaintMask", maskTexture);

        RenderTexture.active = contours;
        contourTexture.ReadPixels(new Rect(0, 0, resultRenderTexture.width, resultRenderTexture.height), 0, 0);
        contourTexture.Apply();
        //paintBrushData.paintMask = maskTexture;
        terrainMaterial.SetTexture("_ContourMask", contourTexture);

        heightMapBuffer.Release();

        shaderRunning = false;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); 
    }

    public void ApplyMask()
    {
        if(shaderRunning || !materialSetup)
            return;

        Cursor.SetCursor(busyCursor, Vector2.zero, CursorMode.Auto); 

        //force the cursor to update
        Cursor.visible = false;
        Cursor.visible = true;

        shaderRunning = true;

        //get the handle for the correct shader kernel
        int kernelHandle = textureShader.FindKernel("ApplyMask");

        int hmResolution = thisTerrain.terrainData.heightmapResolution;

        textureShader.SetInt("width", textureResolution);
        textureShader.SetInt("height", textureResolution);

        //send textures to compute shader
        textureShader.SetTexture(kernelHandle, "paintMask", mask);

        //send parameters to the compute shader
        textureShader.SetInt("heightMapResolution", hmResolution);

        float[] mapArray = ArrayHelper.ConvertTo1DFloatArray(thisTerrain.terrainData.GetHeights(0, 0, thisTerrain.terrainData.heightmapResolution, thisTerrain.terrainData.heightmapResolution));
        ComputeBuffer heightMapBuffer = new ComputeBuffer(mapArray.Length, sizeof(float));
        heightMapBuffer.SetData(mapArray);
        textureShader.SetBuffer(kernelHandle, "heightmap", heightMapBuffer);

        //send filter settings
        textureShader.SetInt("paintMaskType", paintMaskConverter[(int)paintBrushData.filterType]);
        textureShader.SetFloat("paintMaskFactor", paintBrushData.filterFactor);
        textureShader.SetTexture(kernelHandle, "overlayTexture", terrainMaterial.GetTexture("_OverlayTexture"));

        //run the shader
        textureShader.Dispatch(kernelHandle, textureResolution/8, textureResolution/8, 1);

        RenderTexture.active = mask;
        maskTexture.ReadPixels(new Rect(0, 0, mask.width, mask.height), 0, 0);
        maskTexture.Apply();
        paintBrushData.paintMask = maskTexture;
        terrainMaterial.SetTexture("_PaintMask", maskTexture);

        heightMapBuffer.Release();

        shaderRunning = false;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); 
    }

    public void TogglePaintMask(bool isOn)
    {
        if(isOn) {
            terrainMaterial.SetInt("_ApplyMask", 1);
        } else {
            terrainMaterial.SetInt("_ApplyMask", 0);
        }
    }

    public void ApplyLighting(bool apply)
    {
        int lightOn = 0;
        if(apply)
            lightOn = 1;

        terrainMaterial.SetInt("_ApplyLighting", lightOn);
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


}

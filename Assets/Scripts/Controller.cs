using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;

public class Controller : MonoBehaviour
{
    [SerializeField] private Texture blankAO;
    [SerializeField] private Texture2D busyCursor;
    [SerializeField] private CameraController cameraController;

    [Header("Shaders")]
    [SerializeField] private ComputeShader textureShader;
    [SerializeField] private Shader materialShader;

    [Header("Data objects")]
    [SerializeField] private InternalDataScriptable internalData;
    [SerializeField] private SettingsDataScriptable settingsData;
    [SerializeField] private BrushDataScriptable sculptBrushData;
    [SerializeField] private PaintBrushDataScriptable paintBrushData;
    [SerializeField] private BrushDataScriptable stampBrushData;


    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject[] panels;
    [SerializeField] private GameObject terrainPanel;
    [SerializeField] private GameObject materialPanel;
    [SerializeField] private GameObject paintPanel;
    [SerializeField] private GameObject systemPanel;

    [SerializeField] private GameObject sidePanels;

    [Header("Lighting")]
    [SerializeField] private Light sun;
    [SerializeField] private Material skyMaterial;

    [Header("Water")]
    [SerializeField] private GameObject ocean;

    [Header("Terrain")]
    [SerializeField] private GameObject currentTerrain;

    public List<string> customTextures;

    private GameResources gameResources;
    private TerrainManager manager;
    private HeightmapController heightmapController;
    private MaterialController materialController;

    private float time;
    // Start is called before the first frame update

    //ensure the TerrainManager is set up before anything else tries to access it
    void Awake()
    {
        manager = TerrainManager.Instance();
        manager.TerrainObject = currentTerrain;
    }

    void Start()
    {
        heightmapController = manager.HeightmapController;
        materialController = manager.MaterialController;

        //PlayerPrefs.DeleteAll();
        //cache the instance of the GameResources object
        gameResources = GameResources.instance;

        //set up brush settings
        sculptBrushData.brushRadius = 50;
        sculptBrushData.brushStrength = 0.05f;
        sculptBrushData.brushRotation = 0;

        paintBrushData.textureScale = 1.0f;
        paintBrushData.useTexture = true;
        paintBrushData.color = Color.white;

        time = 0;

        internalData.customSculptBrushes = new List<string>();
        internalData.customStampBrushes = new List<string>();
        internalData.customPaintBrushes = new List<string>();
        internalData.customErosionBrushes = new List<string>();
        internalData.customSetHeightBrushes = new List<string>();
        internalData.customTextures = new List<string>();
        internalData.customMaterials = new List<string>();

        internalData.customMaterialIndices = new List<int>();
        internalData.customTextureIndices = new List<int>();
        internalData.customPaintBrushIndices = new List<int>();
        internalData.customSculptBrushIndices = new List<int>();
        internalData.customSetHeightBrushIndices = new List<int>();
        internalData.customStampBrushIndices = new List<int>();
        internalData.customErosionBrushIndices = new List<int>();

        InitialiseMainPanels();

        heightmapController.CreateFlatTerrain(settingsData.defaultTerrainResolution);

        LoadCustomBrushes();
        LoadCustomTextures();
        LoadCustomMaterials();

        InitialiseFlags();
    }

    private void InitialiseFlags()
    {
        internalData.ProcGenOpen = false;
        internalData.sliderChanged = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(internalData.windSpeed > 0) {
            float xmovement = internalData.windSpeed * Mathf.Sin(internalData.windDirection * Mathf.Deg2Rad) * time / 50000;
            float ymovement = internalData.windSpeed * Mathf.Cos(internalData.windDirection * Mathf.Deg2Rad) * time / 50000;
            skyMaterial.SetFloat("_XOffset", internalData.cloudXoffset + xmovement);
            skyMaterial.SetFloat("_YOffset", internalData.cloudYOffset + ymovement);       

            if(ocean.activeSelf)
                Ceto.Ocean.Instance.RenderReflection(ocean);

            time++;
        }        
    }

    public void DoExit()
    {
        SaveCustomBrushes();
        SaveCustomTextures();
        SaveCustomMaterials();

        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
        #endif    
    }

    public void SaveCustomBrushes()
    {                
        PlayerPrefs.SetInt("CustomBrushCount", internalData.customSculptBrushes.Count);

        if(internalData.customSculptBrushes.Count > 0) {
            for(int i = 0; i < internalData.customSculptBrushes.Count; i++)                
                PlayerPrefs.SetString("CustomBrush_" + i, internalData.customSculptBrushes[i]);
        }        

        PlayerPrefs.SetInt("CustomPaintBrushCount", internalData.customPaintBrushes.Count);

        if(internalData.customPaintBrushes.Count > 0) {
            for(int i = 0; i < internalData.customPaintBrushes.Count; i++)                
                PlayerPrefs.SetString("CustomPaintBrush_" + i, internalData.customPaintBrushes[i]);
        }        

        PlayerPrefs.SetInt("CustomErosionBrushCount", internalData.customErosionBrushes.Count);

        if(internalData.customErosionBrushes.Count > 0) {
            for(int i = 0; i < internalData.customErosionBrushes.Count; i++)                
                PlayerPrefs.SetString("CustomErosionBrush_" + i, internalData.customErosionBrushes[i]);
        }        

        PlayerPrefs.SetInt("CustomStampCount", internalData.customStampBrushes.Count);

        if(internalData.customStampBrushes.Count > 0) {
            for(int i = 0; i < internalData.customStampBrushes.Count; i++)                
                PlayerPrefs.SetString("CustomStamp_" + i, internalData.customStampBrushes[i]);
        }        

        PlayerPrefs.SetInt("CustomSetHeightBrushCount", internalData.customSetHeightBrushes.Count);

        if(internalData.customSetHeightBrushes.Count > 0) {
            for(int i = 0; i < internalData.customSetHeightBrushes.Count; i++)                
                PlayerPrefs.SetString("CustomSetHeightBrush_" + i, internalData.customSetHeightBrushes[i]);
        }        
    }

    public void LoadCustomBrushes()
    {
        int count = PlayerPrefs.GetInt("CustomBrushCount");

        if(count > 0) {
            for(int i = 0; i < count; i++) {
                string name = PlayerPrefs.GetString("CustomBrush_" + i);

                LoadCustomTerrainBrush(name, InternalDataScriptable.TerrainModes.Sculpt);
                internalData.customSculptBrushes.Add(name);
            }
        }
        
        count = PlayerPrefs.GetInt("CustomPaintBrushCount");

        if(count > 0) {
            for(int i = 0; i < count; i++) {
                string name = PlayerPrefs.GetString("CustomPaintBrush_" + i);

                LoadCustomPaintBrush(name);
                internalData.customPaintBrushes.Add(name);
            }
        }

        count = PlayerPrefs.GetInt("CustomErosionBrushCount");

        if(count > 0) {
            for(int i = 0; i < count; i++) {
                string name = PlayerPrefs.GetString("CustomErosionBrush_" + i);

                LoadCustomTerrainBrush(name, InternalDataScriptable.TerrainModes.Erode);
                internalData.customErosionBrushes.Add(name);
            }
        }

        count = PlayerPrefs.GetInt("CustomStampCount");

        if(count > 0) {
            for(int i = 0; i < count; i++) {
                string name = PlayerPrefs.GetString("CustomStamp_" + i);

                LoadCustomTerrainBrush(name, InternalDataScriptable.TerrainModes.Stamp);
                internalData.customStampBrushes.Add(name);
            }
        }

        count = PlayerPrefs.GetInt("CustomSetHeightBrushCount");

        if(count > 0) {
            for(int i = 0; i < count; i++) {
                string name = PlayerPrefs.GetString("CustomSetHeightBrush_" + i);

                LoadCustomTerrainBrush(name, InternalDataScriptable.TerrainModes.SetHeight);
                internalData.customSetHeightBrushes.Add(name);
            }
        }
    }

    public void SaveCustomTextures()
    {        
        PlayerPrefs.SetInt("CustomTextureCount", customTextures.Count);

        if(customTextures.Count > 0) {
            for(int i = 0; i < customTextures.Count; i++)
                PlayerPrefs.SetString("CustomTexture_" + i, customTextures[i]);
        }        
    }

    public void LoadCustomTextures()
    {
        int count = PlayerPrefs.GetInt("CustomTextureCount");

        if(count > 0) {
            for(int i = 0; i < count; i++) {
                string name = PlayerPrefs.GetString("CustomTexture_" + i);

                LoadCustomTexture(name);
                customTextures.Add(name);
            }
        }
    }

    public void SaveCustomMaterials()
    {        
        int matCount = 0;

        if(internalData.customMaterials.Count > 0) {
            for(int i = 0; i < internalData.customMaterials.Count; i++) {
                if(internalData.customMaterials[i] != "") {
                    PlayerPrefs.SetString("CustomMaterial_" + i, internalData.customMaterials[i]);
                    matCount++;
                }
            }
        }        

        PlayerPrefs.SetInt("CustomMaterialCount", matCount);
    }

    public void LoadCustomMaterials()
    {
        int count = PlayerPrefs.GetInt("CustomMaterialCount");

        if(count > 0) {
            for(int i = 0; i < count; i++) {
                string name = PlayerPrefs.GetString("CustomMaterial_" + i);

                LoadCustomMaterial(name);
                internalData.customMaterials.Add(name);
            }
        }
    }

    public void LoadCustomMaterial(string filename)
    {
        Material material = new Material(Shader.Find("Standard")); 
        byte[] bytes = File.ReadAllBytes(filename);

        Texture2D materialTexture = new Texture2D(128,128, TextureFormat.DXT5, false);
        materialTexture.filterMode = FilterMode.Bilinear;
        materialTexture.LoadImage(bytes);

        material.mainTexture = materialTexture;

        material.SetTexture("_OcclusionMap", blankAO);

        gameResources.materials.Add(material);

        materialPanel.GetComponent<IPanel>().AddButton(materialTexture);
    }

    public void LoadCustomTexture(string filename)
    {
        Texture2D texture = new Texture2D(128,128, TextureFormat.RGB24, false); 
        byte[] bytes = File.ReadAllBytes(filename);

        texture.filterMode = FilterMode.Trilinear;
        texture.LoadImage(bytes);

        gameResources.textures.Add(texture);

        //Add the brush to the  brush selection panel          
        paintPanel.GetComponent<IPanel>().AddButton(texture, 1);
    }

    public void LoadCustomTerrainBrush(string filename, InternalDataScriptable.TerrainModes mode = InternalDataScriptable.TerrainModes.Sculpt)
    {
        Texture2D texture = new Texture2D(128,128, TextureFormat.RGB24, false); 
        byte[] bytes = File.ReadAllBytes(filename);

        texture.filterMode = FilterMode.Trilinear;
        texture.LoadImage(bytes);

        if(mode == InternalDataScriptable.TerrainModes.Sculpt) {
            gameResources.brushes.Add(texture);
        } else if(mode == InternalDataScriptable.TerrainModes.SetHeight) {
            gameResources.setHeightBrushes.Add(texture);
        } else if(mode == InternalDataScriptable.TerrainModes.Stamp) {
            gameResources.stampBrushes.Add(texture);
        } else if(mode == InternalDataScriptable.TerrainModes.Erode) {
            gameResources.erosionBrushes.Add(texture);
        }

        //Add the brush to the  brush selection panel          
        terrainPanel.GetComponent<IPanel>().AddTerrainButton(texture, mode);
    }

    public void LoadCustomPaintBrush(string filename)
    {
        Texture2D texture = new Texture2D(128,128, TextureFormat.RGB24, false); 
        byte[] bytes = File.ReadAllBytes(filename);

        texture.filterMode = FilterMode.Trilinear;
        texture.LoadImage(bytes);

        gameResources.paintBrushes.Add(texture);

        //Add the brush to the  brush selection panel          
        paintPanel.GetComponent<IPanel>().AddButton(texture, 0);
    }
    
    private void CloseAllPanels()
    {
        ClosaAllMainPanels();
        CloseAllSidePanels();
    }

    private void ClosaAllMainPanels()
    {
        mainPanel.GetComponent<PanelController>().CloseAllPanels();
    }

    private void CloseAllSidePanels()
    {
        sidePanels.GetComponent<PanelController>().CloseAllPanels();
    }

    public void InitialiseMainPanels()
    {
        //Ensure that all panels are initialised at startup even if the panel wasn't active when the program was run
        //This is needed since the Start method is only called on active GameObjects which can cause errors due to
        //properties not being initialised in time
        for(int index = 0; index < panels.Length; index++) {
            panels[index].GetComponent<IPanel>().InitialisePanel();
        }
    }

    public void Reset() {
        heightmapController.CreateFlatTerrain(settingsData.defaultTerrainResolution);
        materialController.ClearOverlay();

        for(int index = 0; index < panels.Length; index++) {
            panels[index].GetComponent<IPanel>().ResetPanel();
        }

        cameraController.ResetCameras();

        internalData.unsavedChanges = false;

    }

    public GameObject[] GetPanels()
    {
        return panels;
    }
}

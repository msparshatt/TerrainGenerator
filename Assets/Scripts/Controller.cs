using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;

public class Controller : MonoBehaviour
{
    [SerializeField] private Texture blankAO;
    [SerializeField] private Texture2D busyCursor;

    [Header("Shaders")]
    [SerializeField] private ComputeShader textureShader;
    [SerializeField] private Shader materialShader;

    [Header("Data objects")]
    [SerializeField] private InternalDataScriptable internalData;
    [SerializeField] private SettingsDataScriptable settingsData;
    [SerializeField] private BrushDataScriptable sculptBrushData;
    [SerializeField] private BrushDataScriptable paintBrushData;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanels;
    [SerializeField] private GameObject sculptPanel;
    [SerializeField] private GameObject materialPanel;
    [SerializeField] private GameObject paintPanel;
    [SerializeField] private GameObject systemPanel;

    [SerializeField] private GameObject sidePanels;

    public List<string> customMaterials;
    public List<string> customBrushes;
    private int brushIndex;

    public List<string> customTextures;
    private int textureIndex;

    private GameResources gameResources;
    private TerrainManager manager;

    // Start is called before the first frame update
    void Start()
    {
        //cache the instance of the GameResources object
        gameResources = GameResources.instance;

        //cache an instant of the terrain manager
        manager = TerrainManager.instance;

        manager.SetupTerrain(settingsData, internalData, busyCursor, textureShader, materialShader);
        manager.CreateFlatTerrain();

        //set up brush settings
        sculptBrushData.brushRadius = 50;
        sculptBrushData.brushStrength = 0.05f;
        sculptBrushData.brushRotation = 0;
        sculptBrushData.textureScale = 1.0f;

        paintBrushData.brushRadius = 50;
        paintBrushData.brushStrength = 0.05f;
        paintBrushData.brushRotation = 0;
        paintBrushData.textureScale = 1.0f;

        customBrushes = new List<string>();
        LoadCustomBrushes();
        customTextures = new List<string>();
        LoadCustomTextures();
        customMaterials = new List<string>();
        LoadCustomMaterials();

        InitialiseFlags();

        InitialiseMainPanels();
        manager.ApplyTextures();
    }

    private void InitialiseFlags()
    {
        internalData.ProcGenOpen = false;
        internalData.sliderChanged = false;
    }

    // Update is called once per frame
    void Update()
    {
        
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
        PlayerPrefs.SetInt("CustomBrushCount", customBrushes.Count);

        if(customBrushes.Count > 0) {
            for(int i = 0; i < customBrushes.Count; i++)                
                PlayerPrefs.SetString("CustomBrush_" + i, customBrushes[i]);
        }        
    }

    public void LoadCustomBrushes()
    {
        int count = PlayerPrefs.GetInt("CustomBrushCount");

        if(count > 0) {
            for(int i = 0; i < count; i++) {
                string name = PlayerPrefs.GetString("CustomBrush_" + i);

                LoadCustomBrush(name);
                customBrushes.Add(name);
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

        if(customMaterials.Count > 0) {
            for(int i = 0; i < customMaterials.Count; i++) {
                if(customMaterials[i] != "") {
                    PlayerPrefs.SetString("CustomMaterial_" + i, customMaterials[i]);
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
                customMaterials.Add(name);
            }
        }
    }

    public void LoadCustomMaterial(string filename)
    {
        Material material = new Material(Shader.Find("Standard")); 
        byte[] bytes = File.ReadAllBytes(filename);

        Texture2D materialTexture = new Texture2D(218,128, TextureFormat.DXT5, false);
        materialTexture.filterMode = FilterMode.Bilinear;
        materialTexture.LoadImage(bytes);

        material.mainTexture = materialTexture;

        material.SetTexture("_OcclusionMap", blankAO);

        gameResources.materials.Add(material);

        materialPanel.GetComponent<MaterialsPanel>().AddButton(materialTexture);
    }

    public void LoadCustomTexture(string filename)
    {
        Texture2D texture = new Texture2D(128,128, TextureFormat.RGB24, false); 
        byte[] bytes = File.ReadAllBytes(filename);

        texture.filterMode = FilterMode.Trilinear;
        texture.LoadImage(bytes);

        gameResources.textures.Add(texture);

        //Add the brush to the  brush selection panel          
        paintPanel.GetComponent<PaintPanel>().AddTextureButton(texture);
    }

    public void LoadCustomBrush(string filename)
    {
        Texture2D texture = new Texture2D(128,128, TextureFormat.RGB24, false); 
        byte[] bytes = File.ReadAllBytes(filename);

        texture.filterMode = FilterMode.Trilinear;
        texture.LoadImage(bytes);

        gameResources.brushes.Add(texture);

        //Add the brush to the  brush selection panel          
        sculptPanel.GetComponent<SculptPanel>().AddButton(texture);
    }

    private void CloseAllPanels()
    {
        ClosaAllMainPanels();
        CloseAllSidePanels();
    }

    private void ClosaAllMainPanels()
    {
        mainPanels.GetComponent<PanelController>().CloseAllPanels();
    }

    private void CloseAllSidePanels()
    {
        sidePanels.GetComponent<PanelController>().CloseAllPanels();
    }

    public void InitialiseMainPanels()
    {
        materialPanel.GetComponent<MaterialsPanel>().InitialiseMaterialPanel();
        sculptPanel.GetComponent<SculptPanel>().InitialiseSculptPanel();
        paintPanel.GetComponent<PaintPanel>().InitialisePaintPanel();
        systemPanel.GetComponent<SystemPanel>().InitialiseSystemPanel();
    }

    public void Reset() {
        manager.CreateFlatTerrain();
        manager.ClearOverlay();

        sculptPanel.GetComponent<SculptPanel>().SelectBrushIcon(0);
        paintPanel.GetComponent<PaintPanel>().SelectBrushIcon(0);
        paintPanel.GetComponent<PaintPanel>().SelectTextureIcon(1);

        for(int index = 0; index < 5; index++) {
            materialPanel.GetComponent<MaterialsPanel>().SelectMaterialIcon(index, index);
        }

        for(int index = 1; index < 5; index++) {
            materialPanel.GetComponent<MaterialsPanel>().mixFactorSliders[index].value = 0;
            materialPanel.GetComponent<MaterialsPanel>().mixtypeDropdowns[index].value = 0;
        }

        internalData.unsavedChanges = false;

    }
}

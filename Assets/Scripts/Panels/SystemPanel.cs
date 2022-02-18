using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using UnityEngine.InputSystem;
using TMPro;

public class SystemPanel : MonoBehaviour
{

    [Header("UI elements")]
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private GameObject aboutPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingButton;
    [SerializeField] private GameObject exitConfirmationPanel;
    [SerializeField] private GameObject unsavedChangesText;
    [SerializeField] private GameObject saveChangesButton;
    [SerializeField] private GameObject proceduralPanel;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Texture blankAO;


    [Header("Export settings")]
    [SerializeField] private Dropdown scaleDropdown;
    [SerializeField] private Texture2D busyCursor;

    [Header("Data objects")]
    [SerializeField] private SettingsDataScriptable settingsData;
    [SerializeField] private InternalDataScriptable internalData;

    private TerrainManager manager;
    private string savefileName;
    private List<string> customMaterials;
    private List<string> customBrushes;
    private int brushIndex;

    private List<string> customTextures;
    private int textureIndex;

    // Start is called before the first frame update
    void Start()
    {
        manager = TerrainManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //new terrain panel
    public void FlatButtonClick()
    {
        proceduralPanel.SetActive(false);
        manager.CreateFlatTerrain();
        manager.ApplyTextures();
    }

    public void HeightmapButtonClick()
    {
        proceduralPanel.SetActive(false);
		FileBrowser.SetFilters( true, new FileBrowser.Filter( "heightmap files", ".png", ".raw"));
        FileBrowser.SetDefaultFilter( ".raw" );

        playerInput.enabled = false;
        FileBrowser.ShowLoadDialog((filenames) => {playerInput.enabled = true; manager.CreateTerrainFromHeightmap(filenames[0]);}, () => {playerInput.enabled = true; Debug.Log("Canceled Load");}, FileBrowser.PickMode.Files);
    }

    public void ProceduralButtonClick()
    {
        proceduralPanel.SetActive(!proceduralPanel.activeSelf);
        internalData.ProcGenOpen = true;

        if(proceduralPanel.activeSelf == false)
            proceduralPanel.GetComponent<ProceduralControlPanel>().CancelButtonClick();
    }


    //load/save panel
    public void LoadButtonClick()
    {
		FileBrowser.SetFilters( true, new FileBrowser.Filter( "Save files", ".json"));
        FileBrowser.SetDefaultFilter( ".json" );

        playerInput.enabled = false;
        FileBrowser.ShowLoadDialog((filenames) => {playerInput.enabled = true; OnLoad(filenames[0]);}, () => {playerInput.enabled = true; Debug.Log("Canceled Load");}, FileBrowser.PickMode.Files);
    }

    public void OnLoad(string filename)
    {
        if(filename != "") {
            Cursor.SetCursor(busyCursor, Vector2.zero, CursorMode.Auto); 

            savefileName = filename;
            //force the cursor to update
            Cursor.visible = false;
            Cursor.visible = true;

            var sr = new StreamReader(filename);
            string fileContents = sr.ReadToEnd();
            sr.Close();        

            Version data = JsonUtility.FromJson<Version>(fileContents);

            if(data.version <= 1) {
                Version1Load(fileContents);
            } else {
                Version2Load(fileContents);
            }

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }        
    }

    public void Version1Load(string fileContents)
    {
        SaveData_v1 data = JsonUtility.FromJson<SaveData_v1>(fileContents);
        manager.CreateTerrainFromHeightmap(data.heightmap);

        int materialPanelIndex = 0;
        int no_textures = 5;
        
        //fix
        if(data.baseTexture == -1) {
            //SelectMaterialIcon(0, AddBaseTexture(data.baseTexture_colors));
        } else {
            //SelectMaterialIcon(0, RemapTextureIndex(data.baseTexture));
        }

        if(data.baseTexture2 == -1) {
            //SelectMaterialIcon(1, AddBaseTexture(data.baseTexture2_colors));
        } else {
            //SelectMaterialIcon(1, RemapTextureIndex(data.baseTexture2));
        }

        manager.mixTypes[1] = data.mixType;
/*        if(data.mixType == 2)
            slopeToggles[1].isOn = true;
        else
            heightToggles[1].isOn = true;*/

        manager.mixFactors[1] = 1- data.mixFactor;
        //mixFactorSliders[1].value = 1 - data.mixFactor;

        for(int index = 2; index < 5; index++) {
            //SelectMaterialIcon(index, index);
            manager.mixTypes[index] = 1;
            //heightToggles[index].isOn = true;

            manager.mixFactors[index] = 0f;
            //mixFactorSliders[index].value = 0f;
        }

        if(data.tiling == 0)
            data.tiling = 1;

        //scaleSlider.value = data.tiling;

        if(data.paintTiling == 0)
            data.paintTiling = 1;
        //paintScaleSlider.value = data.paintTiling;

        //aoToggle.isOn = data.aoActive;
        
        Texture2D texture = new Texture2D(10,10);
        ImageConversion.LoadImage(texture, data.overlayTexture);

        manager.SetOverlay(texture);

        //OldSavePanel.SetActive(true);
    }

    public void UpdateOldSaveFile()
    {
        //backup save file
        File.Copy(savefileName, savefileName + ".bak");

        //save new version
        OnSave(savefileName, false);

        //OldSavePanel.SetActive(false);
    }

    public void DontUpdateOldSaveFile()
    {
        //OldSavePanel.SetActive(false);
    }

    public int RemapTextureIndex(int index)
    {
        int[] newIndices = {0, 1, 7, 8, 9, 17, 18, 25, 26, 27, 28, 29, 35, 36, 37, 44, 45, 52, 22, 69, 70, 56, 57, 53, 54};

        Debug.Log(index + " : " + newIndices[index]);
        return newIndices[index];
    }
    public void Version2Load(string fileContents)
    {
        SaveData_v2 data = JsonUtility.FromJson<SaveData_v2>(fileContents);
        manager.CreateTerrainFromHeightmap(data.heightmap);

        int materialPanelIndex = 0;
        int no_textures = 5;
        
        for(int index = 0; index < no_textures; index++) {
            if(data.baseTexture[index] == -1) {
            //    SelectMaterialIcon(index, AddBaseTexture(data.baseTexture_colors[index]));
            } else {
            //    SelectMaterialIcon(index, data.baseTexture[index]);
            }

            if(index > 0)
            {
                manager.mixTypes[index] = data.mixType[index];
/*                if(data.mixType[index] == 1)
                    heightToggles[index].isOn = true;
                else
                    slopeToggles[index].isOn = true;*/

                manager.mixFactors[index] = data.mixFactor[index];
                //mixFactorSliders[index].value = data.mixFactor[index];
            } else {
                manager.mixTypes[0] = 0;
                manager.mixFactors[0] = 0f;
            }
        }

        if(data.tiling == 0)
            data.tiling = 1;

        //scaleSlider.value = data.tiling;

        if(data.paintTiling == 0)
            data.paintTiling = 1;
        //paintScaleSlider.value = data.paintTiling;

        //aoToggle.isOn = data.aoActive;
        
        Texture2D texture = new Texture2D(10,10);
        ImageConversion.LoadImage(texture, data.overlayTexture);

        manager.SetOverlay(texture);
    }

        public void SaveButtonClick(bool exitOnSave = false)
    {
        Debug.Log("SAVE: Opening file browser");
		FileBrowser.SetFilters( false, new FileBrowser.Filter( "Save files", ".json"));

        playerInput.enabled = false;
        FileBrowser.ShowSaveDialog((filenames) => {playerInput.enabled = true; OnSave(filenames[0], exitOnSave);}, () => {playerInput.enabled = true; Debug.Log("Canceled save");}, FileBrowser.PickMode.Files);
    }

    public void OnSave(string filename, bool exitOnSave)
    {
        if(filename != null && filename != "") {
/*            savefileName = filename;
            Debug.Log("Saving to " + filename);
            Cursor.SetCursor(busyCursor, Vector2.zero, CursorMode.Auto); 

            //force the cursor to update
            Cursor.visible = false;
            Cursor.visible = true;

            Debug.Log("SAVE: Creating SaveData object");
            SaveData_v2 data = new SaveData_v2();
            Texture2D texture;

            data.version = 2;
            Debug.Log("SAVE: Store heightmap");
            data.heightmap = manager.GetHeightmapAsBytes();
            Debug.Log("SAVE: Store base textures");

            int no_textures = 5;
            data.baseTexture = new int[no_textures];
            data.baseTexture_colors = new byte[no_textures][];
            data.mixFactor = new float[no_textures];
            data.mixType = new int[no_textures];
            
            for(int index = 0; index < no_textures; index++) {
                if(currentMaterialIndices[index] >= (gameResources.materials.Count - customMaterials.Count)) {
                    data.baseTexture[index] = -1;
                    texture = (Texture2D)gameResources.materials[currentMaterialIndices[index]].mainTexture;
                    data.baseTexture_colors[index] = texture.EncodeToPNG();
                } else {
                    data.baseTexture[index] = currentMaterialIndices[index];
                    data.baseTexture_colors[index] = null;
                }

                data.mixType[index] = manager.mixTypes[index];
                data.mixFactor[index] = manager.mixFactors[index];
            }

            data.tiling = scaleSlider.value;
            data.aoActive = aoToggle.isOn;

            Debug.Log("SAVE: Store overlay texture");
            texture = manager.GetOverlay();
            data.overlayTexture = texture.EncodeToPNG();
            data.paintTiling = paintScaleSlider.value;

            Debug.Log("SAVE: Create json string");
            string json = JsonUtility.ToJson(data);

            Debug.Log("SAVE: Write to file");
            var sr = File.CreateText(filename);
            sr.WriteLine (json);
            sr.Close();
            Debug.Log("SAVE: Finish");

            if(exitOnSave)
                DoExit();*/
        }
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        internalData.unsavedChanges = false;
    }

    //export panel
    public void ExportButtonClick()
    {
        float scalefactor = 0.02f * Mathf.Pow(2, scaleDropdown.value); //reduce the size so it isn't too large for FlowScape
		FileBrowser.SetFilters( false, new FileBrowser.Filter( "Obj files", ".obj"));

        playerInput.enabled = false;
        FileBrowser.ShowSaveDialog((filenames) => {playerInput.enabled = true;  manager.ExportTerrainAsObj(filenames[0], internalData.ambientOcclusion, scalefactor);}, () => {playerInput.enabled = true; Debug.Log("Canceled save");}, FileBrowser.PickMode.Files);

        //exportTerrain.Export(aoToggle.isOn, scaleSlider.value);
    }

    public void ExportHmButtonClick()
    {
		FileBrowser.SetFilters( false, new FileBrowser.Filter( "Raw heightmap", ".raw"));

        playerInput.enabled = false;
        FileBrowser.ShowSaveDialog((filenames) => {playerInput.enabled = true;  manager.ExportTerrainAsRaw(filenames[0]);}, () => {playerInput.enabled = true; Debug.Log("Canceled save");}, FileBrowser.PickMode.Files);        
    }


    //other panel
    public void HelpButtonClick()
    {
        bool active = !helpPanel.activeSelf;
        CloseAllPanels();
        helpPanel.SetActive(active);
    }

    public void SettingsButtonClick()
    {
        bool active = !settingsPanel.activeSelf;
        CloseAllPanels();
        settingsPanel.SetActive(active);
    }

    public void AboutButtonClick()
    {
        aboutPanel.SetActive(true);
    }

    public void ResetButtonClick()
    {
        FlatButtonClick();
        /*ClearButtonClick();

        SelectBrushIcon(0);
        SelectMaterialIcon(0, 0);
        SelectMaterialIcon(1, 1);
        SelectMaterialIcon(2, 2);
        SelectMaterialIcon(3, 3);
        SelectMaterialIcon(4, 4);

        for(int index = 1; index < 5; index++) {
            mixFactorSliders[index].value = 0;
            mixtypeDropdowns[index].value = 0;
        }

        SelectTextureIcon(1);
    */
        internalData.unsavedChanges = false;
    }

    private void CloseAllPanels()
    {
/*        materialListPanel.SetActive(false);
        materialPanel.SetActive(false);
        brushPanel.SetActive(false);
        texturePanel.SetActive(false);
        helpPanel.SetActive(false);
        settingsPanel.SetActive(false);

        brushImage.color = deselectedColor;
        textureImage.color = deselectedColor;*/
    }

    public void ExitButtonClick()
    {
        exitConfirmationPanel.SetActive(true);
        unsavedChangesText.SetActive(internalData.unsavedChanges);
        saveChangesButton.SetActive(internalData.unsavedChanges);
        
    }

    public void NoButtonClick()
    {
        exitConfirmationPanel.SetActive(false);
    }

    public void YesButtonClick()
    {
        DoExit();
    }

    public void ExitPanelSaveClick()
    {
        SaveButtonClick(true);
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
        Texture2D tmpTexture = new Texture2D(128,128, TextureFormat.RGB24, false);
        byte[] bytes = File.ReadAllBytes(filename);

        Texture2D materialTexture = new Texture2D(tmpTexture.width,tmpTexture.height, TextureFormat.DXT1, false);
        materialTexture.filterMode = FilterMode.Bilinear;
        materialTexture.LoadImage(bytes);

        Debug.Log(materialTexture.format);
        material.mainTexture = materialTexture;

        material.SetTexture("_OcclusionMap", blankAO);

        //gameResources.materials.Add(material);

        //Add the brush to the  brush selection panel          
        GameObject newButton;
        int ObjectIndex = 0 ;// materialIcons.Count;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        //newButton = MakeButton(materialTexture, delegate {SelectMaterialIcon(ObjectIndex); }, ObjectIndex);
        //newButton.transform.SetParent(materialScrollView.transform);
        //materialIcons.Add(newButton);
    }

    public void LoadCustomTexture(string filename)
    {
        Texture2D texture = new Texture2D(128,128, TextureFormat.RGB24, false); 
        byte[] bytes = File.ReadAllBytes(filename);

        texture.filterMode = FilterMode.Trilinear;
        texture.LoadImage(bytes);

        //gameResources.textures.Add(texture);

        //Add the brush to the  brush selection panel          
        GameObject newButton;
        /*int ObjectIndex = textureIcons.Count;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = MakeButton(texture, delegate {SelectTextureIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(textureScrollView.transform);
        textureIcons.Add(newButton);*/
    }

    public void LoadCustomBrush(string filename)
    {
        Texture2D texture = new Texture2D(128,128, TextureFormat.RGB24, false); 
        byte[] bytes = File.ReadAllBytes(filename);

        texture.filterMode = FilterMode.Trilinear;
        texture.LoadImage(bytes);

        /*gameResources.brushes.Add(texture);

        //Add the brush to the  brush selection panel          
        GameObject newButton;
        int ObjectIndex = brushIcons.Count;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = MakeButton(texture, delegate {SelectBrushIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(brushScrollView.transform);
        brushIcons.Add(newButton);*/
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SimpleFileBrowser;
using UnityEngine.InputSystem;
using TMPro;


public class ControlPanel : MonoBehaviour
{
    private enum MixTypes  {Top = 1, Bottom, Steep, Shallow, Peaks, Valleys, Random};
    [Header("UI elements")]
    [SerializeField] private TextMeshProUGUI modeText;
    [SerializeField] private GameObject brushScrollView;
    [SerializeField] private GameObject brushPanel;
    [SerializeField] private Button brushDeleteButton;
    [SerializeField] private GameObject textureScrollView;
    [SerializeField] private GameObject texturePanel;
    [SerializeField] private Button textureDeleteButton;
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private RawImage brushImage;
    [SerializeField] private RawImage textureImage;
    [SerializeField] private Button textureButton;
    [SerializeField] private Slider scaleSlider;
    [SerializeField] private Toggle aoToggle;
    [SerializeField] private Slider paintScaleSlider;
    [SerializeField] private GameObject proceduralPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingButton;
    [SerializeField] private GameObject exitConfirmationPanel;
    [SerializeField] private GameObject unsavedChangesText;
    [SerializeField] private GameObject saveChangesButton;
    //[SerializeField] private Shader terrainShader;
    [SerializeField] private Texture blankAO;
    [SerializeField] private GameObject OldSavePanel;

    [SerializeField] private PlayerInput playerInput;

    [Header("Materials Panel")]
    [SerializeField] private GameObject materialScrollView;
    [SerializeField] private GameObject materialPanel;
    [SerializeField] private Button materialDeleteButton;

    [Header("Material List")]
    [SerializeField] private GameObject materialListPanel;
    [SerializeField] private RawImage[] materialImages;
    [SerializeField] private TMP_Dropdown[] mixtypeDropdowns;
    [SerializeField] private Slider[] mixFactorSliders;
    [SerializeField] private Slider[] offsetSliders;


    [Header("brush settings")]
    [SerializeField] private BrushDataScriptable brushData;

    [Header("Export settings")]
    [SerializeField] private Dropdown scaleDropdown;
    [SerializeField] private Texture2D busyCursor;

    [Header("Data objects")]
    [SerializeField] private SettingsDataScriptable settingsData;
    [SerializeField] private FlagsDataScriptable flagsData;


    [Header("Sprites")]
    [SerializeField] private Sprite selectedTabSprite;
    [SerializeField] private Sprite deselectedTabSprite;

    [Header("Shaders")]
    [SerializeField] private ComputeShader textureShader;
    [SerializeField] private Shader materialShader;

    private TerrainManager manager;
    private int[] currentMaterialIndices;
    private int materialPanelIndex;
    private List<string> customMaterials;

    //buttons created to select brushes,materials and textures
    private List<GameObject> brushIcons;
    private List<GameObject> materialIcons;
    private List<GameObject> textureIcons;
    private List<string> customBrushes;
    private int brushIndex;

    private List<string> customTextures;
    private int textureIndex;

    //UI colours
    private Color selectedColor;
    private Color deselectedColor;

    //assets from the resource folder used by the game
    private GameResources gameResources;

    //Stores the name of the current file
    private string savefileName;

    public void Start() 
    {
        //cache the instance of the GameResources object
        gameResources = GameResources.instance;

        //cache an instant of the terrain manager
        manager = TerrainManager.instance;

        selectedColor = Color.green;
        deselectedColor = Color.white;

        CloseAllPanels();

        //set up brush settings
        brushData.brushRadius = 50;
        brushData.brushStrength = 0.05f;
        brushData.brushRotation = 0;
        brushData.textureScale = 1.0f;

        //create selection panels
        SetupPanels();

        customBrushes = new List<string>();
        LoadCustomBrushes();
        customTextures = new List<string>();
        LoadCustomTextures();
        customMaterials = new List<string>();
        LoadCustomMaterials();

        currentMaterialIndices = new int[] {0,0,0,0,0};

        Debug.Log("creating terrain " + Time.realtimeSinceStartup);

        manager.SetupTerrain(settingsData, flagsData, busyCursor, textureShader, materialShader);
        manager.CreateFlatTerrain();

        //Debug.Log("loaded " + Time.realtimeSinceStartup);
        manager.doNotApply = true;
        SelectMaterialIcon(0, 0);
        SelectMaterialIcon(1, 1);
        SelectMaterialIcon(2, 2);
        SelectMaterialIcon(3, 3);
        SelectMaterialIcon(4, 4);
        manager.doNotApply = false;
        MixFactorSliderChange();
        MaterialDropdownSelect();

        SelectBrushIcon(0);
        SelectTextureIcon(1);
        SwitchMode(BrushDataScriptable.Modes.Sculpt);

        InitialiseFlags();
        //Debug.Log("end of start method " + Time.realtimeSinceStartup);
    }

    private void InitialiseFlags()
    {
        flagsData.ProcGenOpen = false;
        flagsData.sliderChanged = false;
    }

    private void SetupPanels()
    {
        //populate material selection panel          
        materialIcons = SetupIcons(gameResources.icons, materialScrollView.transform, SelectMaterialIcon);
        textureIcons = SetupIcons(gameResources.icons, textureScrollView.transform, SelectTextureIcon);
        brushIcons = SetupIcons(gameResources.brushes, brushScrollView.transform, SelectBrushIcon);
    }


    private List<GameObject> SetupIcons(List<Texture2D> images, Transform parent, Action<int> onClickFunction)
    {
        //populate material selection panel          
        GameObject newButton;
        List<GameObject> buttons = new List<GameObject>();
        int ObjectIndex = 0;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        foreach (Texture2D icon in images)
        {
            int oi = ObjectIndex; //need this to make sure the closure gets the right value
 
            newButton = MakeButton(icon, delegate {onClickFunction(oi); }, oi);
            newButton.transform.SetParent(parent);
            buttons.Add(newButton);
            ObjectIndex++;
        }

        return buttons;
    }

    //create an image button. It will call the passed onClickListener action when clicked
    private GameObject MakeButton(Texture2D icon, UnityAction onClickListener, int index=0)
    {
            GameObject NewObj = new GameObject("button" + index); //Create the GameObject
            Image NewImage = NewObj.AddComponent<Image>(); //Add the Image Component script
            NewImage.rectTransform.sizeDelta = new Vector2(50, 50);
            NewImage.sprite = Sprite.Create(icon, new Rect(0,0,icon.width,icon.height), new Vector2()); //Set the Sprite of the Image Component on the new GameObject

            Button NewButton = NewObj.AddComponent<Button>();
            NewButton.onClick.AddListener(onClickListener);

            NewObj.SetActive(true); //Activate the GameObject    

            return NewObj;
    }    

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
        flagsData.ProcGenOpen = true;

        if(proceduralPanel.activeSelf == false)
            proceduralPanel.GetComponent<ProceduralControlPanel>().CancelButtonClick();
    }

    public void RadiusSliderChange(float value)
    {
        brushData.brushRadius = (int)value;
    }

    public void StrengthSliderChange(float value)
    {
        brushData.brushStrength = value;
    }

    public void RotationSliderChange(float value)
    {
        brushData.brushRotation = value;
    }

    public void ExitButtonClick()
    {
        exitConfirmationPanel.SetActive(true);
        unsavedChangesText.SetActive(flagsData.unsavedChanges);
        saveChangesButton.SetActive(flagsData.unsavedChanges);
        
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

    public void BrushButtonClick()
    {
        bool active = !brushPanel.activeSelf;
        CloseAllPanels();
        brushPanel.SetActive(active);

        if(active)
            brushImage.color = selectedColor;
    }

    public void OpenMaterialListPanelButtonClick()
    {
        bool active = !materialListPanel.activeSelf;

        CloseAllPanels();
        materialListPanel.SetActive(active);
    }
    public void MaterialButtonClick(int index)
    {
        bool active = !materialPanel.activeSelf;
        if(index != materialPanelIndex)
            active = true;

        CloseAllPanels();
        materialPanel.SetActive(active);
        materialListPanel.SetActive(true);
        materialPanelIndex = index;

        for(int i = 0; i < 5; i++) {
            materialImages[i].color = deselectedColor;
        }

        if(active) {
            materialImages[index].color = selectedColor;

            for (int i = 0; i < materialIcons.Count; i++) {
                if(i == currentMaterialIndices[materialPanelIndex]) {
                    materialIcons[i].GetComponent<Image>().color = Color.green;

                } else {
                    materialIcons[i].GetComponent<Image>().color = Color.white;
                }
            }
        }
    }

    public void MaterialPanelButtonClick(int index)
    {
        materialPanelIndex = index;

        for (int i = 0; i < materialIcons.Count; i++) {
            if(i == currentMaterialIndices[materialPanelIndex]) {
                materialIcons[i].GetComponent<Image>().color = Color.green;

            } else {
                materialIcons[i].GetComponent<Image>().color = Color.white;
            }
        }
    }

    public void RegenerateButtonClick()
    {
        manager.ApplyTextures();
    }

    public void TextureButtonClick()
    {
        bool active = !texturePanel.activeSelf;
        CloseAllPanels();
        texturePanel.SetActive(active);

        if(active)
            textureImage.color = selectedColor;
    }

    public void SettingsButtonClick()
    {
        bool active = !settingsPanel.activeSelf;
        CloseAllPanels();
        settingsPanel.SetActive(active);
    }

    public void HelpButtonClick()
    {
        bool active = !helpPanel.activeSelf;
        CloseAllPanels();
        helpPanel.SetActive(active);
    }

    public void SelectBrushIcon(int buttonIndex)
    {
        brushData.brush = gameResources.brushes[buttonIndex];
        brushImage.texture = brushData.brush;
        brushIndex = buttonIndex;

        if(buttonIndex >= (gameResources.brushes.Count - customBrushes.Count)) {
            brushDeleteButton.interactable = true;
        } else {
            brushDeleteButton.interactable = false;
        }        

        for (int i = 0; i < brushIcons.Count; i++) {
            if(i == buttonIndex) {
                brushIcons[i].GetComponent<Image>().color = selectedColor;

            } else {
                brushIcons[i].GetComponent<Image>().color = deselectedColor;
            }
        }
    }

    public void SelectTextureIcon(int buttonIndex)
    {
        brushData.paintTexture = (Texture2D)gameResources.textures[buttonIndex];
        textureImage.texture = gameResources.icons[buttonIndex];
        textureIndex = buttonIndex;

        if(buttonIndex >= (gameResources.icons.Count - customTextures.Count)) {
            textureDeleteButton.interactable = true;
        } else {
            textureDeleteButton.interactable = false;
        }        

        for (int i = 0; i < textureIcons.Count; i++) {
            if(i == buttonIndex) {
                textureIcons[i].GetComponent<Image>().color = Color.green;

            } else {
                textureIcons[i].GetComponent<Image>().color = Color.white;
            }
        }
    }

    public void MaterialDropdownSelect()
    {
        bool oldDetect = flagsData.detectMaximaAndMinima;
        flagsData.detectMaximaAndMinima = false;
        for(int i = 1; i < 5; i++) {
            int mixType = mixtypeDropdowns[i].value + 1;

            if(mixType == (int)MixTypes.Peaks || mixType == (int)MixTypes.Valleys)
                flagsData.detectMaximaAndMinima = true;

            offsetSliders[i].gameObject.SetActive((mixType == (int)MixTypes.Random));
            manager.SetMixType(i, mixType);
        }

        if(!oldDetect && flagsData.detectMaximaAndMinima)
            manager.FindMaximaAndMinima();
            
        manager.ApplyTextures();
    }

    public void MixFactorSliderChange()
    {
        for(int i = 1; i < 5; i++) {
            manager.SetMixFactor(i, mixFactorSliders[i].value);
        }

        flagsData.sliderChanged = true;
    }

    public void OffsetSliderChange()
    {
        for(int i = 1; i < 5; i++) {
            manager.SetOffset(i, offsetSliders[i].value);
        }

        flagsData.sliderChanged = true;
    }

    public void SelectMaterialIcon(int panel, int buttonIndex)
    {
        materialPanelIndex = panel;
        SelectMaterialIcon(buttonIndex);
    }
    public void SelectMaterialIcon(int buttonIndex)
    {        
        Material mat = gameResources.materials[buttonIndex];
        currentMaterialIndices[materialPanelIndex] = buttonIndex;

        Vector2 scale = new Vector2(scaleSlider.value, scaleSlider.value);
        mat.mainTextureScale = scale;

        if(buttonIndex >= (gameResources.materials.Count - customMaterials.Count)) {
            materialDeleteButton.interactable = true;
        } else {
            materialDeleteButton.interactable = false;
        }        

        materialImages[materialPanelIndex].texture = gameResources.icons[buttonIndex];

        manager.SetBaseMaterials(materialPanelIndex, mat);

        for (int i = 0; i < materialIcons.Count; i++) {
            if(i == buttonIndex) {
                materialIcons[i].GetComponent<Image>().color = Color.green;

            } else {
                materialIcons[i].GetComponent<Image>().color = Color.white;
            }
        }
    }

    public void ScaleSliderChange(float value)
    {
        
        flagsData.sliderChanged = true;
        manager.ScaleMaterial(value);
    }

    public void ResetTilingButtonClick()
    {
        scaleSlider.value = 1.0f;
        //Camera.main.GetComponent<CameraController>().sliderChanged = true;

        manager.ApplyTextures();
    }

    public void PaintScaleSliderChange(float value)
    {
        brushData.textureScale = value;
    }

    public void PaintResetTilingButtonClick()
    {
        paintScaleSlider.value = 1.0f;
    }

    public void SwitchMode(BrushDataScriptable.Modes newMode)
    {
        brushData.brushMode = newMode;

        if(newMode == BrushDataScriptable.Modes.Sculpt) {
            modeText.text = "Sculpt";
            textureButton.interactable = false;
        } else {
            modeText.text = "Paint";
            textureButton.interactable = true;
        }
    }

    public void ModeButtonClick()
    {
        if(brushData.brushMode == BrushDataScriptable.Modes.Sculpt) {
            SwitchMode(BrushDataScriptable.Modes.Paint);
        } else {
            SwitchMode(BrushDataScriptable.Modes.Sculpt);
        }
    }

    public void ClearButtonClick()
    {
        manager.ClearOverlay();
    }

    private void CloseAllPanels()
    {
        materialListPanel.SetActive(false);
        materialPanel.SetActive(false);
        brushPanel.SetActive(false);
        texturePanel.SetActive(false);
        helpPanel.SetActive(false);
        settingsPanel.SetActive(false);

        brushImage.color = deselectedColor;
        textureImage.color = deselectedColor;
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
            savefileName = filename;
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
                DoExit();
        }
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        flagsData.unsavedChanges = false;
    }

    public void LoadButtonClick()
    {
		FileBrowser.SetFilters( true, new FileBrowser.Filter( "Save files", ".json"));
        FileBrowser.SetDefaultFilter( ".json" );

        playerInput.enabled = false;
        FileBrowser.ShowLoadDialog((filenames) => {playerInput.enabled = true; OnLoad(filenames[0]);}, () => {playerInput.enabled = true; Debug.Log("Canceled Load");}, FileBrowser.PickMode.Files);
    }

    public void OnLoad(String filename)
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

        materialPanelIndex = 0;
        int no_textures = 5;
        
        if(data.baseTexture == -1) {
            SelectMaterialIcon(0, AddBaseTexture(data.baseTexture_colors));
        } else {
            SelectMaterialIcon(0, RemapTextureIndex(data.baseTexture));
        }

        if(data.baseTexture2 == -1) {
            SelectMaterialIcon(1, AddBaseTexture(data.baseTexture2_colors));
        } else {
            SelectMaterialIcon(1, RemapTextureIndex(data.baseTexture2));
        }

        manager.mixTypes[1] = data.mixType;
/*        if(data.mixType == 2)
            slopeToggles[1].isOn = true;
        else
            heightToggles[1].isOn = true;*/

        manager.mixFactors[1] = 1- data.mixFactor;
        mixFactorSliders[1].value = 1 - data.mixFactor;

        for(int index = 2; index < 5; index++) {
            SelectMaterialIcon(index, index);
            manager.mixTypes[index] = 1;
            //heightToggles[index].isOn = true;

            manager.mixFactors[index] = 0f;
            mixFactorSliders[index].value = 0f;
        }

        if(data.tiling == 0)
            data.tiling = 1;

        scaleSlider.value = data.tiling;

        if(data.paintTiling == 0)
            data.paintTiling = 1;
        paintScaleSlider.value = data.paintTiling;

        aoToggle.isOn = data.aoActive;
        
        Texture2D texture = new Texture2D(10,10);
        ImageConversion.LoadImage(texture, data.overlayTexture);

        manager.SetOverlay(texture);

        OldSavePanel.SetActive(true);
    }

    public void UpdateOldSaveFile()
    {
        //backup save file
        File.Copy(savefileName, savefileName + ".bak");

        //save new version
        OnSave(savefileName, false);

        OldSavePanel.SetActive(false);
    }

    public void DontUpdateOldSaveFile()
    {
        OldSavePanel.SetActive(false);
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

        materialPanelIndex = 0;
        int no_textures = 5;
        
        for(int index = 0; index < no_textures; index++) {
            if(data.baseTexture[index] == -1) {
                SelectMaterialIcon(index, AddBaseTexture(data.baseTexture_colors[index]));
            } else {
                SelectMaterialIcon(index, data.baseTexture[index]);
            }

            if(index > 0)
            {
                manager.mixTypes[index] = data.mixType[index];
/*                if(data.mixType[index] == 1)
                    heightToggles[index].isOn = true;
                else
                    slopeToggles[index].isOn = true;*/

                manager.mixFactors[index] = data.mixFactor[index];
                mixFactorSliders[index].value = data.mixFactor[index];
            } else {
                manager.mixTypes[0] = 0;
                manager.mixFactors[0] = 0f;
            }
        }

        if(data.tiling == 0)
            data.tiling = 1;

        scaleSlider.value = data.tiling;

        if(data.paintTiling == 0)
            data.paintTiling = 1;
        paintScaleSlider.value = data.paintTiling;

        aoToggle.isOn = data.aoActive;
        
        Texture2D texture = new Texture2D(10,10);
        ImageConversion.LoadImage(texture, data.overlayTexture);

        manager.SetOverlay(texture);
    }

    private int AddBaseTexture(byte[] pixels)
    {
        Texture2D colorTexture = new Texture2D(10,10);
        ImageConversion.LoadImage(colorTexture, pixels);

        int index = -1;
        Hash128 textureHash = new Hash128();
        textureHash.Append(colorTexture.GetPixels());
        string hashString = textureHash.ToString();

        for(int i = (gameResources.materials.Count - customMaterials.Count); i < gameResources.materials.Count; i++) {
            Hash128 newHash = new Hash128();
            Texture2D tex = (Texture2D)(gameResources.materials[i].mainTexture);
            newHash.Append(tex.GetPixels());

            if(hashString == newHash.ToString())
                index = i;
        }

        if(index == -1) {
            //if the material doesn't exist add it as a new one
            Material material = new Material(Shader.Find("Standard")); 
            material.mainTexture = colorTexture;

            Texture2D newTexture = new Texture2D(colorTexture.width, colorTexture.height);                    

            gameResources.materials.Add(material);

            //Add the brush to the  brush selection panel          
            GameObject newButton;
            int ObjectIndex = materialIcons.Count;
            Vector2 scale = new Vector2(1.0f, 1.0f);

            newButton = MakeButton(colorTexture, delegate {SelectMaterialIcon(ObjectIndex); }, ObjectIndex);
            newButton.transform.SetParent(materialScrollView.transform);
            materialIcons.Add(newButton);

            customMaterials.Add("");

            index = gameResources.materials.Count - 1;
        }

        return index;
    }

    public void ExportButtonClick()
    {
        float scalefactor = 0.02f * Mathf.Pow(2, scaleDropdown.value); //reduce the size so it isn't too large for FlowScape
		FileBrowser.SetFilters( false, new FileBrowser.Filter( "Obj files", ".obj"));

        playerInput.enabled = false;
        FileBrowser.ShowSaveDialog((filenames) => {playerInput.enabled = true;  manager.ExportTerrainAsObj(filenames[0], aoToggle.isOn, scalefactor);}, () => {playerInput.enabled = true; Debug.Log("Canceled save");}, FileBrowser.PickMode.Files);

        //exportTerrain.Export(aoToggle.isOn, scaleSlider.value);
    }

    public void ExportHmButtonClick()
    {
		FileBrowser.SetFilters( false, new FileBrowser.Filter( "Raw heightmap", ".raw"));

        playerInput.enabled = false;
        FileBrowser.ShowSaveDialog((filenames) => {playerInput.enabled = true;  manager.ExportTerrainAsRaw(filenames[0]);}, () => {playerInput.enabled = true; Debug.Log("Canceled save");}, FileBrowser.PickMode.Files);        
    }

    public void AOToggleChange(bool isOn)
    {
        manager.SetAO(isOn);
    }

    public void BrushImportButtonclick()
    {
		FileBrowser.SetFilters( true, new FileBrowser.Filter( "Image files", ".png"));
        FileBrowser.SetDefaultFilter( ".png" );

        playerInput.enabled = false;
        FileBrowser.ShowLoadDialog((filenames) => {playerInput.enabled = true;  OnBrushImport(filenames[0]);}, () => {playerInput.enabled = true; Debug.Log("Canceled Load");}, FileBrowser.PickMode.Files);
    }

    public void OnBrushImport(String filename)
    {      
        if(filename != "") {
            LoadCustomBrush(filename);
            customBrushes.Add(filename);

            SelectBrushIcon(gameResources.brushes.Count - 1);
        }
    }

    public void BrushDeleteButtonClick()
    {
        int customBrushIndex = brushIndex + customBrushes.Count - gameResources.brushes.Count;

        customBrushes.RemoveAt(customBrushIndex);
        gameResources.brushes.RemoveAt(brushIndex);
        Destroy(brushIcons[brushIndex]);
        brushIcons.RemoveAt(brushIndex);
        
        SelectBrushIcon(0);
    }

    public void LoadCustomBrush(string filename)
    {
        Texture2D texture = new Texture2D(128,128, TextureFormat.RGB24, false); 
        byte[] bytes = File.ReadAllBytes(filename);

        texture.filterMode = FilterMode.Trilinear;
        texture.LoadImage(bytes);

        gameResources.brushes.Add(texture);

        //Add the brush to the  brush selection panel          
        GameObject newButton;
        int ObjectIndex = brushIcons.Count;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = MakeButton(texture, delegate {SelectBrushIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(brushScrollView.transform);
        brushIcons.Add(newButton);
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

    public void TextureImportButtonClick()
    {
		FileBrowser.SetFilters( true, new FileBrowser.Filter( "Image files", ".png"));
        FileBrowser.SetDefaultFilter( ".png" );

        playerInput.enabled = false;
        FileBrowser.ShowLoadDialog((filenames) => {playerInput.enabled = true;  OnTextureImport(filenames[0]);}, () => {playerInput.enabled = true; Debug.Log("Canceled Load");}, FileBrowser.PickMode.Files);
    }

    public void OnTextureImport(String filename)        
    {       
        if(filename != "") {
            LoadCustomTexture(filename);
            customTextures.Add(filename);

            SelectTextureIcon(gameResources.textures.Count - 1);
        }
    }

    public void TextureDeleteButtonClick()
    {
        int customTextureIndex = textureIndex + customTextures.Count - gameResources.textures.Count;

        customTextures.RemoveAt(customTextureIndex);
        gameResources.textures.RemoveAt(textureIndex);
        Destroy(textureIcons[textureIndex]);
        textureIcons.RemoveAt(textureIndex);
        
        SelectTextureIcon(0);
    }

    public void LoadCustomTexture(string filename)
    {
        Texture2D texture = new Texture2D(128,128, TextureFormat.RGB24, false); 
        byte[] bytes = File.ReadAllBytes(filename);

        texture.filterMode = FilterMode.Trilinear;
        texture.LoadImage(bytes);

        gameResources.textures.Add(texture);

        //Add the brush to the  brush selection panel          
        GameObject newButton;
        int ObjectIndex = textureIcons.Count;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = MakeButton(texture, delegate {SelectTextureIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(textureScrollView.transform);
        textureIcons.Add(newButton);
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

    public void MaterialImportButtonClick()
    {
		FileBrowser.SetFilters( true, new FileBrowser.Filter( "Image files", ".png"));
        FileBrowser.SetDefaultFilter( ".png" );

        playerInput.enabled = false;
        FileBrowser.ShowLoadDialog((filenames) => {playerInput.enabled = true;  OnMaterialImport(filenames[0]);}, () => {playerInput.enabled = true; Debug.Log("Canceled Load");}, FileBrowser.PickMode.Files);
    }

    public void OnMaterialImport(string filename)
    {
        if(filename != "") {
            LoadCustomMaterial(filename);
            customMaterials.Add(filename);

            SelectMaterialIcon(gameResources.materials.Count - 1);
        }
    }

    public void MaterialDeleteButtonClick()
    {
        int customMaterialIndex = currentMaterialIndices[materialPanelIndex] + customMaterials.Count - gameResources.materials.Count;

        customMaterials.RemoveAt(customMaterialIndex);
        gameResources.materials.RemoveAt(currentMaterialIndices[0]);
        Destroy(materialIcons[currentMaterialIndices[0]]);
        materialIcons.RemoveAt(currentMaterialIndices[0]);
        
        SelectMaterialIcon(0);
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

        gameResources.materials.Add(material);

        //Add the brush to the  brush selection panel          
        GameObject newButton;
        int ObjectIndex = materialIcons.Count;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = MakeButton(materialTexture, delegate {SelectMaterialIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(materialScrollView.transform);
        materialIcons.Add(newButton);
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

    public void ResetButtonClick()
    {
        FlatButtonClick();
        ClearButtonClick();

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

        flagsData.unsavedChanges = false;
    }
}

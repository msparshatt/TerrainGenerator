using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SimpleFileBrowser;
using UnityEngine.InputSystem;


public class ControlPanel : MonoBehaviour
{
    [Header("UI elements")]
    [SerializeField] private Text modeText;
    [SerializeField] private GameObject brushScrollView;
    [SerializeField] private GameObject brushPanel;
    [SerializeField] private Button brushDeleteButton;
    [SerializeField] private GameObject textureScrollView;
    [SerializeField] private GameObject texturePanel;
    [SerializeField] private Button textureDeleteButton;
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private RawImage materialImage;
    [SerializeField] private RawImage material2Image;
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
    [SerializeField] private Shader terrainShader;

    [SerializeField] private PlayerInput playerInput;

    [Header("Materials Panel")]
    [SerializeField] private Toggle singeMatToggle;
    [SerializeField] private Toggle heightToggle;
    [SerializeField] private Toggle slopeToggle;
    [SerializeField] private Slider mixFactorSlider;
    [SerializeField] private Button panel1Button;
    [SerializeField] private Button panel2Button;
    [SerializeField] private GameObject materialScrollView;
    [SerializeField] private GameObject materialPanel;
    [SerializeField] private Button materialDeleteButton;


    [Header("brush settings")]
    [SerializeField] private BrushDataScriptable brushData;

    [Header("Export settings")]
    [SerializeField] private Dropdown scaleDropdown;
    [SerializeField] private Texture2D busyCursor;

    [Header("settings")]
    [SerializeField] private SettingsDataScriptable settingsData;


    [Header("Sprites")]
    [SerializeField] private Sprite selectedTabSprite;
    [SerializeField] private Sprite deselectedTabSprite;

    [Header("Shaers")]
    [SerializeField] private ComputeShader textureShader;

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
        brushData.textureScale = 1.0f;

        //create selection panels
        SetupPanels();

        customBrushes = new List<string>();
        LoadCustomBrushes();
        customTextures = new List<string>();
        LoadCustomTextures();
        customMaterials = new List<string>();
        LoadCustomMaterials();

        currentMaterialIndices = new int[] {0,0};

        Debug.Log("creating terrain " + Time.realtimeSinceStartup);

        manager.SetupTerrain(settingsData, busyCursor, textureShader);
        manager.CreateFlatTerrain();

        //Debug.Log("loaded " + Time.realtimeSinceStartup);
        materialPanelIndex = 0;
        SelectMaterialIcon(0);
        materialPanelIndex = 1;
        SelectMaterialIcon(1);
        SelectBrushIcon(0);
        SelectTextureIcon(1);
        SwitchMode(BrushDataScriptable.Modes.Sculpt);

        //Debug.Log("end of start method " + Time.realtimeSinceStartup);
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
        TerrainManager.instance.CreateFlatTerrain();
    }

    public void HeightmapButtonClick()
    {
        proceduralPanel.SetActive(false);
		FileBrowser.SetFilters( true, new FileBrowser.Filter( "heightmap files", ".png", ".raw"));
        FileBrowser.SetDefaultFilter( ".raw" );

        playerInput.enabled = false;
        FileBrowser.ShowLoadDialog((filenames) => {playerInput.enabled = true; TerrainManager.instance.CreateTerrainFromHeightmap(filenames[0]);}, () => {playerInput.enabled = true; Debug.Log("Canceled Load");}, FileBrowser.PickMode.Files);
    }

    public void ProceduralButtonClick()
    {
        proceduralPanel.SetActive(!proceduralPanel.activeSelf);

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

    public void ExitButtonClick()
    {
        exitConfirmationPanel.SetActive(true);
    }

    public void NoButtonClick()
    {
        exitConfirmationPanel.SetActive(false);
    }

    public void YesButtonClick()
    {
        DoExit();
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

    public void MaterialButtonClick(int index)
    {
        bool active = !materialPanel.activeSelf;
        if(index != materialPanelIndex)
            active = true;

        CloseAllPanels();
        materialPanel.SetActive(active);
        materialPanelIndex = index;

        if(active) {
            if(index == 0) {
                materialImage.color = selectedColor;    
                panel1Button.GetComponent<Image>().sprite = selectedTabSprite;             
                panel2Button.GetComponent<Image>().sprite = deselectedTabSprite;             
            } else {
                material2Image.color = selectedColor;
                panel1Button.GetComponent<Image>().sprite = deselectedTabSprite;             
                panel2Button.GetComponent<Image>().sprite = selectedTabSprite;             
            }

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

        if(index == 0) {
            materialImage.color = selectedColor;  
            material2Image.color = deselectedColor;  
            panel1Button.GetComponent<Image>().sprite = selectedTabSprite;             
            panel2Button.GetComponent<Image>().sprite = deselectedTabSprite;             
        } else {
            materialImage.color = deselectedColor;  
            material2Image.color = selectedColor;
            panel1Button.GetComponent<Image>().sprite = deselectedTabSprite;             
            panel2Button.GetComponent<Image>().sprite = selectedTabSprite;             
        }

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
        textureImage.texture = brushData.paintTexture;
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

    public void MaterialToggleSelect(int index)
    {
        manager.SetMixType(index);
    }

    public void MixFactorSliderChange(float value)
    {
        manager.mixFactor = value;
        manager.ApplyTextures();        
    }

    public void SelectMaterialIcon(int buttonIndex)
    {        
        Material mat = gameResources.materials[buttonIndex];
        currentMaterialIndices[materialPanelIndex] = buttonIndex;

        Vector2 scale = new Vector2(scaleSlider.value, scaleSlider.value);
        mat.mainTextureScale = scale;

        if(mat.GetTexture("_AOTexture") == null) {
            aoToggle.interactable = false;            
        } else {
            aoToggle.interactable = true;

            AOToggleChange(aoToggle.isOn);
        }

        if(buttonIndex >= (gameResources.materials.Count - customMaterials.Count)) {
            materialDeleteButton.interactable = true;
        } else {
            materialDeleteButton.interactable = false;
        }        

        if(materialPanelIndex == 0)
            materialImage.texture = mat.mainTexture;
        else
            material2Image.texture = mat.mainTexture;

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
        manager.ScaleMaterial(value);
    }

    public void ResetTilingButtonClick()
    {
        scaleSlider.value = 1.0f;
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
        materialPanel.SetActive(false);
        brushPanel.SetActive(false);
        texturePanel.SetActive(false);
        helpPanel.SetActive(false);
        settingsPanel.SetActive(false);

        brushImage.color = deselectedColor;
        materialImage.color = deselectedColor;
        material2Image.color = deselectedColor;
        textureImage.color = deselectedColor;
    }

    public void SaveButtonClick()
    {
        Debug.Log("SAVE: Opening file browser");
		FileBrowser.SetFilters( false, new FileBrowser.Filter( "Save files", ".json"));

        playerInput.enabled = false;
        FileBrowser.ShowSaveDialog((filenames) => {playerInput.enabled = true; OnSave(filenames[0]);}, () => {playerInput.enabled = true; Debug.Log("Canceled save");}, FileBrowser.PickMode.Files);
    }

    public void OnSave(string filename)
    {
        if(filename != null && filename != "") {
            Debug.Log("Saving to " + filename);
            Cursor.SetCursor(busyCursor, Vector2.zero, CursorMode.Auto); 

            //force the cursor to update
            Cursor.visible = false;
            Cursor.visible = true;

            Debug.Log("SAVE: Creating SaveData object");
            SaveData data = new SaveData();
            Texture2D texture;

            data.version = 1;
            Debug.Log("SAVE: Store heightmap");
            data.heightmap = manager.GetHeightmapAsBytes();
            Debug.Log("SAVE: Store base textures");
            if(currentMaterialIndices[0] >= (gameResources.materials.Count - customMaterials.Count)) {
                data.baseTexture = -1;
                texture = (Texture2D)gameResources.materials[currentMaterialIndices[0]].mainTexture;
                data.baseTexture_colors = texture.EncodeToPNG();
            } else {
                data.baseTexture = currentMaterialIndices[0];
                data.baseTexture_colors = null;
            }
            if(currentMaterialIndices[1] >= (gameResources.materials.Count - customMaterials.Count)) {
                data.baseTexture2 = -1;
                texture = (Texture2D)gameResources.materials[currentMaterialIndices[1]].mainTexture;
                data.baseTexture2_colors = texture.EncodeToPNG();
            } else {
                data.baseTexture2 = currentMaterialIndices[1];
                data.baseTexture2_colors = null;
            }

            data.mixType = manager.mixType;
            data.mixFactor = manager.mixFactor;
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

        }
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
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

            //force the cursor to update
            Cursor.visible = false;
            Cursor.visible = true;

            var sr = new StreamReader(filename);
            string fileContents = sr.ReadToEnd();
            sr.Close();        

            SaveData data = JsonUtility.FromJson<SaveData>(fileContents);

            manager.CreateTerrainFromHeightmap(data.heightmap);

            materialPanelIndex = 0;
            if(data.baseTexture == -1) {
                SelectMaterialIcon(AddBaseTexture(data.baseTexture_colors));
            } else {
                SelectMaterialIcon(data.baseTexture);
            }

            materialPanelIndex = 1;
            if(data.baseTexture2 == 0) {
                SelectMaterialIcon(data.baseTexture);
            } else if(data.baseTexture2 == -1) {
                SelectMaterialIcon(AddBaseTexture(data.baseTexture2_colors));
            } else {
                SelectMaterialIcon(data.baseTexture2);
            }

            if(data.mixType == 0) {
                singeMatToggle.isOn = true;
            } else if (data.mixType == 1) {
                heightToggle.isOn = true;
            } else {
                slopeToggle.isOn = true;
            }

            if(data.mixFactor == 0)
                data.mixFactor = 0.5f;

            mixFactorSlider.value = data.mixFactor;

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

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }        
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
            Material material = new Material(terrainShader); 
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
        Material material = new Material(terrainShader); 
        Texture2D materialTexture = new Texture2D(128,128, TextureFormat.RGB24, false);
        byte[] bytes = File.ReadAllBytes(filename);

        materialTexture.filterMode = FilterMode.Trilinear;
        materialTexture.LoadImage(bytes);
        material.mainTexture = materialTexture;

        Texture2D newTexture = new Texture2D(materialTexture.width, materialTexture.height);// GraphicsFormat.R8G8B8A8_UNorm, true);

        Color[] data = new Color[materialTexture.width * materialTexture.height];

        int index = 0;
        //set the every pixel to be transparent
        for(int x = 0; x < materialTexture.width; x++) {
            for(int y = 0; y < materialTexture.height; y++) {                        
                data[index] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                index++;
            }
        }

        newTexture.SetPixels(0, 0, materialTexture.width, materialTexture.height, data);
        newTexture.Apply(true);

        material.SetTexture("_OverlayTexture", newTexture);

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
        SelectMaterialIcon(0);
        SelectTextureIcon(1);
    }
}

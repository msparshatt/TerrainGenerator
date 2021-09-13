using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.FB;
using UnityEngine.Events;

public struct SaveData
{
    public int version;
    public byte[] heightmap;
    public int baseTexture;

    public byte[] baseTexture_colors;
    public float tiling;
    public bool aoActive;
    public byte[] overlayTexture;
    public float paintTiling;
} 


public class ControlPanel : MonoBehaviour
{
    //object that handles exporting the terrain data
    [SerializeField] private Terrain currentTerrain;


    [Header("UI elements")]
    [SerializeField] private Text modeText;
    [SerializeField] private GameObject brushScrollView;
    [SerializeField] private GameObject brushPanel;
    [SerializeField] private Button brushDeleteButton;
    [SerializeField] private GameObject materialScrollView;
    [SerializeField] private GameObject materialPanel;
    [SerializeField] private Button materialDeleteButton;
    [SerializeField] private GameObject textureScrollView;
    [SerializeField] private GameObject texturePanel;
    [SerializeField] private Button textureDeleteButton;
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private RawImage materialImage;
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

    [Header("brush settings")]
    [SerializeField] private BrushDataScriptable brushData;

    [Header("Export settings")]
    [SerializeField] private Dropdown scaleDropdown;
    [SerializeField] private Texture2D busyCursor;

    //The currently used Material
    private Material currentMaterial;
    private int currentMaterialIndex;
    private List<string> customMaterials;

    //buttons created to select brushes,materials and textures
    private List<GameObject> brushIcons;
    private List<GameObject> materialIcons;
    private List<GameObject> textureIcons;
    private List<string> customBrushes;
    private int brushIndex;

    private List<string>customTextures;
    private int textureIndex;

    //UI colours
    private Color selectedColor;
    private Color deselectedColor;

    //assets from the resource folder used by the game
    private GameResources gameResources;

    //export objects
    private ExportHeightmap exportHeightmap;
    private ExportTerrain exportTerrain;

    public void Start() 
    {
        //cache the instance of the GameResources object
        gameResources = GameResources.instance;
        exportHeightmap = ExportHeightmap.instance;
        exportHeightmap.terrainObject = currentTerrain;
        exportTerrain = ExportTerrain.instance;
        exportTerrain.terrainObject = currentTerrain;
        exportTerrain.scaleDropDown = scaleDropdown;
        exportTerrain.busyCursor = busyCursor;

        CloseAllPanels();

        selectedColor = Color.green;
        deselectedColor = Color.white;

        Debug.Log("creating terrain " + Time.realtimeSinceStartup);
        TerrainManager.instance.currentMaterial = currentMaterial;
        TerrainManager.instance.currentTerrain = currentTerrain;

        TerrainManager.instance.setupTerrain();
        TerrainManager.instance.CreateFlatTerrain();

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

        //Debug.Log("loaded " + Time.realtimeSinceStartup);
        SelectMaterialIcon(0);
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

    //creates a list of buttons based off a list of images, parented to the passed in transform
    private List<GameObject> SetupIcons(Texture2D[] images, Transform parent, Action<int> onClickFunction)
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
        string filename = FileBrowser.OpenSingleFile("Open Heightmap file", "", new string[]{"raw", "png"});

        if(filename != "") {
            TerrainManager.instance.CreateTerrainFromHeightmap(filename);
        }
    }

    public void ProceduralButtonClick()
    {
        proceduralPanel.SetActive(!proceduralPanel.activeSelf);
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
    }

    public void MaterialButtonClick()
    {
        bool active = !materialPanel.activeSelf;
        CloseAllPanels();
        materialPanel.SetActive(active);
    }

    public void TextureButtonClick()
    {
        bool active = !texturePanel.activeSelf;
        CloseAllPanels();
        texturePanel.SetActive(active);
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

    public void SelectMaterialIcon(int buttonIndex)
    {
        currentMaterial = gameResources.materials[buttonIndex];

        if(currentMaterial.GetTexture("_AOTexture") == null) {
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

        currentMaterialIndex = buttonIndex;
        materialImage.texture = currentMaterial.mainTexture;
        TerrainManager.instance.SetTerrainMaterial(gameResources.materials[buttonIndex]);

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
        Vector2 scale = new Vector2(value, value);
        currentMaterial.mainTextureScale = scale;
        //brushData.textureScale = value;
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
        Texture2D overlayTexture = (Texture2D)currentMaterial.GetTexture("_OverlayTexture");
        int sizeX = overlayTexture.width;
        int sizeY = overlayTexture.height;

        Color[] data = new Color[sizeX * sizeY];

        int index = 0;
        //set the pixel data to transparent        
        for(int x = 0; x < sizeX; x++) {
            for(int y = 0; y < sizeY; y++) {                        
                data[index] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                index++;
            }
        }

        overlayTexture.SetPixels(0, 0, sizeX, sizeY, data);
        overlayTexture.Apply(true);
    }

    private void CloseAllPanels()
    {
        materialPanel.SetActive(false);
        brushPanel.SetActive(false);
        texturePanel.SetActive(false);
        helpPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void SaveButtonClick()
    {
        string filename = FileBrowser.SaveFile("save.json", "json");

        if(filename != "") {
            SaveData data = new SaveData();
            Texture2D texture;

            data.version = 1;

            data.heightmap = exportHeightmap.GetHeightmap();
            if(currentMaterialIndex >= (gameResources.materials.Count - customMaterials.Count)) {
                data.baseTexture = -1;
                texture = (Texture2D)currentMaterial.mainTexture;
                data.baseTexture_colors = texture.EncodeToPNG();
            } else {
                data.baseTexture = currentMaterialIndex;
                data.baseTexture_colors = null;
            }

            data.tiling = scaleSlider.value;
            data.aoActive = aoToggle.isOn;
            texture = (Texture2D)currentMaterial.GetTexture("_OverlayTexture");
            data.overlayTexture = texture.EncodeToPNG();
            data.paintTiling = paintScaleSlider.value;

            string json = JsonUtility.ToJson(data);

            var sr = File.CreateText(filename);
            sr.WriteLine (json);
            sr.Close();
        }
    }

    public void LoadButtonClick()
    {
        string filename = FileBrowser.OpenSingleFile("Open Heightmap file", "", "json");

        if(filename != "") {
            var sr = new StreamReader(filename);
            string fileContents = sr.ReadToEnd();
            sr.Close();        

            SaveData data = JsonUtility.FromJson<SaveData>(fileContents);

            TerrainManager.instance.CreateTerrainFromHeightmap(data.heightmap);

            if(data.baseTexture == -1) {
                Texture2D colorTexture = new Texture2D(10,10);
                ImageConversion.LoadImage(colorTexture, data.baseTexture_colors);

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

                    Color[] overlayPixels = new Color[colorTexture.width * colorTexture.height];

                    int pixelIndex = 0;
                    //set the every pixel to be transparent
                    for(int x = 0; x < colorTexture.width; x++) {
                        for(int y = 0; y < colorTexture.height; y++) {                        
                            overlayPixels[pixelIndex] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                            pixelIndex++;
                        }
                    }

                    newTexture.SetPixels(0, 0, colorTexture.width, colorTexture.height, overlayPixels);
                    newTexture.Apply(true);

                    material.SetTexture("_OverlayTexture", newTexture);

                    gameResources.materials.Add(material);

                    //Add the brush to the  brush selection panel          
                    GameObject newButton;
                    int ObjectIndex = materialIcons.Count;
                    Vector2 scale = new Vector2(1.0f, 1.0f);

                    newButton = MakeButton(colorTexture, delegate {SelectMaterialIcon(ObjectIndex); }, ObjectIndex);
                    newButton.transform.SetParent(materialScrollView.transform);
                    materialIcons.Add(newButton);

                    customMaterials.Add("");

                    SelectMaterialIcon(gameResources.materials.Count - 1);
                } else {
                    SelectMaterialIcon(index);
                }
            } else {
                SelectMaterialIcon(data.baseTexture);
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

            currentMaterial.SetTexture("_OverlayTexture", texture);
        }        
    }

    public void ExportButtonClick()
    {
        exportTerrain.Export(aoToggle.isOn, scaleSlider.value);
    }

    public void ExportHmButtonClick()
    {
        exportHeightmap.Export();
    }

    public void AOToggleChange(bool isOn)
    {
        if(isOn) {
            currentMaterial.SetInt("_ApplyAO", 1);
        } else {
            currentMaterial.SetInt("_ApplyAO", 0);
        }
    }

    public void BrushImportButtonclick()
    {
        string filename = FileBrowser.OpenSingleFile("Open brush file", "", "png");
        
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
        string filename = FileBrowser.OpenSingleFile("Open brush file", "", "png");
        
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
        string filename = FileBrowser.OpenSingleFile("Open brush file", "", "png");
        
        if(filename != "") {
            LoadCustomMaterial(filename);
            customMaterials.Add(filename);

            SelectMaterialIcon(gameResources.materials.Count - 1);
        }
    }

    public void MaterialDeleteButtonClick()
    {
        int customMaterialIndex = currentMaterialIndex + customMaterials.Count - gameResources.materials.Count;

        customMaterials.RemoveAt(customMaterialIndex);
        gameResources.materials.RemoveAt(currentMaterialIndex);
        Destroy(materialIcons[currentMaterialIndex]);
        materialIcons.RemoveAt(currentMaterialIndex);
        
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
}

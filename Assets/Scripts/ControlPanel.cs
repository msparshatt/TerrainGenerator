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
    [SerializeField] private InternalDataScriptable internalData;


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
        //LoadCustomBrushes();
        customTextures = new List<string>();
        //LoadCustomTextures();
        customMaterials = new List<string>();
        //LoadCustomMaterials();

        currentMaterialIndices = new int[] {0,0,0,0,0};

        Debug.Log("creating terrain " + Time.realtimeSinceStartup);

        manager.SetupTerrain(settingsData, internalData, busyCursor, textureShader, materialShader);
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
        //SelectTextureIcon(1);

        InitialiseFlags();
        //Debug.Log("end of start method " + Time.realtimeSinceStartup);
    }

    private void InitialiseFlags()
    {
        internalData.ProcGenOpen = false;
        internalData.sliderChanged = false;
    }

    private void SetupPanels()
    {
        //populate material selection panel          
        materialIcons = SetupIcons(gameResources.icons, materialScrollView.transform, SelectMaterialIcon);
        //textureIcons = SetupIcons(gameResources.icons, textureScrollView.transform, SelectTextureIcon);
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

    public void MaterialDropdownSelect()
    {
        bool oldDetect = internalData.detectMaximaAndMinima;
        internalData.detectMaximaAndMinima = false;
        for(int i = 1; i < 5; i++) {
            int mixType = mixtypeDropdowns[i].value + 1;

            if(mixType == (int)MixTypes.Peaks || mixType == (int)MixTypes.Valleys)
                internalData.detectMaximaAndMinima = true;

            offsetSliders[i].gameObject.SetActive((mixType == (int)MixTypes.Random));
            manager.SetMixType(i, mixType);
        }

        if(!oldDetect && internalData.detectMaximaAndMinima)
            manager.FindMaximaAndMinima();
            
        manager.ApplyTextures();
    }

    public void MixFactorSliderChange()
    {
        for(int i = 1; i < 5; i++) {
            manager.SetMixFactor(i, mixFactorSliders[i].value);
        }

        internalData.sliderChanged = true;
    }

    public void OffsetSliderChange()
    {
        for(int i = 1; i < 5; i++) {
            manager.SetOffset(i, offsetSliders[i].value);
        }

        internalData.sliderChanged = true;
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


    public void BrushImportButtonclick()
    {
		FileBrowser.SetFilters( true, new FileBrowser.Filter( "Image files", new string[] {".png", ".jpg", ".jpeg"}));
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


    public void MaterialImportButtonClick()
    {
		FileBrowser.SetFilters( true, new FileBrowser.Filter( "Image files", new string[] {".png", ".jpg", ".jpeg"}));
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
}

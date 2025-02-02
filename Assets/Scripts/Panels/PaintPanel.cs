using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using UnityEngine.InputSystem;
using TMPro;
using Newtonsoft.Json;


public class PaintPanel : MonoBehaviour, IPanel
{
    [Header("Brush Elements")]
    [SerializeField] private GameObject paintBrushScrollView;
    [SerializeField] private GameObject paintBrushPanel;
    [SerializeField] private Button paintBrushDeleteButton;
    [SerializeField] private RawImage paintBrushImage;
    [SerializeField] private Slider radiusSlider;
    [SerializeField] private Slider strengthSlider;
    [SerializeField] private Slider rotationSlider;

    [Header("Filter Elements")]
    [SerializeField] private Toggle filterToggle;
    [SerializeField] private TMP_Dropdown filterTypeDropdown;
    [SerializeField] private Slider filterFactorSlider;

    [Header("Texture Elements")]
    [SerializeField] private GameObject textureScrollView;
    [SerializeField] private GameObject texturePanel;
    [SerializeField] private Button textureDeleteButton;
    [SerializeField] private RawImage textureImage;
    [SerializeField] private Slider paintScaleSlider;
    [SerializeField] private Toggle textureToggle;

    [Header("Color Elements")]
    [SerializeField] private Toggle colorToggle;
    [SerializeField] private ColorPicker colorPicker;

    [Header("Elements")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject gameState;
    [SerializeField] private GameObject sidePanels;


    [Header("Data Objects")]
    [SerializeField] private PaintBrushDataScriptable paintBrushData;
    [SerializeField] private PaintBrushDataScriptable defaultBrushData;

    [SerializeField] private SettingsDataScriptable settingsData;
    [SerializeField] private InternalDataScriptable internalData;

    private GameResources gameResources;
    private List<GameObject> textureIcons;
    private List<GameObject> brushIcons;
    private int textureIndex;
    private Controller controller;
    private TerrainManager manager;
    private MaterialController materialController;
    private int brushIndex;

    private int selectedBrushIndex;
    private int selectedTextureIndex;


    // Start is called before the first frame update
    void Start()
    {
    }

    public void InitialisePanel()
    {
        manager = TerrainManager.Instance();
        materialController = manager.MaterialController;
        gameResources = GameResources.instance;
        textureIcons = UIHelper.SetupPanel(gameResources.icons, textureScrollView.transform, SelectTextureIcon);   
        SetupIconIndices(textureIcons, internalData.customTextureIndices);        
        brushIcons = UIHelper.SetupPanel(gameResources.paintBrushes, paintBrushScrollView.transform, SelectBrushIcon);
        SetupIconIndices(brushIcons, internalData.customPaintBrushIndices);        
        controller = gameState.GetComponent<Controller>();

        colorPicker.Awake();
        colorPicker.onColorChanged += delegate {ColorPickerChange(); };
        
        ResetPanel();
    }

    private void SetupIconIndices(List<GameObject> brushIcons, List<int> iconIndices)
    {
        for(int i = 0; i < brushIcons.Count; i++) {
            iconIndices.Add(i);
        }
    }

    public void ResetPanel()
    {
        SelectBrushIcon(0);
        SelectTextureIcon(1);

        radiusSlider.value = defaultBrushData.brushRadius;
        rotationSlider.value = defaultBrushData.brushRotation;
        strengthSlider.value = defaultBrushData.brushStrength;

        filterToggle.isOn = defaultBrushData.filter;
        filterTypeDropdown.value = (int)defaultBrushData.filterType - 1;
        filterFactorSlider.value = defaultBrushData.filterFactor;

        colorPicker.color = defaultBrushData.color;

        textureToggle.isOn = true;
        colorToggle.isOn = false;
    }

    //*************************************************************************************
    //code for loading legacy saves
    public void FromJson(string json)
    {
        if(json != null && json != "") {
            PaintSaveData_v1 data = JsonUtility.FromJson<PaintSaveData_v1>(json);
            radiusSlider.value = data.brushRadius;
            rotationSlider.value = data.brushRotation;
            strengthSlider.value = data.brushStrength;

            paintScaleSlider.value = data.paintScale;

            SelectBrushIcon(data.brushIndex);
        } else {
            ResetPanel();
        }
    }

    public void LoadPanel()
    {
    }
    //*************************************************************************************

    public string PanelName()
    {
        return "Paint";
    }

    public Dictionary<string, string> ToDictionary()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();

        data["paint_scale"] = paintBrushData.textureScale.ToString();
        data["brush_index"] = brushIndex.ToString();
        data["brush_radius"] = paintBrushData.brushRadius.ToString();
        data["brush_rotation"] = paintBrushData.brushRotation.ToString();
        data["brush_strength"] = paintBrushData.brushStrength.ToString();

        data["texture_index"] = textureIndex.ToString();
        data["use_texture"] = textureToggle.isOn.ToString();

        Color col = paintBrushData.color;
        float[] colorFloat = new float[4]{col.r, col.g, col.b, col.a};        
        data["paint_color"] = JsonConvert.SerializeObject(colorFloat);;

        data["filter_on"] = filterToggle.isOn.ToString();
        data["filter_type"] = filterTypeDropdown.value.ToString();
        data["filter_factor"] = filterFactorSlider.value.ToString();
        return data;
    }

    public void FromDictionary(Dictionary<string, string> data)
    {
        IPanel parent = (IPanel)this;

        radiusSlider.value = parent.TryReadValue(data, "brush_radius", defaultBrushData.brushRadius);
        rotationSlider.value = parent.TryReadValue(data, "brush_rotation", defaultBrushData.brushRotation);
        strengthSlider.value = parent.TryReadValue(data, "brush_strength", defaultBrushData.brushStrength);

        paintScaleSlider.value = parent.TryReadValue(data, "paint_scale", defaultBrushData.textureScale);

        SelectBrushIcon(parent.TryReadValue(data, "brush_index", 0));
        SelectTextureIcon(parent.TryReadValue(data, "texture_index", 0));

        textureToggle.isOn = parent.TryReadValue(data, "use_texture", defaultBrushData.useTexture);
        colorToggle.isOn = !textureToggle.isOn;

        float[] defaultColor = new float[4]{defaultBrushData.color.r, defaultBrushData.color.g, defaultBrushData.color.b, defaultBrushData.color.a};        
        float[] colorFloat = JsonConvert.DeserializeObject<float[]>(parent.TryReadValue(data, "paint_color", JsonConvert.SerializeObject(defaultColor)));
        paintBrushData.color = new Color(colorFloat[0], colorFloat[1], colorFloat[2], colorFloat[3]);
        colorPicker.color = paintBrushData.color;
       
        filterToggle.isOn = parent.TryReadValue(data, "filter_on", defaultBrushData.filter);
        filterTypeDropdown.value = parent.TryReadValue(data, "filter_type", (int)defaultBrushData.filterType - 1);
        filterFactorSlider.value = parent.TryReadValue(data, "filter_factor", defaultBrushData.filterFactor);
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

    public void TextureButtonClick()
    {
        bool active = !texturePanel.activeSelf;
        sidePanels.GetComponent<PanelController>().CloseAllPanels();

        if(active) {
            sidePanels.SetActive(true);
            texturePanel.SetActive(true);
            textureImage.color = settingsData.selectedColor;
            paintBrushImage.color = settingsData.deselectedColor;
        } else {
            textureImage.color = settingsData.deselectedColor;
        }
    }

    public void SelectTextureIcon(int buttonIndex)
    {
        selectedTextureIndex = buttonIndex;
        int index = internalData.customTextureIndices[buttonIndex];
        paintBrushData.paintTexture = (Texture2D)gameResources.textures[index];
        textureIndex = index;

        if(index >= (gameResources.icons.Count - internalData.customTextures.Count)) {
            textureDeleteButton.interactable = true;
            textureImage.texture =  gameResources.textures[index];
        } else {
            textureDeleteButton.interactable = false;
            textureImage.texture = gameResources.icons[index];
        }        

        for (int i = 0; i < textureIcons.Count; i++) {
            if(i == index) {
                textureIcons[i].GetComponent<Image>().color = settingsData.selectedColor;

            } else {
                textureIcons[i].GetComponent<Image>().color = settingsData.deselectedColor;
            }
        }
    }
    public void TextureDeleteButtonClick()
    {
        for(int i = selectedTextureIndex + 1; i < internalData.customTextureIndices.Count; i++) {
            internalData.customTextureIndices[i] -= 1;
        }

        int customTextureIndex = textureIndex + internalData.customTextures.Count - gameResources.textures.Count;

        internalData.customTextures.RemoveAt(customTextureIndex);
        gameResources.textures.RemoveAt(textureIndex);
        Destroy(textureIcons[textureIndex]);
        textureIcons.RemoveAt(textureIndex);
        
        SelectTextureIcon(0);
    }

    public void OnTextureImport(string filename)        
    {       
        if(filename != "") {
            controller.LoadCustomTexture(filename);
            internalData.customTextures.Add(filename);

            SelectTextureIcon(internalData.customTextureIndices.Count - 1);
        }
    }

    public void TextureImportButtonClick()
    {
		FileBrowser.SetFilters( true, new FileBrowser.Filter( "Image files", new string[] {".png", ".jpg", ".jpeg"}));
        FileBrowser.SetDefaultFilter( ".png" );

        playerInput.enabled = false;
        FileBrowser.ShowLoadDialog((filenames) => {playerInput.enabled = true;  OnTextureImport(filenames[0]);}, () => {playerInput.enabled = true; Debug.Log("Canceled Load");}, FileBrowser.PickMode.Files);
    }

    public void ClearButtonClick()
    {
        materialController.ClearOverlay();
    }

    public void PaintScaleSliderChange(float value)
    {
        paintBrushData.textureScale = value;
    }

    public void PaintResetTilingButtonClick()
    {
        paintScaleSlider.value = 1.0f;
    }

    public void AddButton(Texture2D texture, int index = 0)
    {
        if(index == 0)
            AddBrushButton(texture);
        else
            AddTextureButton(texture);
    }
    
    public void AddBrushButton(Texture2D texture)
    {
        GameObject newButton;
        internalData.customPaintBrushIndices.Add(brushIcons.Count);
        int ObjectIndex = internalData.customPaintBrushIndices.Count - 1;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = UIHelper.MakeButton(texture, delegate {SelectBrushIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(paintBrushScrollView.transform);
        brushIcons.Add(newButton);
    }

    public void AddTextureButton(Texture2D texture)
    {
        GameObject newButton;
        internalData.customTextureIndices.Add(textureIcons.Count);
        int ObjectIndex = internalData.customTextureIndices.Count - 1;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = UIHelper.MakeButton(texture, delegate {SelectTextureIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(textureScrollView.transform);
        textureIcons.Add(newButton);
    }

    //Brush Panel
    public void SelectBrushIcon(int buttonIndex)
    {
        selectedBrushIndex = buttonIndex;
        int index = internalData.customPaintBrushIndices[buttonIndex];

        paintBrushData.brush = gameResources.paintBrushes[index];
        paintBrushImage.texture = paintBrushData.brush;
        brushIndex = index;

        if(index >= (gameResources.paintBrushes.Count - internalData.customPaintBrushes.Count)) {
            paintBrushDeleteButton.interactable = true;
        } else {
            paintBrushDeleteButton.interactable = false;
        }        

        for (int i = 0; i < brushIcons.Count; i++) {
            if(i == index) {
                brushIcons[i].GetComponent<Image>().color = settingsData.selectedColor;

            } else {
                brushIcons[i].GetComponent<Image>().color = settingsData.deselectedColor;
            }
        }
    }

    public void RadiusSliderChange(float value)
    {
        paintBrushData.brushRadius = (int)value;
    }

    public void StrengthSliderChange(float value)
    {
        paintBrushData.brushStrength = value;
    }

    public void RotationSliderChange(float value)
    {
        paintBrushData.brushRotation = value;
    }

    public void BrushButtonClick()
    {
        bool active = !paintBrushPanel.activeSelf;

        sidePanels.GetComponent<PanelController>().CloseAllPanels();

        if(active) {
            sidePanels.SetActive(true);
            paintBrushPanel.SetActive(true);
            paintBrushImage.color = settingsData.selectedColor;
            textureImage.color = settingsData.deselectedColor;
        } else {
            paintBrushImage.color = settingsData.deselectedColor;
        }
    }

    public void BrushImportButtonclick()
    {
		FileBrowser.SetFilters( true, new FileBrowser.Filter( "Image files", new string[] {".png", ".jpg", ".jpeg"}));
        FileBrowser.SetDefaultFilter( ".png" );

        playerInput.enabled = false;
        FileBrowser.ShowLoadDialog((filenames) => {playerInput.enabled = true;  OnBrushImport(filenames[0]);}, () => {playerInput.enabled = true; Debug.Log("Canceled Load");}, FileBrowser.PickMode.Files);
    }

    public void OnBrushImport(string filename)
    {      
        if(filename != "") {
            controller.LoadCustomPaintBrush(filename);
            internalData.customPaintBrushes.Add(filename);
            SelectBrushIcon(internalData.customPaintBrushIndices.Count - 1);
        }
    }

    public void BrushDeleteButtonClick()
    {
        for(int i = selectedBrushIndex + 1; i < internalData.customPaintBrushIndices.Count; i++) {
            internalData.customPaintBrushIndices[i] -= 1;
        }

        int customBrushIndex = brushIndex + internalData.customPaintBrushes.Count - gameResources.paintBrushes.Count;

        internalData.customPaintBrushes.RemoveAt(customBrushIndex);
        gameResources.paintBrushes.RemoveAt(brushIndex);
        Destroy(brushIcons[brushIndex]);
        brushIcons.RemoveAt(brushIndex);
        
        SelectBrushIcon(0);
    }

    public void TextureToggleChange(bool isOn)
    {
        paintBrushData.useTexture = isOn;
    }

    public void ColorPickerChange()
    {
        paintBrushData.color = colorPicker.color;
    }

    public void FilterToggleChange(bool isOn)
    {
        paintBrushData.filter = isOn;
        materialController.TogglePaintMask(isOn);
    }

    public void FilterTypeChange(int value)
    {
        paintBrushData.filterType = (PaintBrushDataScriptable.MixTypes)(value + 1);
        materialController.ApplyMask();
    }

    public void FilterFactorSliderChange(float value)
    {
        paintBrushData.filterFactor = value;
        materialController.ApplyMask();
    }

    public void OnDisable()
    {
        if(materialController != null) {
            materialController.TogglePaintMask(false);
            textureImage.color = settingsData.deselectedColor;
            paintBrushImage.color = settingsData.deselectedColor;
        }
    }

    public void OnEnable()
    {
        if(materialController != null)
            materialController.TogglePaintMask(filterToggle.isOn);
    }
}

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using UnityEngine.InputSystem;
using TMPro;

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
        brushIcons = UIHelper.SetupPanel(gameResources.paintBrushes, paintBrushScrollView.transform, SelectBrushIcon);
        controller = gameState.GetComponent<Controller>();

        paintBrushData.filter = filterToggle.isOn;
        paintBrushData.filterType = (PaintBrushDataScriptable.MixTypes)(filterTypeDropdown.value + 1);
        paintBrushData.filterFactor = filterFactorSlider.value;

        colorPicker.Awake();
        colorPicker.onColorChanged += delegate {ColorPickerChange(); };
        colorPicker.color = Color.white;
        
        ResetPanel();
    }

    public void ResetPanel()
    {
        SelectBrushIcon(0);
        SelectTextureIcon(1);

        paintBrushData.brushRadius = defaultBrushData.brushRadius;
        paintBrushData.brushRotation = defaultBrushData.brushRotation;
        paintBrushData.brushStrength = defaultBrushData.brushStrength;

        radiusSlider.value = defaultBrushData.brushRadius;
        rotationSlider.value = defaultBrushData.brushRotation;
        strengthSlider.value = defaultBrushData.brushStrength;
    }

    public void FromJson(string json)
    {
        PaintSaveData_v1 data = JsonUtility.FromJson<PaintSaveData_v1>(json);
        radiusSlider.value = data.brushRadius;
        rotationSlider.value = data.brushRotation;
        strengthSlider.value = data.brushStrength;

        paintScaleSlider.value = data.paintScale;

        SelectBrushIcon(data.brushIndex);
    }

    public string ToJson()
    {
        PaintSaveData_v1 data = new PaintSaveData_v1();
        data.paintScale = internalData.paintScale;
        data.brushIndex = brushIndex;
        data.brushRadius = paintBrushData.brushRadius;
        data.brushRotation = paintBrushData.brushRotation;
        data.brushStrength = paintBrushData.brushStrength;
        
        return JsonUtility.ToJson(data);
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
        paintBrushData.paintTexture = (Texture2D)gameResources.textures[buttonIndex];
        textureIndex = buttonIndex;

        if(buttonIndex >= (gameResources.icons.Count - internalData.customTextures.Count)) {
            textureDeleteButton.interactable = true;
            textureImage.texture =  gameResources.textures[buttonIndex];
        } else {
            textureDeleteButton.interactable = false;
            textureImage.texture = gameResources.icons[buttonIndex];
        }        

        for (int i = 0; i < textureIcons.Count; i++) {
            if(i == buttonIndex) {
                textureIcons[i].GetComponent<Image>().color = settingsData.selectedColor;

            } else {
                textureIcons[i].GetComponent<Image>().color = settingsData.deselectedColor;
            }
        }
    }
    public void TextureDeleteButtonClick()
    {
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

            SelectTextureIcon(gameResources.textures.Count - 1);
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
        internalData.paintScale = value;
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
        int ObjectIndex = brushIcons.Count;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = UIHelper.MakeButton(texture, delegate {SelectBrushIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(paintBrushScrollView.transform);
        brushIcons.Add(newButton);
    }

    public void AddTextureButton(Texture2D texture)
    {
        GameObject newButton;
        int ObjectIndex = textureIcons.Count;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = UIHelper.MakeButton(texture, delegate {SelectTextureIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(textureScrollView.transform);
        textureIcons.Add(newButton);
    }

    //Brush Panel
    public void SelectBrushIcon(int buttonIndex)
    {
        paintBrushData.brush = gameResources.paintBrushes[buttonIndex];
        paintBrushImage.texture = paintBrushData.brush;
        brushIndex = buttonIndex;

        if(buttonIndex >= (gameResources.paintBrushes.Count - internalData.customPaintBrushes.Count)) {
            paintBrushDeleteButton.interactable = true;
        } else {
            paintBrushDeleteButton.interactable = false;
        }        

        for (int i = 0; i < brushIcons.Count; i++) {
            if(i == buttonIndex) {
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
            SelectBrushIcon(gameResources.paintBrushes.Count - 1);
        }
    }

    public void BrushDeleteButtonClick()
    {
        int customBrushIndex = brushIndex + internalData.customPaintBrushes.Count - gameResources.paintBrushes.Count;

        internalData.customPaintBrushes.RemoveAt(customBrushIndex);
        gameResources.paintBrushes.RemoveAt(brushIndex);
        Destroy(brushIcons[brushIndex]);
        brushIcons.RemoveAt(brushIndex);
        
        SelectBrushIcon(0);
    }

    public void LoadPanel()
    {
        paintScaleSlider.value = internalData.paintScale;
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

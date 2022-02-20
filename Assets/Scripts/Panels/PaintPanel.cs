using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using UnityEngine.InputSystem;

public class PaintPanel : MonoBehaviour
{
    [Header("Brush Elements")]
    [SerializeField] private GameObject paintBrushScrollView;
    [SerializeField] private GameObject paintBrushPanel;
    [SerializeField] private Button paintBrushDeleteButton;
    [SerializeField] private RawImage paintBrushImage;

    [Header("Texture Elements")]
    [SerializeField] private GameObject textureScrollView;
    [SerializeField] private GameObject texturePanel;
    [SerializeField] private Button textureDeleteButton;
    [SerializeField] private RawImage textureImage;
    [SerializeField] private Slider paintScaleSlider;

    [Header("Elements")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject gameState;
    [SerializeField] private GameObject sidePanels;


    [Header("Data Objects")]
    [SerializeField] private BrushDataScriptable paintBrushData;
    [SerializeField] private SettingsDataScriptable settingsData;

    private GameResources gameResources;
    private List<GameObject> textureIcons;
    private List<GameObject> brushIcons;
    private int textureIndex;
    private TerrainManager manager;
    private Controller controller;

    private int brushIndex;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void InitialisePaintPanel()
    {
        gameResources = GameResources.instance;
        textureIcons = UIHelper.SetupPanel(gameResources.icons, textureScrollView.transform, SelectTextureIcon);           
        brushIcons = UIHelper.SetupPanel(gameResources.paintBrushes, paintBrushScrollView.transform, SelectBrushIcon);
        manager = TerrainManager.instance;
        controller = gameState.GetComponent<Controller>();

        SelectTextureIcon(1);
        SelectBrushIcon(0);
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

        if(buttonIndex >= (gameResources.icons.Count - controller.customTextures.Count)) {
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
        int customTextureIndex = textureIndex + controller.customTextures.Count - gameResources.textures.Count;

        controller.customTextures.RemoveAt(customTextureIndex);
        gameResources.textures.RemoveAt(textureIndex);
        Destroy(textureIcons[textureIndex]);
        textureIcons.RemoveAt(textureIndex);
        
        SelectTextureIcon(0);
    }

    public void OnTextureImport(string filename)        
    {       
        if(filename != "") {
            controller.LoadCustomTexture(filename);
            controller.customTextures.Add(filename);

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
        manager.ClearOverlay();
    }

    public void PaintScaleSliderChange(float value)
    {
        paintBrushData.textureScale = value;
    }

    public void PaintResetTilingButtonClick()
    {
        paintScaleSlider.value = 1.0f;
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

        if(buttonIndex >= (gameResources.paintBrushes.Count - controller.customPaintBrushes.Count)) {
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
            SelectBrushIcon(gameResources.paintBrushes.Count - 1);
            controller.customPaintBrushes.Add(filename);
        }
    }

    public void BrushDeleteButtonClick()
    {
        int customBrushIndex = brushIndex + controller.customPaintBrushes.Count - gameResources.paintBrushes.Count;

        controller.customPaintBrushes.RemoveAt(customBrushIndex);
        gameResources.paintBrushes.RemoveAt(brushIndex);
        Destroy(brushIcons[brushIndex]);
        brushIcons.RemoveAt(brushIndex);
        
        SelectBrushIcon(0);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using SimpleFileBrowser;

public class MaterialsPanel : MonoBehaviour, IPanel
{
    public enum MixTypes  {Top = 1, Steep, Bottom, Shallow, Peaks, Valleys, Random};

    [Header("UI elements")]
    [SerializeField] private Slider scaleSlider;
    [SerializeField] private Toggle aoToggle;
    public TMP_Dropdown[] mixtypeDropdowns;
    public Slider[] mixFactorSliders;
    public Slider[] offsetSliders;
    [SerializeField] private GameObject gameState;
    [SerializeField] private Button materialDeleteButton;
    [SerializeField] private RawImage[] materialImages;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject materialPanel;
    [SerializeField] private GameObject materialScrollView;
    [SerializeField] private GameObject sidePanels;
    [SerializeField] private GameObject[] materialPanels;
    [SerializeField] private Toggle colorToggle;
    [SerializeField] private Toggle materialToggle;

    [Header("Solid Colors")]
    [SerializeField] private ColorPicker colorPicker;


    [Header("Data objects")]
    [SerializeField] private SettingsDataScriptable settingsData;
    [SerializeField] private InternalDataScriptable internalData;

    private List<GameObject> materialIcons;

    private TerrainManager manager;
    private GameResources gameResources;
    private Controller controller;

    private int materialPanelIndex;
    bool changeToggle = true;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void InitialisePanel()
    {
        changeToggle = false;
        manager = TerrainManager.instance;
        gameResources = GameResources.instance;
        controller = gameState.GetComponent<Controller>();

        internalData.currentMaterialIndices = new int[] {0,0,0,0,0};

        internalData.useTexture = new bool[InternalDataScriptable.NUMBER_MATERIALS];
        internalData.colors = new Color[InternalDataScriptable.NUMBER_MATERIALS];

        materialIcons = UIHelper.SetupPanel(gameResources.icons, materialScrollView.transform, SelectMaterialIcon);   
      
        colorPicker.Awake();
        colorPicker.onColorChanged += delegate {ColorPickerChange(); };

        changeToggle = true;

        ResetPanel();
    }

    public void ResetPanel()
    {
        Material[] materials = new Material[] {gameResources.materials[0], gameResources.materials[1], gameResources.materials[2], gameResources.materials[3], gameResources.materials[4]};

        for(int index = 0; index < InternalDataScriptable.NUMBER_MATERIALS; index++)
        {
            materialImages[index].texture = gameResources.icons[index];
            internalData.currentMaterialIndices[index] = index;
            internalData.useTexture[index] = true;
            internalData.colors[index] = Color.white;
        }

        scaleSlider.value = 1;
        internalData.materialScale = 1;

        for(int index = 1; index < InternalDataScriptable.NUMBER_MATERIALS; index++) {
            mixFactorSliders[index].value = 0;
            mixtypeDropdowns[index].value = 0;
        }

        MixFactorSliderChange();
        MaterialDropdownSelect();
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDisable()
    {
        for(int index = 0; index < InternalDataScriptable.NUMBER_MATERIALS; index++)
        {
            materialPanels[index].GetComponent<Image>().color = settingsData.deselectedColor2;
        }
    }

    //settings panel
    public void AOToggleChange(bool isOn)
    {
        internalData.ambientOcclusion = isOn;
        manager.SetAO(isOn);
    }

    public void ScaleSliderChange(float value)
    {
        internalData.sliderChanged = true;
        internalData.materialScale = value;
        manager.ApplyTextures();
    }

    public void ResetTilingButtonClick()
    {
        scaleSlider.value = 1.0f;
        //Camera.main.GetComponent<CameraController>().sliderChanged = true;

        manager.ApplyTextures();
    }

    public void MaterialDropdownSelect()
    {
        bool oldDetect = internalData.detectMaximaAndMinima;
        internalData.detectMaximaAndMinima = false;
        for(int i = 1; i < InternalDataScriptable.NUMBER_MATERIALS; i++) {
            int mixType = mixtypeDropdowns[i].value + 1;

            if(mixType == (int)MixTypes.Peaks || mixType == (int)MixTypes.Valleys)
                internalData.detectMaximaAndMinima = true;

            offsetSliders[i].gameObject.SetActive((mixType == (int)MixTypes.Random));

            internalData.mixTypes[i] = mixType;
        }

        if(!oldDetect && internalData.detectMaximaAndMinima)
            manager.FindMaximaAndMinima();
            
        manager.ApplyTextures();
    }

    public void MaterialDropdownSelect(int index)
    {
        bool oldDetect = internalData.detectMaximaAndMinima;
        internalData.detectMaximaAndMinima = false;

        int mixType = mixtypeDropdowns[index].value + 1;

        if(mixType == (int)MixTypes.Peaks || mixType == (int)MixTypes.Valleys)
            internalData.detectMaximaAndMinima = true;

        offsetSliders[index].gameObject.SetActive((mixType == (int)MixTypes.Random));

        internalData.mixTypes[index] = mixType;

        if(!oldDetect && internalData.detectMaximaAndMinima)
            manager.FindMaximaAndMinima();
            
        manager.ApplyTextures();
    }

    public void MixFactorSliderChange()
    {
        for(int i = 1; i < InternalDataScriptable.NUMBER_MATERIALS; i++) {
            internalData.mixFactors[i] = mixFactorSliders[i].value;
        }

        internalData.sliderChanged = true;
    }

    public void MixFactorSliderChange(int index)
    {
        float value = mixFactorSliders[index].value;

        internalData.mixFactors[index] = value;

        internalData.sliderChanged = true;
    }

    public void SelectMaterialIcon(int panel, int buttonIndex)
    {
        Material mat = gameResources.materials[buttonIndex];
        internalData.currentMaterialIndices[panel] = buttonIndex;

        if(changeToggle) {
            if(materialToggle.isOn) {
                ToggleChange(false);
            } else {
                materialToggle.isOn = true;
            }
        }

        for (int i = 0; i < materialIcons.Count; i++) {
            if(i == buttonIndex) {
                materialIcons[i].GetComponent<Image>().color = settingsData.selectedColor;
            } else {
                materialIcons[i].GetComponent<Image>().color = settingsData.deselectedColor;
            }
        }
    }
    public void SelectMaterialIcon(int buttonIndex)
    {        
        SelectMaterialIcon(materialPanelIndex, buttonIndex);
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
            controller.LoadCustomMaterial(filename);
            internalData.customMaterials.Add(filename);

            SelectMaterialIcon(gameResources.materials.Count - 1);
        }
    }

    public void MaterialDeleteButtonClick()
    {
        int customMaterialIndex = internalData.currentMaterialIndices[materialPanelIndex] + internalData.customMaterials.Count - gameResources.materials.Count;

        internalData.customMaterials.RemoveAt(customMaterialIndex);
        gameResources.materials.RemoveAt(internalData.currentMaterialIndices[materialPanelIndex]);
        Destroy(materialIcons[internalData.currentMaterialIndices[materialPanelIndex]]);
        materialIcons.RemoveAt(internalData.currentMaterialIndices[materialPanelIndex]);
        
        SelectMaterialIcon(0);
    }
    public void OffsetSliderChange()
    {
        for(int i = 1; i < InternalDataScriptable.NUMBER_MATERIALS; i++) {
            internalData.mixOffsets[i] = offsetSliders[i].value;
        }

        internalData.sliderChanged = true;
    }


    public void OffsetSliderChange(int index)
    {
        internalData.mixOffsets[index] = offsetSliders[index].value;
        internalData.sliderChanged = true;
    }

    public void MaterialButtonClick(int index)
    {
        bool active = !materialPanel.activeSelf;
        if(index != materialPanelIndex)
            active = true;

        sidePanels.GetComponent<PanelController>().CloseAllPanels();

        for(int i = 0; i < InternalDataScriptable.NUMBER_MATERIALS; i++) {
            materialPanels[i].GetComponent<Image>().color = settingsData.deselectedColor2;
        }

        if(active) {
            changeToggle = false;
            materialPanelIndex = index;
            colorPicker.color = internalData.colors[materialPanelIndex];
            sidePanels.SetActive(true);
            materialPanel.SetActive(true);

            if(internalData.useTexture[materialPanelIndex])
                materialToggle.isOn = true;
            else
                colorToggle.isOn = true;

            changeToggle = true;

            materialPanels[index].GetComponent<Image>().color = settingsData.selectedColor;


            for (int i = 0; i < materialIcons.Count; i++) {
                if(i == internalData.currentMaterialIndices[materialPanelIndex]) {
                    materialIcons[i].GetComponent<Image>().color = settingsData.selectedColor;

                } else {
                    materialIcons[i].GetComponent<Image>().color = settingsData.deselectedColor;
                }
            }
        }
    }

    public int AddBaseTexture(byte[] pixels)
    {
        Texture2D colorTexture = new Texture2D(10,10);
        ImageConversion.LoadImage(colorTexture, pixels);

        int index = -1;
        Hash128 textureHash = new Hash128();
        textureHash.Append(colorTexture.GetPixels());
        string hashString = textureHash.ToString();

        for(int i = (gameResources.materials.Count - internalData.customMaterials.Count); i < gameResources.materials.Count; i++) {
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

            newButton = UIHelper.MakeButton(colorTexture, delegate {SelectMaterialIcon(ObjectIndex); }, ObjectIndex);
            newButton.transform.SetParent(materialScrollView.transform);
            materialIcons.Add(newButton);

            internalData.customMaterials.Add("");

            index = gameResources.materials.Count - 1;
        }

        return index;
    }

    public void AddButton(Texture2D texture, int index = 0)
    {
        GameObject newButton;
        int ObjectIndex = materialIcons.Count;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = UIHelper.MakeButton(texture, delegate {SelectMaterialIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(materialScrollView.transform);
        materialIcons.Add(newButton);
    }

    public void LoadPanel()
    {
        for(int i = 1; i < InternalDataScriptable.NUMBER_MATERIALS; i++) {
            mixtypeDropdowns[i].value = internalData.mixTypes[i] - 1;
            mixFactorSliders[i].value = internalData.mixFactors[i];
        }

        aoToggle.isOn = internalData.ambientOcclusion;
        scaleSlider.value = internalData.materialScale;

        for(int index = 0; index < InternalDataScriptable.NUMBER_MATERIALS; index++) {
            if(internalData.useTexture[index]) {
                materialImages[index].color = Color.white;
                if(internalData.currentMaterialIndices[index] >= (gameResources.materials.Count - internalData.customMaterials.Count)) {
                    materialDeleteButton.interactable = true;
                    materialImages[index].texture = gameResources.materials[internalData.currentMaterialIndices[index]].mainTexture;
                } else {
                    materialDeleteButton.interactable = false;
                    materialImages[index].texture = gameResources.icons[internalData.currentMaterialIndices[index]];
                }        
            } else {
                materialImages[index].color = internalData.colors[index];
                materialImages[index].texture = null;
            }
        }
    }

    public void ToggleChange(bool isOn)
    {
        internalData.useTexture[materialPanelIndex] = !isOn;

        if(isOn) {
            materialImages[materialPanelIndex].color = internalData.colors[materialPanelIndex];
            materialImages[materialPanelIndex].texture = null;
        } else {
            materialImages[materialPanelIndex].color = Color.white;
            if(internalData.currentMaterialIndices[materialPanelIndex] >= (gameResources.materials.Count - internalData.customMaterials.Count)) {
                materialDeleteButton.interactable = true;
                materialImages[materialPanelIndex].texture = gameResources.materials[internalData.currentMaterialIndices[materialPanelIndex]].mainTexture;
            } else {
                materialDeleteButton.interactable = false;
                materialImages[materialPanelIndex].texture = gameResources.icons[internalData.currentMaterialIndices[materialPanelIndex]];
            }        
        }
        manager.ApplyTextures();
    }

    public void ColorPickerChange()
    {
        internalData.colors[materialPanelIndex] = colorPicker.color;

        if(changeToggle) {
            if(colorToggle.isOn){
                ToggleChange(true);
            } else {
                colorToggle.isOn = true;
            }
        }
    }
}
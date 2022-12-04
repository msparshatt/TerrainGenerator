using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using SimpleFileBrowser;
using Newtonsoft.Json;

public class MaterialsPanel : MonoBehaviour, IPanel
{
    public enum MixTypes  {Top = 1, Steep, Bottom, Shallow, Random};

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
    [SerializeField] private MaterialSettings materialSettings;

    private List<GameObject> materialIcons;

    private GameResources gameResources;
    private Controller controller;
    private TerrainManager manager;
    private MaterialController materialController;

    private int materialPanelIndex;
    private int[] selectedMaterialIndices = new int[] {0,0,0,0,0};
    bool changeToggle = true;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void InitialisePanel()
    {
        changeToggle = false;
        manager = TerrainManager.Instance();
        materialController = manager.MaterialController;
        gameResources = GameResources.instance;
        controller = gameState.GetComponent<Controller>();

        materialSettings.currentMaterialIndices = new int[MaterialSettings.NUMBER_MATERIALS];

        materialSettings.useTexture = new bool[MaterialSettings.NUMBER_MATERIALS];
        materialSettings.colors = new Color[MaterialSettings.NUMBER_MATERIALS];
        materialSettings.mixFactors = new float[MaterialSettings.NUMBER_MATERIALS];
        materialSettings.mixTypes = new int[MaterialSettings.NUMBER_MATERIALS];
        materialSettings.mixOffsets = new float[MaterialSettings.NUMBER_MATERIALS];

        materialIcons = UIHelper.SetupPanel(gameResources.icons, materialScrollView.transform, SelectMaterialIcon);   
        SetupIconIndices(materialIcons, internalData.customMaterialIndices);
      
        colorPicker.Awake();
        colorPicker.onColorChanged += delegate {ColorPickerChange(); };

        changeToggle = true;

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
        Material[] materials = new Material[] {gameResources.materials[0], gameResources.materials[1], gameResources.materials[2], gameResources.materials[3], gameResources.materials[4]};

        for(int index = 0; index < MaterialSettings.NUMBER_MATERIALS; index++)
        {
            materialImages[index].texture = gameResources.icons[index];
            materialSettings.currentMaterialIndices[index] = index;
            materialSettings.useTexture[index] = true;
            materialSettings.colors[index] = Color.white;
        }

        scaleSlider.value = 1;
        materialSettings.materialScale = 1;

        for(int index = 1; index < MaterialSettings.NUMBER_MATERIALS; index++) {
            mixFactorSliders[index].value = 0;
            mixtypeDropdowns[index].value = 0;
        }

        MixFactorSliderChange();
        MaterialDropdownSelect();
    }

    public void FromJson(string json)
    {
        
        MaterialSaveData_v1 data = JsonUtility.FromJson<MaterialSaveData_v1>(json);

        materialSettings.ambientOcclusion = data.ambientOcclusion;
        materialSettings.currentMaterialIndices = data.currentMaterialIndices;
        materialSettings.materialScale = data.materialScale;
        materialSettings.useTexture = data.useTexture;
        materialSettings.mixTypes = data.mixTypes;
        materialSettings.mixFactors = data.mixFactors;
        materialSettings.mixOffsets = data.mixOffsets;
        materialSettings.colors = data.colors;
    }

    public string ToJson()
    {
        MaterialSaveData_v1 data = new MaterialSaveData_v1();
        data.ambientOcclusion = materialSettings.ambientOcclusion;
        data.currentMaterialIndices = materialSettings.currentMaterialIndices;
        data.materialScale = materialSettings.materialScale;
        data.useTexture = materialSettings.useTexture;
        data.mixTypes = materialSettings.mixTypes;
        data.mixFactors = materialSettings.mixFactors;
        data.mixOffsets = materialSettings.mixOffsets;
        data.colors = materialSettings.colors;

        //fix to work with custom materials
        return  JsonUtility.ToJson(data);
    }

    public string PanelName()
    {
        return "Materials";
    }

    public Dictionary<string, string> ToDictionary()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["ambient_occlusion"] = JsonConvert.SerializeObject(materialSettings.ambientOcclusion);
        data["current_material_indices"] = JsonConvert.SerializeObject(materialSettings.currentMaterialIndices);
        data["material_scale"] = JsonConvert.SerializeObject(materialSettings.materialScale);
        data["use_texture"] = JsonConvert.SerializeObject(materialSettings.useTexture);
        data["mix_types"] = JsonConvert.SerializeObject(materialSettings.mixTypes);
        data["mix_factors"] = JsonConvert.SerializeObject(materialSettings.mixFactors);
        data["mix_offsets"] = JsonConvert.SerializeObject(materialSettings.mixOffsets);
        List<float[]> colorValues = new List<float[]>();
        for(int i = 0; i < MaterialSettings.NUMBER_MATERIALS; i++) {
            Color col = materialSettings.colors[i];
            float[] colorFloat = new float[4]{col.r, col.g, col.b, col.a};
            colorValues.Add(colorFloat);
        }
        data["colors"] = JsonConvert.SerializeObject(colorValues);
 

        return data;
    }

    public void FromDictionary(Dictionary<string, string> data)
    {
        materialSettings.ambientOcclusion = JsonConvert.DeserializeObject<bool>(data["ambient_occlusion"]);
        materialSettings.currentMaterialIndices = JsonConvert.DeserializeObject<int[]>(data["current_material_indices"]);
        materialSettings.materialScale = JsonConvert.DeserializeObject<float>(data["material_scale"]);
        materialSettings.useTexture = JsonConvert.DeserializeObject<bool[]>(data["use_texture"]);
        materialSettings.mixTypes = JsonConvert.DeserializeObject<int[]>(data["mix_types"]);
        materialSettings.mixFactors = JsonConvert.DeserializeObject<float[]>(data["mix_factors"]);
        materialSettings.mixOffsets = JsonConvert.DeserializeObject<float[]>(data["mix_offsets"]);
        List<float[]> colorValues = JsonConvert.DeserializeObject<List<float[]>>(data["colors"]);
        for(int i = 0; i < MaterialSettings.NUMBER_MATERIALS; i++) {
            Color col = new Color(colorValues[i][0], colorValues[i][1], colorValues[i][2], colorValues[i][3]);

            materialSettings.colors[i] = col;
        }

        LoadPanel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDisable()
    {
        for(int index = 0; index < MaterialSettings.NUMBER_MATERIALS; index++)
        {
            materialPanels[index].GetComponent<Image>().color = settingsData.deselectedColor2;
        }
    }

    //settings panel
    public void AOToggleChange(bool isOn)
    {
        materialSettings.ambientOcclusion = isOn;
        materialController.SetAO(isOn);
    }

    public void ScaleSliderChange(float value)
    {
        internalData.sliderChanged = true;
        materialSettings.materialScale = value;
        materialController.ApplyTextures();
    }

    public void ResetTilingButtonClick()
    {
        scaleSlider.value = 1.0f;
        //Camera.main.GetComponent<CameraController>().sliderChanged = true;

        materialController.ApplyTextures();
    }

    public void MaterialDropdownSelect()
    {
        for(int i = 1; i < MaterialSettings.NUMBER_MATERIALS; i++) {
            int mixType = mixtypeDropdowns[i].value + 1;

            offsetSliders[i].gameObject.SetActive((mixType == (int)MixTypes.Random));

            materialSettings.mixTypes[i] = mixType;
        }

           
        materialController.ApplyTextures();
    }

    public void MaterialDropdownSelect(int index)
    {
        int mixType = mixtypeDropdowns[index].value + 1;

        offsetSliders[index].gameObject.SetActive((mixType == (int)MixTypes.Random));

        materialSettings.mixTypes[index] = mixType;
           
        materialController.ApplyTextures();
    }

    public void MixFactorSliderChange()
    {
        for(int i = 1; i < MaterialSettings.NUMBER_MATERIALS; i++) {
            materialSettings.mixFactors[i] = mixFactorSliders[i].value;
        }

        internalData.sliderChanged = true;
    }

    public void MixFactorSliderChange(int index)
    {
        float value = mixFactorSliders[index].value;

        materialSettings.mixFactors[index] = value;

        internalData.sliderChanged = true;
    }

    public void SelectMaterialIcon(int panel, int buttonIndex)
    {
        selectedMaterialIndices[panel] = buttonIndex;        
        int index = internalData.customMaterialIndices[buttonIndex];
        Material mat = gameResources.materials[index];
        materialSettings.currentMaterialIndices[panel] = index;

        if(changeToggle) {
            if(materialToggle.isOn) {
                ToggleChange(false);
            } else {
                materialToggle.isOn = true;
            }
        }

        for (int i = 0; i < materialIcons.Count; i++) {
            if(i == index) {
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

            SelectMaterialIcon(internalData.customMaterialIndices.Count - 1);
        }
    }

    public void MaterialDeleteButtonClick()
    {
        for(int i = selectedMaterialIndices[materialPanelIndex] + 1; i < internalData.customMaterialIndices.Count; i++) {
            internalData.customMaterialIndices[i] -= 1;
        }

        int customMaterialIndex = materialSettings.currentMaterialIndices[materialPanelIndex] + internalData.customMaterials.Count - gameResources.materials.Count;

        internalData.customMaterials.RemoveAt(customMaterialIndex);
        gameResources.materials.RemoveAt(materialSettings.currentMaterialIndices[materialPanelIndex]);
        Destroy(materialIcons[materialSettings.currentMaterialIndices[materialPanelIndex]]);
        materialIcons.RemoveAt(materialSettings.currentMaterialIndices[materialPanelIndex]);
        
        SelectMaterialIcon(0);
    }
    public void OffsetSliderChange()
    {
        for(int i = 1; i < MaterialSettings.NUMBER_MATERIALS; i++) {
            materialSettings.mixOffsets[i] = offsetSliders[i].value;
        }

        internalData.sliderChanged = true;
    }


    public void OffsetSliderChange(int index)
    {
        materialSettings.mixOffsets[index] = offsetSliders[index].value;
        internalData.sliderChanged = true;
    }

    public void MaterialButtonClick(int index)
    {
        bool active = !materialPanel.activeSelf;
        if(index != materialPanelIndex)
            active = true;

        sidePanels.GetComponent<PanelController>().CloseAllPanels();

        for(int i = 0; i < MaterialSettings.NUMBER_MATERIALS; i++) {
            materialPanels[i].GetComponent<Image>().color = settingsData.deselectedColor2;
        }

        if(active) {
            changeToggle = false;
            materialPanelIndex = index;
            colorPicker.color = materialSettings.colors[materialPanelIndex];
            sidePanels.SetActive(true);
            materialPanel.SetActive(true);

            if(materialSettings.useTexture[materialPanelIndex])
                materialToggle.isOn = true;
            else
                colorToggle.isOn = true;

            changeToggle = true;

            materialPanels[index].GetComponent<Image>().color = settingsData.selectedColor;


            for (int i = 0; i < materialIcons.Count; i++) {
                if(i == materialSettings.currentMaterialIndices[materialPanelIndex]) {
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
        internalData.customMaterialIndices.Add(materialIcons.Count);
        int ObjectIndex = internalData.customMaterialIndices.Count - 1;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = UIHelper.MakeButton(texture, delegate {SelectMaterialIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(materialScrollView.transform);
        materialIcons.Add(newButton);
    }

    public void LoadPanel()
    {
        for(int i = 1; i < MaterialSettings.NUMBER_MATERIALS; i++) {
            mixtypeDropdowns[i].value = materialSettings.mixTypes[i] - 1;
            mixFactorSliders[i].value = materialSettings.mixFactors[i];
        }

        aoToggle.isOn = materialSettings.ambientOcclusion;
        scaleSlider.value = materialSettings.materialScale;

        for(int index = 0; index < MaterialSettings.NUMBER_MATERIALS; index++) {
            if(materialSettings.useTexture[index]) {
                materialImages[index].color = Color.white;
                if(materialSettings.currentMaterialIndices[index] >= (gameResources.materials.Count - internalData.customMaterials.Count)) {
                    materialDeleteButton.interactable = true;
                    materialImages[index].texture = gameResources.materials[materialSettings.currentMaterialIndices[index]].mainTexture;
                } else {
                    materialDeleteButton.interactable = false;
                    materialImages[index].texture = gameResources.icons[materialSettings.currentMaterialIndices[index]];
                }        
            } else {
                materialImages[index].color = materialSettings.colors[index];
                materialImages[index].texture = null;
            }
        }
    }

    public void ToggleChange(bool isOn)
    {
        materialSettings.useTexture[materialPanelIndex] = !isOn;

        if(isOn) {
            materialImages[materialPanelIndex].color = materialSettings.colors[materialPanelIndex];
            materialImages[materialPanelIndex].texture = null;
        } else {
            materialImages[materialPanelIndex].color = Color.white;
            if(materialSettings.currentMaterialIndices[materialPanelIndex] >= (gameResources.materials.Count - internalData.customMaterials.Count)) {
                materialDeleteButton.interactable = true;
                materialImages[materialPanelIndex].texture = gameResources.materials[materialSettings.currentMaterialIndices[materialPanelIndex]].mainTexture;
            } else {
                materialDeleteButton.interactable = false;
                materialImages[materialPanelIndex].texture = gameResources.icons[materialSettings.currentMaterialIndices[materialPanelIndex]];
            }        
        }
        materialController.ApplyTextures();
    }

    public void ColorPickerChange()
    {
        materialSettings.colors[materialPanelIndex] = colorPicker.color;

        if(changeToggle) {
            if(colorToggle.isOn){
                ToggleChange(true);
            } else {
                colorToggle.isOn = true;
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using SimpleFileBrowser;

public class MaterialsPanel : MonoBehaviour
{
    private enum MixTypes  {Top = 1, Bottom, Steep, Shallow, Peaks, Valleys, Random};

    [Header("UI elements")]
    [SerializeField] private Slider scaleSlider;
    [SerializeField] private TMP_Dropdown[] mixtypeDropdowns;
    [SerializeField] private Slider[] mixFactorSliders;
    [SerializeField] private Slider[] offsetSliders;
    [SerializeField] private GameObject gameState;
    [SerializeField] private Button materialDeleteButton;
    [SerializeField] private RawImage[] materialImages;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject materialPanel;
    [SerializeField] private GameObject materialScrollView;
    [SerializeField] private GameObject sidePanels;

    [Header("Data objects")]
    [SerializeField] private SettingsDataScriptable settingsData;
    [SerializeField] private InternalDataScriptable internalData;

    private List<GameObject> materialIcons;

    private TerrainManager manager;
    private GameResources gameResources;
    private Controller controller;

    private int materialPanelIndex;
    private int[] currentMaterialIndices;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void InitialiseMaterialPanel()
    {
        manager = TerrainManager.instance;
        gameResources = GameResources.instance;
        controller = gameState.GetComponent<Controller>();

        currentMaterialIndices = new int[] {0,0,0,0,0};

        materialIcons = UIHelper.SetupPanel(gameResources.icons, materialScrollView.transform, SelectMaterialIcon);   

        manager.doNotApply = true;

        MixFactorSliderChange();
        MaterialDropdownSelect();
        for(int index = 0; index < 5; index++)
        {
            SelectMaterialIcon(index, index);
        }
        manager.doNotApply = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //settings panel
    public void AOToggleChange(bool isOn)
    {
        manager.SetAO(isOn);
    }

    public void ScaleSliderChange(float value)
    {
        internalData.sliderChanged = true;
        manager.ScaleMaterial(value);
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

    public void SelectMaterialIcon(int panel, int buttonIndex)
    {
        Material mat = gameResources.materials[buttonIndex];
        currentMaterialIndices[panel] = buttonIndex;

        Vector2 scale = new Vector2(scaleSlider.value, scaleSlider.value);
        mat.mainTextureScale = scale;

        if(buttonIndex >= (gameResources.materials.Count - controller.customMaterials.Count)) {
            materialDeleteButton.interactable = true;
        } else {
            materialDeleteButton.interactable = false;
        }        

        materialImages[panel].texture = gameResources.icons[buttonIndex];

        manager.SetBaseMaterials(panel, mat);

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
            controller.customMaterials.Add(filename);

            SelectMaterialIcon(gameResources.materials.Count - 1);
        }
    }

    public void MaterialDeleteButtonClick()
    {
        int customMaterialIndex = currentMaterialIndices[materialPanelIndex] + controller.customMaterials.Count - gameResources.materials.Count;

        controller.customMaterials.RemoveAt(customMaterialIndex);
        gameResources.materials.RemoveAt(currentMaterialIndices[0]);
        Destroy(materialIcons[currentMaterialIndices[0]]);
        materialIcons.RemoveAt(currentMaterialIndices[0]);
        
        SelectMaterialIcon(0);
    }
    public void OffsetSliderChange()
    {
        for(int i = 1; i < 5; i++) {
            manager.SetOffset(i, offsetSliders[i].value);
        }

        internalData.sliderChanged = true;
    }

    public void MaterialButtonClick(int index)
    {
        bool active = !materialPanel.activeSelf;
        if(index != materialPanelIndex)
            active = true;

        sidePanels.GetComponent<PanelController>().CloseAllPanels();

        for(int i = 0; i < 5; i++) {
            materialImages[i].color = settingsData.deselectedColor;
        }

        if(active) {
            sidePanels.SetActive(true);
            materialPanel.SetActive(true);
            materialPanelIndex = index;

            materialImages[index].color = settingsData.selectedColor;

            for (int i = 0; i < materialIcons.Count; i++) {
                if(i == currentMaterialIndices[materialPanelIndex]) {
                    materialIcons[i].GetComponent<Image>().color = settingsData.selectedColor;

                } else {
                    materialIcons[i].GetComponent<Image>().color = settingsData.deselectedColor;
                }
            }
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

        for(int i = (gameResources.materials.Count - controller.customMaterials.Count); i < gameResources.materials.Count; i++) {
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

            controller.customMaterials.Add("");

            index = gameResources.materials.Count - 1;
        }

        return index;
    }

    public void AddButton(Texture2D texture)
    {
        GameObject newButton;
        int ObjectIndex = materialIcons.Count;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = UIHelper.MakeButton(texture, delegate {SelectMaterialIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(materialScrollView.transform);
        materialIcons.Add(newButton);
    }
}
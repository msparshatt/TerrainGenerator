using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SimpleFileBrowser;
using TMPro;


public class SculptPanel : MonoBehaviour, IPanel
{
    [Header("UI elements")]
    [SerializeField] private GameObject[] brushPanels;
    [SerializeField] private GameObject erosionPanel;
    [SerializeField] private GameObject sculptBrushScrollView;
    [SerializeField] private GameObject sculptBrushPanel;
    [SerializeField] private GameObject sidePanels;
    [SerializeField] private Button brushDeleteButton;
    [SerializeField] private RawImage brushImage;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject gameState;

    [SerializeField] private TMP_Dropdown modeDropdown;
    [SerializeField] private Slider radiusSlider;
    [SerializeField] private Slider strengthSlider;
    [SerializeField] private Slider rotationSlider;
    [SerializeField] private Slider heightSlider;
    [SerializeField] private Toggle contourToggle;

    [Header("Data Objects")]
    [SerializeField] private BrushDataScriptable brushData;
    [SerializeField] private BrushDataScriptable setHeightBrushData;
    [SerializeField] private BrushDataScriptable defaultBrushData;
    [SerializeField] private SettingsDataScriptable settingsData;
    [SerializeField] private InternalDataScriptable internalData;

    private List<GameObject> brushIcons;
    private int brushIndex;

    private GameResources gameResources;
    private Controller controller;

    private TerrainManager manager;
    private MaterialController materialController;

    public void InitialisePanel()
    {
        gameResources = GameResources.instance;
        controller = gameState.GetComponent<Controller>();
        manager = TerrainManager.Instance();
        materialController = manager.MaterialController;
        
        brushIcons = UIHelper.SetupPanel(gameResources.brushes, sculptBrushScrollView.transform, SelectBrushIcon);   

        ResetPanel();
    }

    public void ResetPanel()
    {
        SelectBrushIcon(0);

        radiusSlider.value = defaultBrushData.brushRadius;
        rotationSlider.value = defaultBrushData.brushRotation;
        strengthSlider.value = defaultBrushData.brushStrength;

        modeDropdown.value = 0;
        ModeChange(0);

        internalData.terrainMode = InternalDataScriptable.TerrainModes.Sculpt;

        heightSlider.value = 0.5f;
        brushData.brushHeight = 0.5f;
    }

    public string ToJson()
    {
        BrushSaveData_v1 data = new BrushSaveData_v1();
        data.brushRadius = brushData.brushRadius;
        data.brushRotation = brushData.brushRotation;
        data.brushStrength = brushData.brushStrength;
        data.brushIndex = brushIndex;

        return JsonUtility.ToJson(data);
    }

    public void FromJson(string json)
    {
        BrushSaveData_v1 data = JsonUtility.FromJson<BrushSaveData_v1>(json);

        radiusSlider.value = data.brushRadius;
        rotationSlider.value = data.brushRotation;
        strengthSlider.value = data.brushStrength;
        SelectBrushIcon(data.brushIndex);
    }

    public void OnDisable()
    {
        brushImage.color = settingsData.deselectedColor;

        if(materialController != null) {
            materialController.ToggleContourMask(false);
        }
    }
    
    public void OnEnable()
    {
        if(materialController != null)
            materialController.ToggleContourMask(contourToggle.isOn);
    }

    public void BrushButtonClick()
    {
        bool active = !sculptBrushPanel.activeSelf;

        sidePanels.GetComponent<PanelController>().CloseAllPanels();

        if(active) {
            sidePanels.SetActive(true);
            sculptBrushPanel.SetActive(true);
            brushImage.color = settingsData.selectedColor;
        } else {
            brushImage.color = settingsData.deselectedColor;
        }
    }

    public void SelectBrushIcon(int buttonIndex)
    {
        brushData.brush = gameResources.brushes[buttonIndex];
        brushImage.texture = brushData.brush;
        brushIndex = buttonIndex;

        if(buttonIndex >= (gameResources.brushes.Count - internalData.customSculptBrushes.Count)) {
            brushDeleteButton.interactable = true;
        } else {
            brushDeleteButton.interactable = false;
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
        brushData.brushRadius = (int)value;
    }

    public void StrengthSliderChange(float value)
    {
        brushData.brushStrength = value;
    }

    public void RotationSliderChange(float value)
    {
        setHeightBrushData.brushRotation = value;
    }

    public void SetHeightRadiusSliderChange(float value)
    {
        setHeightBrushData.brushRadius = (int)value;
    }

    public void SetHeightStrengthSliderChange(float value)
    {
        setHeightBrushData.brushStrength = value;
    }

    public void SetHeightRotationSliderChange(float value)
    {
        brushData.brushRotation = value;
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
            controller.LoadCustomBrush(filename);
            internalData.customSculptBrushes.Add(filename);
            SelectBrushIcon(gameResources.brushes.Count - 1);
        }
    }

    public void BrushDeleteButtonClick()
    {
        int customBrushIndex = brushIndex + internalData.customSculptBrushes.Count - gameResources.brushes.Count;

        internalData.customSculptBrushes.RemoveAt(customBrushIndex);
        gameResources.brushes.RemoveAt(brushIndex);
        Destroy(brushIcons[brushIndex]);
        brushIcons.RemoveAt(brushIndex);
        
        SelectBrushIcon(0);
    }

    public void AddButton(Texture2D texture, int index = 0)
    {
        GameObject newButton;
        int ObjectIndex = brushIcons.Count;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = UIHelper.MakeButton(texture, delegate {SelectBrushIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(sculptBrushScrollView.transform);
        brushIcons.Add(newButton);
    }

    public void ModeChange(int mode)
    {
        for(int i = 0; i < brushPanels.Length; i++) {
            brushPanels[i].SetActive(false);
        }
        brushPanels[mode].SetActive(true);

        if(mode == 3)
            erosionPanel.SetActive(true);
        else
            erosionPanel.SetActive(false);


        internalData.terrainMode = (InternalDataScriptable.TerrainModes)mode;
    }

    public void HeightSliderChange(float height)
    {
        brushData.brushHeight = height;
    }

    public void SetHeightButtonClick()
    {
        TerrainManager.Instance().TerrainSculpter.SetTerrainHeight(brushData.brushHeight);
    }

    public void ContourToggleChange(bool isOn)
    {
        if(materialController != null)
            materialController.ToggleContourMask(contourToggle.isOn);
    }
}

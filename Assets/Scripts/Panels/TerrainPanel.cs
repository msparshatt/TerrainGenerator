using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SimpleFileBrowser;
using TMPro;


public class TerrainPanel : MonoBehaviour, IPanel
{
    [Header("UI elements")]
    [SerializeField] private GameObject[] brushPanels;
    [SerializeField] private GameObject erosionPanel;
    [SerializeField] private GameObject sculptBrushScrollView;
    [SerializeField] private GameObject stampBrushScrollView;
    [SerializeField] private GameObject erosionBrushScrollView;
    [SerializeField] private GameObject sculptBrushScrollViewContents;
    [SerializeField] private GameObject stampBrushScrollViewContents;
    [SerializeField] private GameObject erosionBrushScrollViewContents;
    [SerializeField] private GameObject setHeightBrushScrollView;
    [SerializeField] private GameObject setHeightBrushScrollViewContents;

    [SerializeField] private GameObject sculptBrushPanel;
    [SerializeField] private GameObject sidePanels;
    [SerializeField] private Button brushDeleteButton;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject gameState;

    [SerializeField] private TMP_Dropdown modeDropdown;
    [SerializeField] private Toggle contourToggle;

    [Header("Sculpt UI")]
    [SerializeField] private Slider radiusSlider;
    [SerializeField] private Slider strengthSlider;
    [SerializeField] private Slider rotationSlider;
    [SerializeField] private RawImage brushImage;

    [Header("Set Height UI")]
    [SerializeField] private Slider setHeightRadiusSlider;
    [SerializeField] private Slider setHeightStrengthSlider;
    [SerializeField] private Slider setHeightRotationSlider;
    [SerializeField] private Slider setHeightHeightSlider;
    [SerializeField] private RawImage setHeightBrushImage;

    [Header("Stamp UI")]
    [SerializeField] private Slider stampRadiusSlider;
    [SerializeField] private Slider stampStrengthSlider;
    [SerializeField] private Slider stampRotationSlider;
    [SerializeField] private RawImage stampBrushImage;

    [Header("Erode UI")]
    [SerializeField] private Slider erodeRadiusSlider;
    [SerializeField] private Slider erodeStrengthSlider;
    [SerializeField] private Slider erodeRotationSlider;
    [SerializeField] private RawImage erodeBrushImage;

    [Header("Data Objects")]
    [SerializeField] private BrushDataScriptable brushData;
    [SerializeField] private BrushDataScriptable setHeightBrushData;
    [SerializeField] private BrushDataScriptable stampBrushData;
    [SerializeField] private BrushDataScriptable erodeBrushData;

    [SerializeField] private BrushDataScriptable defaultBrushData;
    [SerializeField] private BrushDataScriptable defaultSetHeightBrushData;
    [SerializeField] private BrushDataScriptable defaultStampBrushData;
    [SerializeField] private BrushDataScriptable defaultErodeBrushData;
    [SerializeField] private SettingsDataScriptable settingsData;
    [SerializeField] private InternalDataScriptable internalData;
    [SerializeField] private ErosionDataScriptable erosionData;
    [SerializeField] private ErosionDataScriptable defaultErosionData;

    [Header("Erosion Settings")]
    [SerializeField] private Slider lifetimeSlider;
    [SerializeField] private Slider sedimentCapacityFactorSlider;
    [SerializeField] private Slider inertiaSlider;
    [SerializeField] private Slider depositSpeedSlider;
    [SerializeField] private Slider erodeSpeedSlider;
    [SerializeField] private Slider startSpeedSlider;
    [SerializeField] private Slider evaporateSpeedSlider;
    [SerializeField] private Slider startWaterSlider;



    private List<GameObject> sculptBrushIcons;
    private List<GameObject> stampBrushIcons;
    private List<GameObject> erodeBrushIcons;
    private List<GameObject> setHeightBrushIcons;

    private int brushIndex;
    private int stampBrushIndex;
    private int erodeBrushIndex;
    private int setHeightBrushIndex;


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
        
        sculptBrushIcons = UIHelper.SetupPanel(gameResources.brushes, sculptBrushScrollViewContents.transform, SelectBrushIcon);   
        stampBrushIcons = UIHelper.SetupPanel(gameResources.stampBrushes, stampBrushScrollViewContents.transform, SelectBrushIcon);   
        erodeBrushIcons = UIHelper.SetupPanel(gameResources.erosionBrushes, erosionBrushScrollViewContents.transform, SelectBrushIcon);   
        setHeightBrushIcons = UIHelper.SetupPanel(gameResources.setHeightBrushes, setHeightBrushScrollViewContents.transform, SelectBrushIcon);   

        ResetPanel();
    }

    public void ResetPanel()
    {
        SelectBrushIcon(0, InternalDataScriptable.TerrainModes.Sculpt);
        SelectBrushIcon(0, InternalDataScriptable.TerrainModes.Stamp);
        SelectBrushIcon(0, InternalDataScriptable.TerrainModes.Erode);
        SelectBrushIcon(0, InternalDataScriptable.TerrainModes.SetHeight);

        sculptBrushScrollView.SetActive(false);
        stampBrushScrollView.SetActive(false);
        erosionBrushScrollView.SetActive(false);

        radiusSlider.value = defaultBrushData.brushRadius;
        rotationSlider.value = defaultBrushData.brushRotation;
        strengthSlider.value = defaultBrushData.brushStrength;
        setHeightRadiusSlider.value = defaultBrushData.brushRadius;
        setHeightRotationSlider.value = defaultBrushData.brushRotation;
        setHeightStrengthSlider.value = defaultBrushData.brushStrength;
        setHeightRadiusSlider.value = defaultSetHeightBrushData.brushRadius;
        setHeightRotationSlider.value = defaultSetHeightBrushData.brushRotation;
        setHeightStrengthSlider.value = defaultSetHeightBrushData.brushStrength;
        setHeightHeightSlider.value = defaultSetHeightBrushData.brushHeight;
        stampRadiusSlider.value = defaultStampBrushData.brushRadius;
        stampRotationSlider.value = defaultStampBrushData.brushRotation;
        stampStrengthSlider.value = defaultStampBrushData.brushStrength;
        erodeRadiusSlider.value = defaultErodeBrushData.brushRadius;
        erodeRotationSlider.value = defaultErodeBrushData.brushRotation;
        erodeStrengthSlider.value = defaultErodeBrushData.brushStrength;

        modeDropdown.value = 0;
        ModeChange(0);

        internalData.terrainMode = InternalDataScriptable.TerrainModes.Sculpt;
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
        SelectBrushIcon(data.brushIndex, InternalDataScriptable.TerrainModes.Sculpt);
    }

    public void LoadStampSettings(string json)
    {
        BrushSaveData_v1 data = JsonUtility.FromJson<BrushSaveData_v1>(json);

        stampRadiusSlider.value = data.brushRadius;
        stampRotationSlider.value = data.brushRotation;
        stampStrengthSlider.value = data.brushStrength;

        SelectBrushIcon(data.brushIndex, InternalDataScriptable.TerrainModes.Stamp);        
    }

    public void LoadErodeSettings(string json)
    {
        if(json != null && json != "") {
            ErosionSaveData_v1 data = JsonUtility.FromJson<ErosionSaveData_v1>(json);
            erodeRadiusSlider.value = data.brushRadius;
            erodeRotationSlider.value = data.brushRotation;
            erodeStrengthSlider.value = data.brushStrength;

            lifetimeSlider.value = data.lifetime;
            sedimentCapacityFactorSlider.value = data.sedimentCapacityFactor;
            inertiaSlider.value = data.inertia;
            depositSpeedSlider.value = data.depositSpeed;
            erodeSpeedSlider.value = data.erodeSpeed;
            startSpeedSlider.value = data.startSpeed;
            evaporateSpeedSlider.value = data.evaporateSpeed;
            startWaterSlider.value = data.startWater;

            SelectBrushIcon(data.brushIndex, InternalDataScriptable.TerrainModes.Erode);
        } else {
            ResetPanel();
        }
    }

    public void OnDisable()
    {
        brushImage.color = settingsData.deselectedColor;
        stampBrushImage.color = settingsData.deselectedColor;
        erodeBrushImage.color = settingsData.deselectedColor;

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

            if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Sculpt) {
                sculptBrushScrollView.SetActive(true);
            } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Stamp) {
                stampBrushScrollView.SetActive(true);
            } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Erode) {
                erosionBrushScrollView.SetActive(true);
            } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.SetHeight) {
                setHeightBrushScrollView.SetActive(true);
            }

            brushImage.color = settingsData.selectedColor;
            stampBrushImage.color = settingsData.selectedColor;
            erodeBrushImage.color = settingsData.selectedColor;
            setHeightBrushImage.color = settingsData.selectedColor;
        } else {
            brushImage.color = settingsData.deselectedColor;
            stampBrushImage.color = settingsData.deselectedColor;
            erodeBrushImage.color = settingsData.deselectedColor;
            setHeightBrushImage.color = settingsData.deselectedColor;

            sculptBrushScrollView.SetActive(false);
            stampBrushScrollView.SetActive(false);
            erosionBrushScrollView.SetActive(false);
            setHeightBrushScrollView.SetActive(false);
        }
    }

    public void SelectBrushIcon(int buttonIndex)
    {
        SelectBrushIcon(buttonIndex, internalData.terrainMode);
    }


    public void SelectBrushIcon(int buttonIndex, InternalDataScriptable.TerrainModes terrainMode)
    {
        List<GameObject> brushIcons = null;

        if(terrainMode == InternalDataScriptable.TerrainModes.Sculpt) {
            brushData.brush = gameResources.brushes[buttonIndex];
            brushImage.texture = brushData.brush;
            brushIndex = buttonIndex;

            brushIcons = sculptBrushIcons;

            brushImage.texture = brushData.brush;
        } else if(terrainMode == InternalDataScriptable.TerrainModes.Stamp) {
            stampBrushData.brush = gameResources.stampBrushes[buttonIndex];
            stampBrushImage.texture = stampBrushData.brush;
            stampBrushIndex = buttonIndex;

            brushIcons = stampBrushIcons;

            stampBrushImage.texture = stampBrushData.brush;
        } else if(terrainMode == InternalDataScriptable.TerrainModes.Erode) {
            erodeBrushData.brush = gameResources.erosionBrushes[buttonIndex];
            erodeBrushImage.texture = erodeBrushData.brush;
            erodeBrushIndex = buttonIndex;

            brushIcons = erodeBrushIcons;

            erodeBrushImage.texture = erodeBrushData.brush;
        } else if(terrainMode == InternalDataScriptable.TerrainModes.SetHeight) {
            setHeightBrushData.brush = gameResources.setHeightBrushes[buttonIndex];
            setHeightBrushImage.texture = setHeightBrushData.brush;
            setHeightBrushIndex = buttonIndex;

            brushIcons = erodeBrushIcons;

            setHeightBrushImage.texture = setHeightBrushData.brush;
        }


        if(buttonIndex >= (gameResources.brushes.Count - internalData.customSculptBrushes.Count)) {
            brushDeleteButton.interactable = true;
        } else {
            brushDeleteButton.interactable = false;
        }        

        if(brushIcons != null) {
            for (int i = 0; i < brushIcons.Count; i++) {
                if(i == buttonIndex) {
                    brushIcons[i].GetComponent<Image>().color = settingsData.selectedColor;

                } else {
                    brushIcons[i].GetComponent<Image>().color = settingsData.deselectedColor;
                }
            }
        }
    }

    //Slider controls
    public void RadiusSliderChange(float value)
    {
        if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Sculpt) {
            brushData.brushRadius = (int)value;
        } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.SetHeight) {
            setHeightBrushData.brushRadius = (int)value;
        } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Stamp) {
            stampBrushData.brushRadius = (int)value;
        } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Erode) {
            erodeBrushData.brushRadius = (int)value;
        }
    }

    public void StrengthSliderChange(float value)
    {
        if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Sculpt) {
            brushData.brushStrength = value;
        } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.SetHeight) {
            setHeightBrushData.brushStrength = value;
        } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Stamp) {
            stampBrushData.brushStrength = value;
        } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Erode) {
            erodeBrushData.brushStrength = value;
        }
    }

    public void RotationSliderChange(float value)
    {
        if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Sculpt) {
            brushData.brushRotation = (int)value;
        } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.SetHeight) {
            setHeightBrushData.brushRotation = (int)value;
        } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Stamp) {
            stampBrushData.brushRotation = (int)value;
        } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Erode) {
            erodeBrushData.brushRotation = (int)value;
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
        Destroy(sculptBrushIcons[brushIndex]);

        sculptBrushIcons.RemoveAt(brushIndex);
        
        SelectBrushIcon(0);
    }

    public void AddButton(Texture2D texture, int index = 0)
    {
        GameObject newButton;
        int ObjectIndex = sculptBrushIcons.Count;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = UIHelper.MakeButton(texture, delegate {SelectBrushIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(sculptBrushScrollView.transform);
        sculptBrushIcons.Add(newButton);
    }

    public void AddStampButton(Texture2D texture, int index = 0)
    {
        GameObject newButton;
        int ObjectIndex = stampBrushIcons.Count;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = UIHelper.MakeButton(texture, delegate {SelectBrushIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(sculptBrushScrollView.transform);
        stampBrushIcons.Add(newButton);
    }

    public void AddErodeButton(Texture2D texture, int index = 0)
    {
        GameObject newButton;
        int ObjectIndex = erodeBrushIcons.Count;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = UIHelper.MakeButton(texture, delegate {SelectBrushIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(sculptBrushScrollView.transform);
        erodeBrushIcons.Add(newButton);
    }

    public void ModeChange(int mode)
    {
        for(int i = 0; i < brushPanels.Length; i++) {
            brushPanels[i].SetActive(false);
        }
        brushPanels[mode].SetActive(true);

        sculptBrushPanel.SetActive(false);
        if(mode == 3)
            erosionPanel.SetActive(true);
        else
            erosionPanel.SetActive(false);


        internalData.terrainMode = (InternalDataScriptable.TerrainModes)mode;
    }

    public void HeightSliderChange(float height)
    {
        setHeightBrushData.brushHeight = height;
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

    public void ErosionSliderChange()
    {
        erosionData.lifetime = (int)lifetimeSlider.value;
        erosionData.sedimentCapacityFactor = sedimentCapacityFactorSlider.value;
        erosionData.inertia = inertiaSlider.value;
        erosionData.depositSpeed = depositSpeedSlider.value;
        erosionData.erodeSpeed = erodeSpeedSlider.value;
        erosionData.startSpeed = startSpeedSlider.value;
        erosionData.evaporateSpeed = evaporateSpeedSlider.value;
        erosionData.startWater = startWaterSlider.value;
    }
}

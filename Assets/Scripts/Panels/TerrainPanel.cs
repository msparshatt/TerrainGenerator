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
    [SerializeField] private GameObject slopeBrushScrollView;
    [SerializeField] private GameObject sculptBrushScrollViewContents;
    [SerializeField] private GameObject stampBrushScrollViewContents;
    [SerializeField] private GameObject erosionBrushScrollViewContents;
    [SerializeField] private GameObject slopeBrushScrollViewContents;
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

    [Header("Slope UI")]
    [SerializeField] private Slider slopeRadiusSlider;
    [SerializeField] private Slider slopeStrengthSlider;
    [SerializeField] private Slider slopeRotationSlider;
    [SerializeField] private RawImage slopeBrushImage;

    [Header("Data Objects")]
    [SerializeField] private BrushDataScriptable brushData;
    [SerializeField] private BrushDataScriptable setHeightBrushData;
    [SerializeField] private BrushDataScriptable stampBrushData;
    [SerializeField] private BrushDataScriptable erodeBrushData;
    [SerializeField] private BrushDataScriptable slopeBrushData;

    [SerializeField] private BrushDataScriptable defaultBrushData;
    [SerializeField] private BrushDataScriptable defaultSetHeightBrushData;
    [SerializeField] private BrushDataScriptable defaultStampBrushData;
    [SerializeField] private BrushDataScriptable defaultErodeBrushData;
    [SerializeField] private BrushDataScriptable defaultSlopeBrushData;

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
    private List<GameObject> slopeBrushIcons;

    private int brushIndex;
    private int stampBrushIndex;
    private int erodeBrushIndex;
    private int setHeightBrushIndex;
    private int slopeBrushIndex;
    private int selectedSculptBrushIndex;
    private int selectedSetHeightBrushIndex;
    private int selectedStampBrushIndex;
    private int selectedErodeBrushIndex;
    private int selectedSlopeBrushIndex;


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
        SetupIconIndices(sculptBrushIcons, internalData.customSculptBrushIndices);
        stampBrushIcons = UIHelper.SetupPanel(gameResources.stampBrushes, stampBrushScrollViewContents.transform, SelectBrushIcon);   
        SetupIconIndices(stampBrushIcons, internalData.customStampBrushIndices);
        erodeBrushIcons = UIHelper.SetupPanel(gameResources.erosionBrushes, erosionBrushScrollViewContents.transform, SelectBrushIcon);   
        SetupIconIndices(erodeBrushIcons, internalData.customErosionBrushIndices);
        setHeightBrushIcons = UIHelper.SetupPanel(gameResources.setHeightBrushes, setHeightBrushScrollViewContents.transform, SelectBrushIcon);   
        SetupIconIndices(setHeightBrushIcons, internalData.customSetHeightBrushIndices);
        slopeBrushIcons = UIHelper.SetupPanel(gameResources.slopeBrushes, slopeBrushScrollViewContents.transform, SelectBrushIcon);   
        SetupIconIndices(slopeBrushIcons, internalData.customSlopeBrushIndices);

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
        SelectBrushIcon(0, InternalDataScriptable.TerrainModes.Sculpt);
        SelectBrushIcon(0, InternalDataScriptable.TerrainModes.Stamp);
        SelectBrushIcon(0, InternalDataScriptable.TerrainModes.Erode);
        SelectBrushIcon(0, InternalDataScriptable.TerrainModes.SetHeight);
        SelectBrushIcon(0, InternalDataScriptable.TerrainModes.Slope);

        sculptBrushScrollView.SetActive(false);
        stampBrushScrollView.SetActive(false);
        erosionBrushScrollView.SetActive(false);
        setHeightBrushScrollView.SetActive(false);

        radiusSlider.value = defaultBrushData.brushRadius;
        rotationSlider.value = defaultBrushData.brushRotation;
        strengthSlider.value = defaultBrushData.brushStrength;
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
        slopeRadiusSlider.value = defaultSlopeBrushData.brushRadius;
        slopeRotationSlider.value = defaultSlopeBrushData.brushRotation;
        slopeStrengthSlider.value = defaultSlopeBrushData.brushStrength;

        lifetimeSlider.value = defaultErosionData.lifetime;
        sedimentCapacityFactorSlider.value = defaultErosionData.sedimentCapacityFactor;
        inertiaSlider.value = defaultErosionData.inertia;
        depositSpeedSlider.value = defaultErosionData.depositSpeed;
        erodeSpeedSlider.value = defaultErosionData.erodeSpeed;
        startSpeedSlider.value = defaultErosionData.startSpeed;
        evaporateSpeedSlider.value = defaultErosionData.evaporateSpeed;
        startWaterSlider.value = defaultErosionData.startWater;

        modeDropdown.value = 0;
        ModeChange(0);
        contourToggle.isOn = false;
        materialController.ToggleContourMask(false);

        internalData.terrainMode = InternalDataScriptable.TerrainModes.Sculpt;
    }

    public void FromJson(string json)
    {
        if(json != null && json != "") {
            BrushSaveData_v1 data = JsonUtility.FromJson<BrushSaveData_v1>(json);

            radiusSlider.value = data.brushRadius;
            rotationSlider.value = data.brushRotation;
            strengthSlider.value = data.brushStrength;
            SelectBrushIcon(data.brushIndex, InternalDataScriptable.TerrainModes.Sculpt);
        } else {
            ResetPanel();
        }
    }

    public void LoadStampSettings(string json)
    {
        if(json != null && json != "") {
            BrushSaveData_v1 data = JsonUtility.FromJson<BrushSaveData_v1>(json);

            stampRadiusSlider.value = data.brushRadius;
            stampRotationSlider.value = data.brushRotation;
            stampStrengthSlider.value = data.brushStrength;

            SelectBrushIcon(data.brushIndex, InternalDataScriptable.TerrainModes.Stamp);        
        } else {
            ResetPanel();
        }
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

    public string PanelName()
    {
        return "Terrain";
    }

    public Dictionary<string, string> ToDictionary()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();

        data["contours_on"] = contourToggle.isOn.ToString();
        data["terrain_mode"] = modeDropdown.value.ToString();

        data["sculpt_brush_radius"] = brushData.brushRadius.ToString();
        data["sculpt_brush_rotation"] = brushData.brushRotation.ToString();
        data["sculpt_brush_strength"] = brushData.brushStrength.ToString();
        data["sculpt_brush_index"] = brushIndex.ToString();

        data["stamp_brush_radius"] = stampBrushData.brushRadius.ToString();
        data["stamp_brush_rotation"] = stampBrushData.brushRotation.ToString();
        data["stamp_brush_strength"] = stampBrushData.brushStrength.ToString();
        data["stamp_brush_index"] = stampBrushIndex.ToString();

        data["erode_brush_radius"] = erodeBrushData.brushRadius.ToString();
        data["erode_brush_rotation"] = erodeBrushData.brushRotation.ToString();
        data["erode_brush_strength"] = erodeBrushData.brushStrength.ToString();
        data["erode_brush_index"] = erodeBrushIndex.ToString();

        data["erode_lifetime"] = lifetimeSlider.value.ToString();
        data["erode_sediment_capacity_factor"] = sedimentCapacityFactorSlider.value.ToString();
        data["erode_inertia"] = inertiaSlider.value.ToString();
        data["erode_deposit_speed"] = depositSpeedSlider.value.ToString();
        data["erode_speed"] = erodeSpeedSlider.value.ToString();
        data["erode_start_speed"] = startSpeedSlider.value.ToString();
        data["erode_evaporate_speed"] = evaporateSpeedSlider.value.ToString();
        data["erode_start_water"] = startWaterSlider.value.ToString();

        data["set_height_brush_radius"] = setHeightBrushData.brushRadius.ToString();
        data["set_height_brush_rotation"] = setHeightBrushData.brushRotation.ToString();
        data["set_height_brush_strength"] = setHeightBrushData.brushStrength.ToString();
        data["set_height_brush_index"] = setHeightBrushIndex.ToString();
        data["set_height_brush_height"] = setHeightBrushData.brushHeight.ToString();

        data["slope_brush_radius"] = slopeBrushData.brushRadius.ToString();
        data["slope_brush_rotation"] = slopeBrushData.brushRotation.ToString();
        data["slope_brush_strength"] = slopeBrushData.brushStrength.ToString();
        data["slope_brush_index"] = slopeBrushIndex.ToString();

        return data;
    }

    public void FromDictionary(Dictionary<string, string> data)
    {
        IPanel parent = (IPanel)this;
        contourToggle.isOn = parent.TryReadValue(data, "contours_on", false);
        modeDropdown.value = parent.TryReadValue(data, "terrain_mode", 0);

        radiusSlider.value = parent.TryReadValue(data, "sculpt_brush_radius", defaultBrushData.brushRadius);
        rotationSlider.value = parent.TryReadValue(data, "sculpt_brush_rotation", defaultBrushData.brushRotation);
        strengthSlider.value = parent.TryReadValue(data, "sculpt_brush_strength", defaultBrushData.brushStrength);
        SelectBrushIcon(parent.TryReadValue(data, "sculpt_brush_index", 0), InternalDataScriptable.TerrainModes.Sculpt);

        stampRadiusSlider.value = parent.TryReadValue(data, "stamp_brush_radius", defaultStampBrushData.brushRadius);
        stampRotationSlider.value = parent.TryReadValue(data, "stamp_brush_rotation", defaultStampBrushData.brushRotation);
        stampStrengthSlider.value = parent.TryReadValue(data, "stamp_brush_strength", defaultStampBrushData.brushStrength);
        SelectBrushIcon(parent.TryReadValue(data, "stamp_brush_index", 0), InternalDataScriptable.TerrainModes.Stamp);

        erodeRadiusSlider.value = parent.TryReadValue(data, "erode_brush_radius", defaultBrushData.brushRadius);
        erodeRotationSlider.value = parent.TryReadValue(data, "erode_brush_rotation", defaultBrushData.brushRotation);
        erodeStrengthSlider.value = parent.TryReadValue(data, "erode_brush_strength", defaultBrushData.brushStrength);
        SelectBrushIcon(parent.TryReadValue(data, "erode_brush_index", 0), InternalDataScriptable.TerrainModes.Erode);

        lifetimeSlider.value = parent.TryReadValue(data, "erode_lifetime", defaultErosionData.lifetime);
        sedimentCapacityFactorSlider.value = parent.TryReadValue(data, "erode_sediment_capacity_factor", defaultErosionData.sedimentCapacityFactor);
        inertiaSlider.value = parent.TryReadValue(data, "erode_inertia", defaultErosionData.inertia);
        depositSpeedSlider.value = parent.TryReadValue(data, "erode_deposit_speed", defaultErosionData.depositSpeed);
        erodeSpeedSlider.value = parent.TryReadValue(data, "erode_speed", defaultErosionData.erodeSpeed);
        startSpeedSlider.value = parent.TryReadValue(data, "erode_start_speed", defaultErosionData.startSpeed);
        evaporateSpeedSlider.value = parent.TryReadValue(data, "erode_evaporate_speed", defaultErosionData.evaporateSpeed);
        startWaterSlider.value = parent.TryReadValue(data, "erode_start_water", defaultErosionData.startWater);

        setHeightRadiusSlider.value = parent.TryReadValue(data, "set_height_brush_radius", defaultSetHeightBrushData.brushRadius);
        setHeightRotationSlider.value = parent.TryReadValue(data, "set_height_brush_rotation", defaultSetHeightBrushData.brushRotation);
        setHeightStrengthSlider.value = parent.TryReadValue(data, "set_height_brush_strength", defaultSetHeightBrushData.brushStrength);
        setHeightHeightSlider.value = parent.TryReadValue(data, "set_height_brush_height", defaultSetHeightBrushData.brushHeight);

        SelectBrushIcon(parent.TryReadValue(data, "set_height_brush_index", 0), InternalDataScriptable.TerrainModes.SetHeight);        

        slopeRadiusSlider.value = parent.TryReadValue(data, "slope_brush_radius", defaultSlopeBrushData.brushRadius);
        slopeRotationSlider.value = parent.TryReadValue(data, "slope_brush_rotation", defaultSlopeBrushData.brushRotation);
        slopeStrengthSlider.value = parent.TryReadValue(data, "slope_brush_strength", defaultSlopeBrushData.brushStrength);

        SelectBrushIcon(parent.TryReadValue(data, "slope_brush_index", 0), InternalDataScriptable.TerrainModes.Slope);        
    }

    public void OnDisable()
    {
        DeactivateSidePanels();

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

        DeactivateSidePanels();

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
            } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Slope) {
                slopeBrushScrollView.SetActive(true);
            }

            brushImage.color = settingsData.selectedColor;
            stampBrushImage.color = settingsData.selectedColor;
            erodeBrushImage.color = settingsData.selectedColor;
            setHeightBrushImage.color = settingsData.selectedColor;
        } else {
        }
    }

    private void DeactivateSidePanels()
    {
        sidePanels.GetComponent<PanelController>().CloseAllPanels();

        brushImage.color = settingsData.deselectedColor;
        stampBrushImage.color = settingsData.deselectedColor;
        erodeBrushImage.color = settingsData.deselectedColor;
        setHeightBrushImage.color = settingsData.deselectedColor;
        slopeBrushImage.color = settingsData.deselectedColor;

        sculptBrushScrollView.SetActive(false);
        stampBrushScrollView.SetActive(false);
        erosionBrushScrollView.SetActive(false);
        setHeightBrushScrollView.SetActive(false);
        slopeBrushScrollView.SetActive(false);
    }

    public void SelectBrushIcon(int buttonIndex)
    {
        SelectBrushIcon(buttonIndex, internalData.terrainMode);
    }


    public void SelectBrushIcon(int buttonIndex, InternalDataScriptable.TerrainModes terrainMode)
    {
        List<GameObject> brushIcons = null;
        List<Texture2D> brushes = null;
        List<string> customBrushes = null;

        if(terrainMode == InternalDataScriptable.TerrainModes.Sculpt) {
            brushes = gameResources.brushes;
            customBrushes = internalData.customSculptBrushes;
            selectedSculptBrushIndex = buttonIndex;
            buttonIndex = internalData.customSculptBrushIndices[buttonIndex];

            brushData.brush = gameResources.brushes[buttonIndex];
            brushImage.texture = brushData.brush;
            brushIndex = buttonIndex;

            brushIcons = sculptBrushIcons;

            brushImage.texture = brushData.brush;
        } else if(terrainMode == InternalDataScriptable.TerrainModes.Stamp) {
            brushes = gameResources.stampBrushes;
            customBrushes = internalData.customStampBrushes;
            selectedStampBrushIndex = buttonIndex;
            buttonIndex = internalData.customStampBrushIndices[buttonIndex];

            stampBrushData.brush = gameResources.stampBrushes[buttonIndex];
            stampBrushImage.texture = stampBrushData.brush;
            stampBrushIndex = buttonIndex;

            brushIcons = stampBrushIcons;

            stampBrushImage.texture = stampBrushData.brush;
        } else if(terrainMode == InternalDataScriptable.TerrainModes.Erode) {
            brushes = gameResources.erosionBrushes;
            customBrushes = internalData.customErosionBrushes;
            selectedErodeBrushIndex = buttonIndex;
            buttonIndex = internalData.customErosionBrushIndices[buttonIndex];

            erodeBrushData.brush = gameResources.erosionBrushes[buttonIndex];
            erodeBrushImage.texture = erodeBrushData.brush;
            erodeBrushIndex = buttonIndex;

            brushIcons = erodeBrushIcons;
            erodeBrushImage.texture = erodeBrushData.brush;
        } else if(terrainMode == InternalDataScriptable.TerrainModes.SetHeight) {
            brushes = gameResources.setHeightBrushes;
            customBrushes = internalData.customSetHeightBrushes;
            selectedSetHeightBrushIndex = buttonIndex;
            buttonIndex = internalData.customSetHeightBrushIndices[buttonIndex];

            setHeightBrushData.brush = gameResources.setHeightBrushes[buttonIndex];
            setHeightBrushImage.texture = setHeightBrushData.brush;
            setHeightBrushIndex = buttonIndex;

            brushIcons = setHeightBrushIcons;

            setHeightBrushImage.texture = setHeightBrushData.brush;
        } else if(terrainMode == InternalDataScriptable.TerrainModes.Slope) {
            brushes = gameResources.slopeBrushes;
            customBrushes = internalData.customSlopeBrushes;
            selectedSlopeBrushIndex = buttonIndex;
            buttonIndex = internalData.customSlopeBrushIndices[buttonIndex];

            slopeBrushData.brush = gameResources.setHeightBrushes[buttonIndex];
            slopeBrushImage.texture = setHeightBrushData.brush;
            slopeBrushIndex = buttonIndex;

            brushIcons = slopeBrushIcons;

            slopeBrushImage.texture = slopeBrushData.brush;
        }

        if(buttonIndex >= (brushes.Count - customBrushes.Count)) {
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
        } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Slope) {
            slopeBrushData.brushRadius = (int)value;
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
        } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Slope) {
            slopeBrushData.brushStrength = value;
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
        } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Slope) {
            slopeBrushData.brushRotation = (int)value;
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
            int index = 0;
            controller.LoadCustomTerrainBrush(filename, internalData.terrainMode);

            if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Sculpt) {
                internalData.customSculptBrushes.Add(filename);
                index = internalData.customSculptBrushIndices.Count - 1;
            } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.SetHeight) {
                internalData.customSetHeightBrushes.Add(filename);
                index = internalData.customSetHeightBrushIndices.Count - 1;
            } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Stamp) {
                internalData.customStampBrushes.Add(filename);
                index = internalData.customStampBrushIndices.Count - 1;
            } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Erode) {
                internalData.customErosionBrushes.Add(filename);
                index = internalData.customErosionBrushIndices.Count - 1;
            } 

            SelectBrushIcon(index);
        }
    }

    public void BrushDeleteButtonClick()
    {
        int customBrushIndex = 0;
        if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Sculpt) {
            for(int i = selectedSculptBrushIndex + 1; i < internalData.customSculptBrushIndices.Count; i++) {
                internalData.customSculptBrushIndices[i] -= 1;
            }

            customBrushIndex = brushIndex + internalData.customSculptBrushes.Count - gameResources.brushes.Count;
            internalData.customSculptBrushes.RemoveAt(customBrushIndex);
            gameResources.brushes.RemoveAt(brushIndex);
            Destroy(sculptBrushIcons[brushIndex]);

            sculptBrushIcons.RemoveAt(brushIndex);
            
        } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.SetHeight) {
            for(int i = selectedSetHeightBrushIndex + 1; i < internalData.customSetHeightBrushIndices.Count; i++) {
                internalData.customSetHeightBrushIndices[i] -= 1;
            }

            customBrushIndex = setHeightBrushIndex + internalData.customSetHeightBrushes.Count - gameResources.setHeightBrushes.Count;
            internalData.customSetHeightBrushes.RemoveAt(customBrushIndex);
            gameResources.setHeightBrushes.RemoveAt(setHeightBrushIndex);
            Destroy(setHeightBrushIcons[setHeightBrushIndex]);

            setHeightBrushIcons.RemoveAt(setHeightBrushIndex);            
        } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Stamp) {
            customBrushIndex = stampBrushIndex + internalData.customStampBrushes.Count - gameResources.stampBrushes.Count;

            for(int i = selectedStampBrushIndex; i < internalData.customStampBrushIndices.Count; i++) {
                internalData.customStampBrushIndices[i] -= 1;
            }

            internalData.customStampBrushes.RemoveAt(customBrushIndex);
            gameResources.stampBrushes.RemoveAt(stampBrushIndex);
            Destroy(stampBrushIcons[stampBrushIndex]);

            stampBrushIcons.RemoveAt(stampBrushIndex);            
        } else if(internalData.terrainMode == InternalDataScriptable.TerrainModes.Erode) {
            customBrushIndex = erodeBrushIndex + internalData.customErosionBrushes.Count - gameResources.erosionBrushes.Count;

            for(int i = selectedErodeBrushIndex; i < internalData.customErosionBrushIndices.Count; i++) {
                internalData.customErosionBrushIndices[i] -= 1;
            }
            internalData.customErosionBrushes.RemoveAt(customBrushIndex);
            gameResources.erosionBrushes.RemoveAt(erodeBrushIndex);
            Destroy(erodeBrushIcons[erodeBrushIndex]);

            erodeBrushIcons.RemoveAt(erodeBrushIndex);            
        }

        SelectBrushIcon(0);
    }

    public void AddTerrainButton(Texture2D texture, InternalDataScriptable.TerrainModes mode = InternalDataScriptable.TerrainModes.Sculpt)
    {
        Transform transform = sculptBrushScrollViewContents.transform;
        int ObjectIndex = internalData.customSculptBrushIndices.Count;
        List<GameObject> brushIcons = sculptBrushIcons;

        if(mode == InternalDataScriptable.TerrainModes.Sculpt) {
            transform = sculptBrushScrollViewContents.transform;
            ObjectIndex = internalData.customSculptBrushIndices.Count;
            brushIcons = sculptBrushIcons;

            internalData.customSculptBrushIndices.Add(sculptBrushIcons.Count);

        } else if(mode == InternalDataScriptable.TerrainModes.SetHeight) {
            transform = setHeightBrushScrollViewContents.transform;
            ObjectIndex = internalData.customSetHeightBrushIndices.Count;
            brushIcons = setHeightBrushIcons;
            internalData.customSetHeightBrushIndices.Add(setHeightBrushIcons.Count);
        } else if(mode == InternalDataScriptable.TerrainModes.Stamp) {
            transform = stampBrushScrollViewContents.transform;
            ObjectIndex = internalData.customStampBrushIndices.Count;
            brushIcons = stampBrushIcons;
            internalData.customStampBrushIndices.Add(stampBrushIcons.Count);
        } else if(mode == InternalDataScriptable.TerrainModes.Erode) {
            transform = erosionBrushScrollViewContents.transform;
            ObjectIndex = internalData.customErosionBrushIndices.Count;
            brushIcons = erodeBrushIcons;

            internalData.customErosionBrushIndices.Add(erodeBrushIcons.Count);
        }

        GameObject newButton;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = UIHelper.MakeButton(texture, delegate {SelectBrushIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(transform);
        brushIcons.Add(newButton);
    }

    public void ModeChange(int mode)
    {
        for(int i = 0; i < brushPanels.Length; i++) {
            brushPanels[i].SetActive(false);
        }
        brushPanels[mode].SetActive(true);

        DeactivateSidePanels();

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
        
        TerrainManager.Instance().TerrainModifier.SetTerrainHeight(setHeightBrushData.brushHeight);
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

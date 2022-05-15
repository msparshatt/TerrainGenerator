using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SimpleFileBrowser;



public class ErosionPanel : MonoBehaviour, IPanel
{
    [Header("UI elements")]
    [SerializeField] private GameObject erosionBrushScrollView;
    [SerializeField] private GameObject erosionBrushPanel;
    [SerializeField] private GameObject sidePanels;
    [SerializeField] private Button brushDeleteButton;
    [SerializeField] private RawImage brushImage;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject gameState;
    [SerializeField] private Slider radiusSlider;
    [SerializeField] private Slider strengthSlider;
    [SerializeField] private Slider rotationSlider;


    [Header("Erosion Settings")]
    [SerializeField] private Slider lifetimeSlider;
    [SerializeField] private Slider sedimentCapacityFactorSlider;
    [SerializeField] private Slider inertiaSlider;
    [SerializeField] private Slider depositSpeedSlider;
    [SerializeField] private Slider erodeSpeedSlider;
    [SerializeField] private Slider startSpeedSlider;
    [SerializeField] private Slider evaporateSpeedSlider;
    [SerializeField] private Slider startWaterSlider;


    [Header("Data Objects")]
    [SerializeField] private BrushDataScriptable brushData;
    [SerializeField] private BrushDataScriptable defaultBrushData;
    [SerializeField] private SettingsDataScriptable settingsData;
    [SerializeField] private InternalDataScriptable internalData;
    [SerializeField] private ErosionDataScriptable erosionData;

    private List<GameObject> brushIcons;
    private int brushIndex;

    private GameResources gameResources;
    private Controller controller;

    public void InitialisePanel()
    {
        gameResources = GameResources.instance;
        controller = gameState.GetComponent<Controller>();
        
        brushIcons = UIHelper.SetupPanel(gameResources.erosionBrushes, erosionBrushScrollView.transform, SelectBrushIcon);   

        ResetPanel();
    }

    public void ResetPanel()
    {
        SelectBrushIcon(0);

        radiusSlider.value = defaultBrushData.brushRadius;
        rotationSlider.value = defaultBrushData.brushRotation;
        strengthSlider.value = defaultBrushData.brushStrength;
    }

    public string ToJson()
    {
        Debug.Log("SAVE: Sky panel data");

        ErosionSaveData_v1 data = new ErosionSaveData_v1();

        data.erosionBrushRadius =  erosionData.erosionBrushRadius;
        data.lifetime = erosionData.lifetime;
        data.sedimentCapacityFactor =  erosionData.sedimentCapacityFactor;
        data.minSedimentCapacity =  erosionData.minSedimentCapacity;
        data.inertia =  erosionData.inertia;
        data.depositSpeed = erosionData.depositSpeed;
        data.erodeSpeed =  erosionData.erodeSpeed;
        data.startSpeed = erosionData.startSpeed;
        data.evaporateSpeed = erosionData.evaporateSpeed;
        data.startWater = erosionData.startWater;
        data.gravity = erosionData.gravity;        

        return JsonUtility.ToJson(data);
    }

    public void FromJson(string dataString)
    {
        if(dataString != null && dataString != "") {
            ErosionSaveData_v1 data = JsonUtility.FromJson<ErosionSaveData_v1>(dataString);

            lifetimeSlider.value = data.lifetime;
            sedimentCapacityFactorSlider.value = data.sedimentCapacityFactor ;
            inertiaSlider.value = data.inertia;
            depositSpeedSlider.value = data.depositSpeed;
            erodeSpeedSlider.value = data.erodeSpeed;
            startSpeedSlider.value = data.startSpeed;
            evaporateSpeedSlider.value = data.evaporateSpeed;
            startWaterSlider.value = data.startWater;
        } else {

        }
    }
    public void BrushButtonClick()
    {
        bool active = !erosionBrushPanel.activeSelf;

        sidePanels.GetComponent<PanelController>().CloseAllPanels();

        if(active) {
            sidePanels.SetActive(true);
            erosionBrushPanel.SetActive(true);
            brushImage.color = settingsData.selectedColor;
        } else {
            brushImage.color = settingsData.deselectedColor;
        }
    }


    public void SelectBrushIcon(int buttonIndex)
    {
        brushData.brush = gameResources.erosionBrushes[buttonIndex];
        brushImage.texture = brushData.brush;
        brushIndex = buttonIndex;

        if(buttonIndex >= (gameResources.erosionBrushes.Count - internalData.customErosionBrushes.Count)) {
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
        brushData.brushRotation = value;
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
            controller.LoadCustomErosionBrush(filename);
            internalData.customErosionBrushes.Add(filename);
            SelectBrushIcon(gameResources.erosionBrushes.Count - 1);
        }
    }

    public void BrushDeleteButtonClick()
    {
        int customBrushIndex = brushIndex + internalData.customErosionBrushes.Count - gameResources.erosionBrushes.Count;

        internalData.customErosionBrushes.RemoveAt(customBrushIndex);
        gameResources.erosionBrushes.RemoveAt(brushIndex);
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
        newButton.transform.SetParent(erosionBrushScrollView.transform);
        brushIcons.Add(newButton);
    }
}

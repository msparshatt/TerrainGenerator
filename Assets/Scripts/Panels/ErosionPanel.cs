using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class ErosionPanel : MonoBehaviour, IPanel
{
    [Header("UI elements")]
    [SerializeField] private GameObject sculptBrushScrollView;
    [SerializeField] private GameObject sculptBrushPanel;
    [SerializeField] private GameObject sidePanels;
    [SerializeField] private Button brushDeleteButton;
    [SerializeField] private RawImage brushImage;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject gameState;

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
        
        //brushIcons = UIHelper.SetupPanel(gameResources.brushes, sculptBrushScrollView.transform, SelectBrushIcon);   

        SelectBrushIcon(0);
    }

    public void ResetPanel()
    {
        SelectBrushIcon(0);
    }

    public void SelectBrushIcon(int buttonIndex)
    {
        brushData.brush = gameResources.brushes[buttonIndex];
        brushImage.texture = brushData.brush;
        brushIndex = buttonIndex;

        /*if(buttonIndex >= (gameResources.brushes.Count - internalData.customSculptBrushes.Count)) {
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
        }*/
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
}

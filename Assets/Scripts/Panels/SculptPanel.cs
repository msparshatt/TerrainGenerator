using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SculptPanel : MonoBehaviour
{
    [Header("UI elements")]
    [SerializeField] private GameObject sculptBrushScrollView;
    [SerializeField] private GameObject sculptBrushPanel;
    [SerializeField] private Button brushDeleteButton;
    [SerializeField] private RawImage brushImage;


    [Header("Data Objects")]
    [SerializeField] private BrushDataScriptable brushData;
    [SerializeField] private SettingsDataScriptable settingsData;

    private List<GameObject> brushIcons;
    private List<string> customBrushes;
    private int brushIndex;

    private GameResources gameResources;


    // Start is called before the first frame update
    void Start()
    {
        gameResources = GameResources.instance;
        UIHelper.SetupPanel(gameResources.brushes, sculptBrushScrollView.transform, SelectBrushIcon);   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectBrushIcon(int buttonIndex)
    {
        brushData.brush = gameResources.brushes[buttonIndex];
        brushImage.texture = brushData.brush;
        brushIndex = buttonIndex;

        if(buttonIndex >= (gameResources.brushes.Count - customBrushes.Count)) {
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

    public void BrushButtonClick()
    {
        bool active = !sculptBrushPanel.activeSelf;
        //CloseAllPanels();
        sculptBrushPanel.SetActive(active);

        if(active)
            brushImage.color = settingsData.selectedColor;
    }
}

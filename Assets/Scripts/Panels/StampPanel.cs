using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SimpleFileBrowser;

public class StampPanel : MonoBehaviour, IPanel
{
   [Header("UI elements")]
    [SerializeField] private GameObject stampBrushScrollView;
    [SerializeField] private GameObject stampBrushPanel;
    [SerializeField] private GameObject sidePanels;
    [SerializeField] private Button brushDeleteButton;
    [SerializeField] private RawImage brushImage;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject gameState;

    [Header("UI Elements")]
    [SerializeField] private Slider radiusSlider;
    [SerializeField] private Slider rotationSlider;
    [SerializeField] private Slider heightSlider;

    [Header("Data Objects")]
    [SerializeField] private BrushDataScriptable brushData;
    [SerializeField] private BrushDataScriptable defaultBrushData;
    [SerializeField] private SettingsDataScriptable settingsData;
    [SerializeField] private InternalDataScriptable internalData;

    private List<GameObject> brushIcons;
    private int brushIndex;

    private GameResources gameResources;
    private Controller controller;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitialisePanel()
    {
        gameResources = GameResources.instance;
        controller = gameState.GetComponent<Controller>();
        brushIcons = UIHelper.SetupPanel(gameResources.stampBrushes, stampBrushScrollView.transform, SelectBrushIcon);

        ResetPanel();
    }

    public void ResetPanel()
    {
        SelectBrushIcon(0);

        radiusSlider.value = defaultBrushData.brushRadius;
        rotationSlider.value = defaultBrushData.brushRotation;
        heightSlider.value = defaultBrushData.brushStrength;
    }
    
    public void BrushButtonClick()
    {
        bool active = !stampBrushPanel.activeSelf;

        sidePanels.GetComponent<PanelController>().CloseAllPanels();

        if(active) {
            sidePanels.SetActive(true);
            stampBrushPanel.SetActive(true);
            brushImage.color = settingsData.selectedColor;
        } else {
            brushImage.color = settingsData.deselectedColor;
        }
    }

    public void SelectBrushIcon(int buttonIndex)
    {
        brushData.brush = gameResources.stampBrushes[buttonIndex];
        brushImage.texture = brushData.brush;
        brushIndex = buttonIndex;

        if(buttonIndex >= (gameResources.brushes.Count - internalData.customStampBrushes.Count)) {
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

    public void RotationSliderChange(float value)
    {
        brushData.brushRotation = value;
    }

    public void HeightSliderChange(float value)
    {
        brushData.brushStrength = value;
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
            controller.LoadCustomStampBrush(filename);
            internalData.customStampBrushes.Add(filename);
            SelectBrushIcon(gameResources.stampBrushes.Count - 1);
        }
    }

    public void BrushDeleteButtonClick()
    {
        int customBrushIndex = brushIndex + internalData.customStampBrushes.Count - gameResources.stampBrushes.Count;

        internalData.customStampBrushes.RemoveAt(customBrushIndex);
        gameResources.stampBrushes.RemoveAt(brushIndex);
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
        newButton.transform.SetParent(stampBrushScrollView.transform);
        brushIcons.Add(newButton);
    }
}

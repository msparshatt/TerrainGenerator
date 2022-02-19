using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SimpleFileBrowser;

public class SculptPanel : MonoBehaviour
{
    [Header("UI elements")]
    [SerializeField] private GameObject sculptBrushScrollView;
    [SerializeField] private GameObject sculptBrushPanel;
    [SerializeField] private Button brushDeleteButton;
    [SerializeField] private RawImage brushImage;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject gameState;


    [Header("Data Objects")]
    [SerializeField] private BrushDataScriptable brushData;
    [SerializeField] private SettingsDataScriptable settingsData;

    private List<GameObject> brushIcons;
    private List<string> customBrushes;
    private int brushIndex;

    private GameResources gameResources;
    private Controller controller;


    // Start is called before the first frame update
    void Start()
    {
    }

    public void InitialiseSculptPanel()
    {
        gameResources = GameResources.instance;
        controller = gameState.GetComponent<Controller>();
        
        brushIcons = UIHelper.SetupPanel(gameResources.brushes, sculptBrushScrollView.transform, SelectBrushIcon);   

        SelectBrushIcon(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BrushButtonClick()
    {
        bool active = !sculptBrushPanel.activeSelf;
        //CloseAllPanels();
        sculptBrushPanel.SetActive(active);

        if(active)
            brushImage.color = settingsData.selectedColor;
    }

    public void SelectBrushIcon(int buttonIndex)
    {
        brushData.brush = gameResources.brushes[buttonIndex];
        brushImage.texture = brushData.brush;
        brushIndex = buttonIndex;

        if(buttonIndex >= (gameResources.brushes.Count - controller.customBrushes.Count)) {
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
            SelectBrushIcon(gameResources.brushes.Count - 1);
        }
    }

    public void BrushDeleteButtonClick()
    {
        int customBrushIndex = brushIndex + customBrushes.Count - gameResources.brushes.Count;

        customBrushes.RemoveAt(customBrushIndex);
        gameResources.brushes.RemoveAt(brushIndex);
        Destroy(brushIcons[brushIndex]);
        brushIcons.RemoveAt(brushIndex);
        
        SelectBrushIcon(0);
    }

    public void AddButton(Texture2D texture)
    {
        GameObject newButton;
        int ObjectIndex = brushIcons.Count;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = UIHelper.MakeButton(texture, delegate {SelectBrushIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(sculptBrushScrollView.transform);
        brushIcons.Add(newButton);
    }
}

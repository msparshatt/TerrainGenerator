using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using UnityEngine.InputSystem;
using TMPro;

public class SystemPanel : MonoBehaviour, IPanel
{

    [Header("UI elements")]
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private GameObject aboutPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingButton;
    [SerializeField] private GameObject exitConfirmationPanel;
    [SerializeField] private GameObject unsavedChangesText;
    [SerializeField] private GameObject saveChangesButton;
    [SerializeField] private GameObject proceduralPanel;
    [SerializeField] private PlayerInput playerInput;

    [Header("Export settings")]
    [SerializeField] private Dropdown scaleDropdown;
    [SerializeField] private Texture2D busyCursor;

    [Header("Data objects")]
    [SerializeField] private SettingsDataScriptable settingsData;
    [SerializeField] private InternalDataScriptable internalData;
    [SerializeField] private MaterialSettings materialSettings;

    [SerializeField] private GameObject gameState;

    private string savefileName;
    Serialisation serialiser;
    Controller controller;

    private TerrainManager manager;
    private HeightmapController heightmapController;    
    private MaterialController materialController;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void InitialisePanel()
    {
        manager = TerrainManager.Instance();
        heightmapController = manager.HeightmapController;
        materialController = manager.MaterialController;

        serialiser = gameState.GetComponent<Serialisation>();
        controller = gameState.GetComponent<Controller>();
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    //new terrain panel
    public void FlatButtonClick()
    {
        proceduralPanel.SetActive(false);
        heightmapController.CreateFlatTerrain();
        materialController.ApplyTextures();
    }

    public void HeightmapButtonClick()
    {
        proceduralPanel.SetActive(false);
		FileBrowser.SetFilters( true, new FileBrowser.Filter( "heightmap files", ".png", ".raw"));
        FileBrowser.SetDefaultFilter( ".raw" );

        playerInput.enabled = false;
        FileBrowser.ShowLoadDialog((filenames) => {playerInput.enabled = true; heightmapController.CreateTerrainFromHeightmap(filenames[0]);}, () => {playerInput.enabled = true; Debug.Log("Canceled Load");}, FileBrowser.PickMode.Files);
    }

    public void ProceduralButtonClick()
    {
        proceduralPanel.SetActive(!proceduralPanel.activeSelf);
        internalData.ProcGenOpen = true;

        if(proceduralPanel.activeSelf == false)
            proceduralPanel.GetComponent<ProceduralControlPanel>().CancelButtonClick();
    }


    //load/save panel
    public void LoadButtonClick()
    {
		FileBrowser.SetFilters( true, new FileBrowser.Filter( "Save files", ".json"));
        FileBrowser.SetDefaultFilter( ".json" );

        playerInput.enabled = false;
        FileBrowser.ShowLoadDialog((filenames) => {playerInput.enabled = true; serialiser.Load(filenames[0]);}, () => {playerInput.enabled = true; Debug.Log("Canceled Load");}, FileBrowser.PickMode.Files);
    }


    public void SaveButtonClick(bool exitOnSave = false)
    {
        Debug.Log("SAVE: Opening file browser");
		FileBrowser.SetFilters( false, new FileBrowser.Filter( "Save files", ".json"));

        playerInput.enabled = false;
        FileBrowser.ShowSaveDialog((filenames) => {playerInput.enabled = true; serialiser.Save(filenames[0], exitOnSave);}, () => {playerInput.enabled = true; Debug.Log("Canceled save");}, FileBrowser.PickMode.Files);
    }

    //export panel
    public void ExportButtonClick()
    {
        float scalefactor = 0.02f * Mathf.Pow(2, scaleDropdown.value); //reduce the size so it isn't too large for FlowScape
		FileBrowser.SetFilters( false, new FileBrowser.Filter( "Obj files", ".obj"));

        playerInput.enabled = false;
        FileBrowser.ShowSaveDialog((filenames) => {playerInput.enabled = true;  heightmapController.ExportTerrainAsObj(filenames[0], materialSettings.ambientOcclusion, scalefactor);}, () => {playerInput.enabled = true; Debug.Log("Canceled save");}, FileBrowser.PickMode.Files);

        //exportTerrain.Export(aoToggle.isOn, scaleSlider.value);
    }

    public void ExportHmButtonClick()
    {
		FileBrowser.SetFilters( false, new FileBrowser.Filter( "Raw heightmap", ".raw"));

        playerInput.enabled = false;
        FileBrowser.ShowSaveDialog((filenames) => {playerInput.enabled = true;  heightmapController.ExportTerrainAsRaw(filenames[0]);}, () => {playerInput.enabled = true; Debug.Log("Canceled save");}, FileBrowser.PickMode.Files);        
    }


    //other panel
    public void HelpButtonClick()
    {
        bool active = !helpPanel.activeSelf;
        CloseAllPanels();
        helpPanel.SetActive(active);
    }

    public void SettingsButtonClick()
    {
        bool active = !settingsPanel.activeSelf;
        CloseAllPanels();
        settingsPanel.SetActive(active);
    }

    public void AboutButtonClick()
    {
        aboutPanel.SetActive(true);
    }

    public void ResetButtonClick()
    {
        controller.Reset();
    }

    private void CloseAllPanels()
    {
/*        materialListPanel.SetActive(false);
        materialPanel.SetActive(false);
        brushPanel.SetActive(false);
        texturePanel.SetActive(false);
        helpPanel.SetActive(false);
        settingsPanel.SetActive(false);

        brushImage.color = deselectedColor;
        textureImage.color = deselectedColor;*/
    }

    public void ExitButtonClick()
    {
        exitConfirmationPanel.SetActive(true);
        unsavedChangesText.SetActive(internalData.unsavedChanges);
        saveChangesButton.SetActive(internalData.unsavedChanges);        
    }
}

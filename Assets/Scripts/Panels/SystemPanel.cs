using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using UnityEngine.InputSystem;
using TMPro;

public class SystemPanel : MonoBehaviour
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

    [SerializeField] private GameObject gameState;

    private TerrainManager manager;
    private string savefileName;
    Serialisation serialiser;
    Controller controller;
    

    // Start is called before the first frame update
    void Start()
    {
    }

    public void InitialiseSystemPanel()
    {
        manager = TerrainManager.instance;
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
        manager.CreateFlatTerrain();
        manager.ApplyTextures();
    }

    public void HeightmapButtonClick()
    {
        proceduralPanel.SetActive(false);
		FileBrowser.SetFilters( true, new FileBrowser.Filter( "heightmap files", ".png", ".raw"));
        FileBrowser.SetDefaultFilter( ".raw" );

        playerInput.enabled = false;
        FileBrowser.ShowLoadDialog((filenames) => {playerInput.enabled = true; manager.CreateTerrainFromHeightmap(filenames[0]);}, () => {playerInput.enabled = true; Debug.Log("Canceled Load");}, FileBrowser.PickMode.Files);
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


    public int RemapTextureIndex(int index)
    {
        int[] newIndices = {0, 1, 7, 8, 9, 17, 18, 25, 26, 27, 28, 29, 35, 36, 37, 44, 45, 52, 22, 69, 70, 56, 57, 53, 54};

        Debug.Log(index + " : " + newIndices[index]);
        return newIndices[index];
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
        FileBrowser.ShowSaveDialog((filenames) => {playerInput.enabled = true;  manager.ExportTerrainAsObj(filenames[0], internalData.ambientOcclusion, scalefactor);}, () => {playerInput.enabled = true; Debug.Log("Canceled save");}, FileBrowser.PickMode.Files);

        //exportTerrain.Export(aoToggle.isOn, scaleSlider.value);
    }

    public void ExportHmButtonClick()
    {
		FileBrowser.SetFilters( false, new FileBrowser.Filter( "Raw heightmap", ".raw"));

        playerInput.enabled = false;
        FileBrowser.ShowSaveDialog((filenames) => {playerInput.enabled = true;  manager.ExportTerrainAsRaw(filenames[0]);}, () => {playerInput.enabled = true; Debug.Log("Canceled save");}, FileBrowser.PickMode.Files);        
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
        FlatButtonClick();
        /*ClearButtonClick();

        SelectBrushIcon(0);
        SelectMaterialIcon(0, 0);
        SelectMaterialIcon(1, 1);
        SelectMaterialIcon(2, 2);
        SelectMaterialIcon(3, 3);
        SelectMaterialIcon(4, 4);

        for(int index = 1; index < 5; index++) {
            mixFactorSliders[index].value = 0;
            mixtypeDropdowns[index].value = 0;
        }

        SelectTextureIcon(1);
    */
        internalData.unsavedChanges = false;
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
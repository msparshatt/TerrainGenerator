using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using SimpleFileBrowser;

public class ExitPanel : MonoBehaviour
{
    [SerializeField] private GameObject gameState;
    [SerializeField] private PlayerInput playerInput;
    Serialisation serialiser;

    private Controller controller;
    public void Start()
    {
        controller = gameState.GetComponent<Controller>();
        serialiser = gameState.GetComponent<Serialisation>();
    }

    public void NoButtonClick()
    {
        gameObject.SetActive(false);
    }

    public void YesButtonClick()
    {
        controller.DoExit();
    }

    public void ExitPanelSaveClick()
    {
        Debug.Log("SAVE: Opening file browser");
		FileBrowser.SetFilters( false, new FileBrowser.Filter( "Save files", ".json"));

        playerInput.enabled = false;
        FileBrowser.ShowSaveDialog((filenames) => {playerInput.enabled = true; serialiser.Save(filenames[0], true);}, () => {playerInput.enabled = true; Debug.Log("Canceled save");}, FileBrowser.PickMode.Files);
    }
}

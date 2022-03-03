using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SimpleFileBrowser;

public class StampPanel : MonoBehaviour
{
   [Header("UI elements")]
    [SerializeField] private GameObject sculptBrushScrollView;
    [SerializeField] private GameObject sculptBrushPanel;
    [SerializeField] private GameObject sidePanels;
    [SerializeField] private Button brushDeleteButton;
    [SerializeField] private RawImage brushImage;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject gameState;


    [Header("Data Objects")]
    [SerializeField] private BrushDataScriptable brushData;
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

    public void InitialiseStampPanel()
    {
    }
}

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using UnityEngine.InputSystem;

public class PaintPanel : MonoBehaviour
{
    [Header("Brush Elements")]
    [SerializeField] private GameObject paintBrushScrollView;
    [SerializeField] private GameObject paintBrushPanel;
    [SerializeField] private Button paintBrushDeleteButton;
    [SerializeField] private RawImage paintBrushImage;

    [Header("Texture Elements")]
    [SerializeField] private GameObject textureScrollView;
    [SerializeField] private GameObject texturePanel;
    [SerializeField] private Button textureDeleteButton;
    [SerializeField] private RawImage textureImage;
    [SerializeField] private Slider paintScaleSlider;



    [Header("Elements")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Data Objects")]
    [SerializeField] private BrushDataScriptable paintBrushData;
    [SerializeField] private SettingsDataScriptable settingsData;

    private GameResources gameResources;
    private List<GameObject> textureIcons;
    private List<string> customTextures;
    private int textureIndex;
    private TerrainManager manager;

    // Start is called before the first frame update
    void Start()
    {
        gameResources = GameResources.instance;
        textureIcons = UIHelper.SetupPanel(gameResources.icons, textureScrollView.transform, SelectTextureIcon);           
        manager = TerrainManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectTextureIcon(int buttonIndex)
    {
        paintBrushData.paintTexture = (Texture2D)gameResources.textures[buttonIndex];
        textureImage.texture = gameResources.icons[buttonIndex];
        textureIndex = buttonIndex;

        if(buttonIndex >= (gameResources.icons.Count - customTextures.Count)) {
            textureDeleteButton.interactable = true;
        } else {
            textureDeleteButton.interactable = false;
        }        

        for (int i = 0; i < textureIcons.Count; i++) {
            if(i == buttonIndex) {
                textureIcons[i].GetComponent<Image>().color = Color.green;

            } else {
                textureIcons[i].GetComponent<Image>().color = Color.white;
            }
        }
    }
    public void TextureDeleteButtonClick()
    {
        int customTextureIndex = textureIndex + customTextures.Count - gameResources.textures.Count;

        customTextures.RemoveAt(customTextureIndex);
        gameResources.textures.RemoveAt(textureIndex);
        Destroy(textureIcons[textureIndex]);
        textureIcons.RemoveAt(textureIndex);
        
        SelectTextureIcon(0);
    }

    public void LoadCustomTexture(string filename)
    {
        Texture2D texture = new Texture2D(128,128, TextureFormat.RGB24, false); 
        byte[] bytes = File.ReadAllBytes(filename);

        texture.filterMode = FilterMode.Trilinear;
        texture.LoadImage(bytes);

        gameResources.textures.Add(texture);

        //Add the brush to the  brush selection panel          
        GameObject newButton;
        int ObjectIndex = textureIcons.Count;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        newButton = UIHelper.MakeButton(texture, delegate {SelectTextureIcon(ObjectIndex); }, ObjectIndex);
        newButton.transform.SetParent(paintBrushScrollView.transform);
        textureIcons.Add(newButton);
    }
    public void OnTextureImport(string filename)        
    {       
        if(filename != "") {
            LoadCustomTexture(filename);
            customTextures.Add(filename);

            SelectTextureIcon(gameResources.textures.Count - 1);
        }
    }

    public void TextureImportButtonClick()
    {
		FileBrowser.SetFilters( true, new FileBrowser.Filter( "Image files", new string[] {".png", ".jpg", ".jpeg"}));
        FileBrowser.SetDefaultFilter( ".png" );

        playerInput.enabled = false;
        FileBrowser.ShowLoadDialog((filenames) => {playerInput.enabled = true;  OnTextureImport(filenames[0]);}, () => {playerInput.enabled = true; Debug.Log("Canceled Load");}, FileBrowser.PickMode.Files);
    }

    public void ClearButtonClick()
    {
        manager.ClearOverlay();
    }

    public void PaintScaleSliderChange(float value)
    {
        paintBrushData.textureScale = value;
    }

    public void PaintResetTilingButtonClick()
    {
        paintScaleSlider.value = 1.0f;
    }
}

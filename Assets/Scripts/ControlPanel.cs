using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.FB;
using UnityEngine.Events;
//using UnityEngine.JSONSerializeModule;

//[Serializable]
public struct SaveData
{
    public byte[] heightmap;
    public int baseTexture;
    public byte[] overlayTexture;
} 


public class ControlPanel : MonoBehaviour
{
    //object that handles exporting the terrain data
    [SerializeField] private Terrain currentTerrain;


    [Header("UI elements")]
    [SerializeField] private Text modeText;
    [SerializeField] private GameObject brushScrollView;
    [SerializeField] private GameObject brushPanel;
    [SerializeField] private GameObject materialScrollView;
    [SerializeField] private GameObject materialPanel;
    [SerializeField] private GameObject textureScrollView;
    [SerializeField] private GameObject texturePanel;
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private RawImage materialImage;
    [SerializeField] private RawImage brushImage;
    [SerializeField] private RawImage textureImage;
    [SerializeField] private Button textureButton;
    [SerializeField] private Slider scaleSlider;
    [SerializeField] private GameObject proceduralPanel;

    [Header("brush settings")]
    [SerializeField] private BrushDataScriptable brushData;

    [Header("Export settings")]
    [SerializeField] private Dropdown scaleDropdown;
    [SerializeField] private Texture2D busyCursor;

    //The currently used Material
    private Material currentMaterial;
    private int currentMaterialIndex;

    //buttons created to select brushes,materials and textures
    private List<GameObject> brushIcons;
    private List<GameObject> materialIcons;
    private List<GameObject> textureIcons;

    //UI colours
    private Color selectedColor;
    private Color deselectedColor;

    //terrain dimensions    
    private int terrainSize;
    private int terrainHeight;

    //assets from the resource folder used by the game
    private GameResources gameResources;

    //export objects
    private ExportHeightmap exportHeightmap;
    private ExportTerrain exportTerrain;

    public void Start() 
    {
        //cache the instance of the GameResources object
        gameResources = GameResources.instance;
        exportHeightmap = ExportHeightmap.instance;
        exportHeightmap.terrainObject = currentTerrain;
        exportTerrain = ExportTerrain.instance;
        exportTerrain.terrainObject = currentTerrain;
        exportTerrain.scaleDropDown = scaleDropdown;
        exportTerrain.busyCursor = busyCursor;

        CloseAllPanels();

        selectedColor = Color.green;
        deselectedColor = Color.white;

        terrainSize = 1000;
        terrainHeight = 1000;

        Debug.Log("creating terrain " + Time.realtimeSinceStartup);
        TerrainManager.instance.currentMaterial = currentMaterial;
        TerrainManager.instance.currentTerrain = currentTerrain;
        TerrainManager.instance.CreateFlatTerrain(terrainSize, terrainSize, terrainHeight);

        //set up brush settings
        brushData.brushRadius = 50;
        brushData.brushStrength = 0.05f;
        brushData.textureScale = 1.0f;

        //create selection panels
        SetupPanels();

        //Debug.Log("loaded " + Time.realtimeSinceStartup);
        SelectMaterialIcon(0);
        SelectBrushIcon(0);
        SelectTextureIcon(1);
        SwitchMode(BrushDataScriptable.Modes.Sculpt);
        //Debug.Log("end of start method " + Time.realtimeSinceStartup);
    }

    private void SetupPanels()
    {
        //populate material selection panel          
        materialIcons = SetupIcons(gameResources.icons, materialScrollView.transform, SelectMaterialIcon);
        textureIcons = SetupIcons(gameResources.icons, textureScrollView.transform, SelectTextureIcon);
        brushIcons = SetupIcons(gameResources.brushes, brushScrollView.transform, SelectBrushIcon);

    }

    //creates a list of buttons based off a list of images, parented to the passed in transform
    private List<GameObject> SetupIcons(Texture2D[] images, Transform parent, Action<int> onClickFunction)
    {
        //populate material selection panel          
        GameObject newButton;
        List<GameObject> buttons = new List<GameObject>();
        int ObjectIndex = 0;
        Vector2 scale = new Vector2(1.0f, 1.0f);

        foreach (Texture2D icon in images)
        {
            int oi = ObjectIndex; //need this to make sure the closure gets the right value
 
            newButton = MakeButton(icon, delegate {onClickFunction(oi); }, oi);
            newButton.transform.SetParent(parent);
            buttons.Add(newButton);
            ObjectIndex++;
        }

        return buttons;
    }

    //create an image button. It will call the passed onClickListener action when clicked
    private GameObject MakeButton(Texture2D icon, UnityAction onClickListener, int index=0)
    {
            GameObject NewObj = new GameObject("button" + index); //Create the GameObject
            Image NewImage = NewObj.AddComponent<Image>(); //Add the Image Component script
            NewImage.rectTransform.sizeDelta = new Vector2(50, 50);
            NewImage.sprite = Sprite.Create(icon, new Rect(0,0,icon.width,icon.height), new Vector2()); //Set the Sprite of the Image Component on the new GameObject

            Button NewButton = NewObj.AddComponent<Button>();
            NewButton.onClick.AddListener(onClickListener);

            NewObj.SetActive(true); //Activate the GameObject    

            return NewObj;
    }    

    //create a new transparent texture and add it to the material in the _OverlayTexture slot
    private void CreateOverlayTexture(Material mat, Vector2 size)
    {
        Texture2D newTexture = new Texture2D((int)size.x, (int)size.y);// GraphicsFormat.R8G8B8A8_UNorm, true);

        Color[] data = new Color[(int)size.x * (int)size.y];

        int index = 0;
        //set the every pixel to be transparent
        for(int x = 0; x < size.x; x++) {
            for(int y = 0; y < size.y; y++) {                        
                data[index] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                index++;
            }
        }

        newTexture.SetPixels(0, 0, (int)size.x, (int)size.y, data);
        newTexture.Apply(true);

        mat.SetTexture("_OverlayTexture", newTexture);
    }

    public void FlatButtonClick()
    {
        TerrainManager.instance.CreateFlatTerrain(terrainSize, terrainSize, terrainHeight);
    }

    public void HeightmapButtonClick()
    {
        string filename = FileBrowser.OpenSingleFile("Open Heightmap file", "", new string[]{"raw", "png"});

        if(filename != "") {
            TerrainManager.instance.CreateTerrainFromHeightmap(terrainSize, terrainSize, terrainHeight, filename);
        }
    }

    public void ProceduralButtonClick()
    {
        proceduralPanel.SetActive(!proceduralPanel.activeSelf);
    }

    public void RadiusSliderChange(float value)
    {
        brushData.brushRadius = (int)value;
    }

    public void StrengthSliderChange(float value)
    {
        brushData.brushStrength = value;
    }

    public void ExitButtonClick()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
        #endif    
    }

    public void BrushButtonClick()
    {
        bool active = !brushPanel.activeSelf;
        CloseAllPanels();
        brushPanel.SetActive(active);
    }

    public void MaterialButtonClick()
    {
        bool active = !materialPanel.activeSelf;
        CloseAllPanels();
        materialPanel.SetActive(active);
    }

    public void TextureButtonClick()
    {
        bool active = !texturePanel.activeSelf;
        CloseAllPanels();
        texturePanel.SetActive(active);
    }

    public void HelpButtonClick()
    {
        bool active = !helpPanel.activeSelf;
        CloseAllPanels();
        helpPanel.SetActive(active);
    }

    public void SelectBrushIcon(int buttonIndex)
    {
        brushData.brush = gameResources.brushes[buttonIndex];
        brushImage.texture = brushData.brush;

        for (int i = 0; i < brushIcons.Count; i++) {
            if(i == buttonIndex) {
                brushIcons[i].GetComponent<Image>().color = selectedColor;

            } else {
                brushIcons[i].GetComponent<Image>().color = deselectedColor;
            }
        }
    }

    public void SelectTextureIcon(int buttonIndex)
    {
        brushData.paintTexture = (Texture2D)gameResources.materials[buttonIndex].mainTexture;
        textureImage.texture = brushData.paintTexture;

        for (int i = 0; i < textureIcons.Count; i++) {
            if(i == buttonIndex) {
                textureIcons[i].GetComponent<Image>().color = Color.green;

            } else {
                textureIcons[i].GetComponent<Image>().color = Color.white;
            }
        }
    }

    public void SelectMaterialIcon(int buttonIndex)
    {
        currentMaterial = gameResources.materials[buttonIndex];
        currentMaterialIndex = buttonIndex;
        materialImage.texture = currentMaterial.mainTexture;
        TerrainManager.instance.SetTerrainMaterial(gameResources.materials[buttonIndex]);

        for (int i = 0; i < materialIcons.Count; i++) {
            if(i == buttonIndex) {
                materialIcons[i].GetComponent<Image>().color = Color.green;

            } else {
                materialIcons[i].GetComponent<Image>().color = Color.white;
            }
        }
    }

    public void ScaleSliderChange(float value)
    {
        Vector2 scale = new Vector2(value, value);
        currentMaterial.mainTextureScale = scale;
        currentMaterial.SetTextureScale("_OverlayTexture", scale);
        brushData.textureScale = value;
    }

    public void SwitchMode(BrushDataScriptable.Modes newMode)
    {
        brushData.brushMode = newMode;

        if(newMode == BrushDataScriptable.Modes.Sculpt) {
            modeText.text = "Sculpt";
            textureButton.interactable = false;
        } else {
            modeText.text = "Paint";
            textureButton.interactable = true;
        }
    }

    public void ModeButtonClick()
    {
        if(brushData.brushMode == BrushDataScriptable.Modes.Sculpt) {
            SwitchMode(BrushDataScriptable.Modes.Paint);
        } else {
            SwitchMode(BrushDataScriptable.Modes.Sculpt);
        }
    }

    public void ClearButtonClick()
    {
        Texture2D overlayTexture = (Texture2D)currentMaterial.GetTexture("_OverlayTexture");
        int sizeX = overlayTexture.width;
        int sizeY = overlayTexture.height;

        Color[] data = new Color[sizeX * sizeY];

        int index = 0;
        //set the pixel data to transparent        
        for(int x = 0; x < sizeX; x++) {
            for(int y = 0; y < sizeY; y++) {                        
                data[index] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                index++;
            }
        }

        overlayTexture.SetPixels(0, 0, sizeX, sizeY, data);
        overlayTexture.Apply(true);
    }

    private void CloseAllPanels()
    {
        materialPanel.SetActive(false);
        brushPanel.SetActive(false);
        texturePanel.SetActive(false);
        helpPanel.SetActive(false);
    }

    public void SaveButtonClick()
    {
        string filename = FileBrowser.SaveFile("save.json", "json");

        if(filename != "") {
            SaveData data = new SaveData();

            data.heightmap = exportHeightmap.GetHeightmap();
            data.baseTexture = currentMaterialIndex;
            Texture2D texture = (Texture2D)currentMaterial.GetTexture("_OverlayTexture");
            data.overlayTexture = texture.EncodeToPNG();

            string json = JsonUtility.ToJson(data);

            var sr = File.CreateText(filename);
            sr.WriteLine (json);
            sr.Close();
        }
    }

    public void LoadButtonClick()
    {
        string filename = FileBrowser.OpenSingleFile("Open Heightmap file", "", "json");

        if(filename != "") {
            var sr = new StreamReader(filename);
            string fileContents = sr.ReadToEnd();
            sr.Close();        

            SaveData data = JsonUtility.FromJson<SaveData>(fileContents);

            TerrainManager.instance.CreateTerrainFromHeightmap(terrainSize, terrainSize, terrainHeight, data.heightmap);
            SelectMaterialIcon(data.baseTexture);
            Texture2D texture = new Texture2D(10,10);
            ImageConversion.LoadImage(texture, data.overlayTexture);

            currentMaterial.SetTexture("_OverlayTexture", texture);
        }        
    }

    public void ExportButtonClick()
    {
        exportTerrain.Export();
    }

    public void ExportHmButtonClick()
    {
        exportHeightmap.Export();
    }

    public void ResetTilingButtonClick()
    {
        scaleSlider.value = 1.0f;
    }
}

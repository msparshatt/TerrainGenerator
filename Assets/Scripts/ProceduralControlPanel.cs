using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GeneratorMode {perlin, ds_perlin, ds_random, maxVal};
public class ProceduralControlPanel : MonoBehaviour
{
    [Header("Tabs")]
    public GameObject[] panels;
    public Button[] buttons;
    public Sprite unselectedTab;
    public Sprite selectedTab;


    [Header("Perlin")]
    public Slider xOffsetSlider;
    public Slider yOffsetSlider;
    public Slider scaleSlider;
    public Slider iterationSlider;

    [Header("Voronoi")]
    public Slider voronoiXOffsetSlider;
    public Slider voronoiyOffsetSlider;
    public Slider voronoiCellSizeSlider;
    public Slider voronoiRandomSlider;

    [Header("Factor")]
    public Slider factorSlider;

    [Header("Settings")]
    public Slider minimumHeightSlider;
    public Toggle clampToggle;


    [Header("Terrace UI")]
    public Toggle terraceToggle;
    public Toggle layer1Toggle;
    public Slider layer1CountSlider;
    public Slider layer1ShaperSlider;

    public Toggle layer2Toggle;
    public Slider layer2CountSlider;
    public Slider layer2ShaperSlider;

    public Toggle layer3Toggle;
    public Slider layer3CountSlider;
    public Slider layer3ShaperSlider;

    [Header("Erosion UI")]
    public Toggle erodeToggle;
    public Slider erosionIterationsSlider;
    public Slider erosionFactorsetSlider;

    [Header("Terrain")]
    public Terrain currentTerrain;

    [Header("Other")]
    public SettingsDataScriptable settingsData;

    public Texture2D busyCursor;
    public RawImage heightmapImage;

    //objects to handle the stages of generation
    ProceduralGeneration procGen;
    TerraceSettings terrace;
    Erosion erosion;

    private TerrainManager manager;

    private GeneratorMode mode;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("start");
        //resolution = currentTerrain.terrainData.heightmapResolution;
        procGen = new ProceduralGeneration(settingsData.defaultTerrainResolution);
        terrace = new TerraceSettings();
        erosion = new Erosion();
        manager = TerrainManager.instance;

        gameObject.SetActive(false);
    }

    public void OnEnable()
    {
        TabButtonClick(0);

        if(manager != null) {
            manager.SetupChanges();

            if(procGen != null)
                UpdateTerrain();
        }
    }

    public void TabButtonClick(int index)
    {
        CloseAllTabs();
        panels[index].SetActive(true);
        buttons[index].GetComponent<Image>().sprite = selectedTab;
    }

    private void CloseAllTabs()
    {
        for(int index = 0; index < panels.Length; index++) {
            panels[index].SetActive(false);
            buttons[index].GetComponent<Image>().sprite = unselectedTab;
        }
    }
    public void UpdateTerrain()
    {

        procGen.perlinOffset = new Vector2(xOffsetSlider.value, yOffsetSlider.value);
        procGen.scale = scaleSlider.value;
        procGen.iterations = (int)iterationSlider.value;

        procGen.voronoiOffset = new Vector2(voronoiXOffsetSlider.value, voronoiXOffsetSlider.value);
        procGen.cellSize = voronoiCellSizeSlider.value; // * 100;
        procGen.noiseAmplitude = voronoiRandomSlider.value;

        procGen.factor = factorSlider.value;

        procGen.clampEdges = clampToggle.isOn;
        procGen.minHeight = minimumHeightSlider.value;

        if(erodeToggle.isOn) {
            erosion.isOn = true;
            erosion.iterationCount = (int)erosionIterationsSlider.value;
            erosion.factor = erosionFactorsetSlider.value;
        } else {
            erosion.isOn = false;
            erosion.iterationCount = 0;
            erosion.factor = 0f;
        }

        terrace.ClearLayers();
        if(terraceToggle.isOn) {
            if(layer1Toggle.isOn) {
                terrace.AddLayer(Mathf.FloorToInt(layer1CountSlider.value), layer1ShaperSlider.value);
            }
            if(layer2Toggle.isOn) {
                terrace.AddLayer(Mathf.FloorToInt(layer2CountSlider.value), layer2ShaperSlider.value);
            }
            if(layer3Toggle.isOn) {
                terrace.AddLayer(Mathf.FloorToInt(layer3CountSlider.value), layer3ShaperSlider.value);
            }
        }

        manager.CreateProceduralTerrain(procGen, terrace, erosion);

        heightmapImage.GetComponent<RawImage>().texture = manager.GetHeightmapTexture();
    }

    public void CancelButtonClick()
    {
        manager.RevertChanges();
        gameObject.SetActive(false);
    }

    public void ApplyButtonClick()
    {
        Cursor.SetCursor(busyCursor, Vector2.zero, CursorMode.Auto); 

        //force the cursor to update
        Cursor.visible = false;
        Cursor.visible = true;

        manager.ApplyChanges(procGen, terrace, erosion);

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); 

        //force the cursor to update
        Cursor.visible = false;
        Cursor.visible = true;

        gameObject.SetActive(false);
    }

    public void RedrawButtonClick()
    {
        UpdateTerrain();
    }
}

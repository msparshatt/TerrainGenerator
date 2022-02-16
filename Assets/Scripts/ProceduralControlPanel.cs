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
    public Slider iterationFactorSlider;

    [Header("Voronoi")]
    public Slider voronoiXOffsetSlider;
    public Slider voronoiYOffsetSlider;
    public Slider voronoiCellSizeSlider;
    public Slider voronoiValleySlider;

    [Header("Factor")]
    public Slider factorSlider;

    [Header("Settings")]
    public Slider minimumHeightSlider;
    public Slider heightScaleSlider;
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
    public Slider erosionCapcitySlider;
    public Slider erosionErosionSpeedSlider;
    public Slider erosionDepositSpeedSlider;
    public Slider erosionEvaporationSlider;
    public Slider erosionLifetimeSlider;
    public Slider erosionStartSpeedSlider;
    public Slider erosionStartWaterSlider;
    public Slider erosionInertiaSlider;

    [Header("Terrain")]
    public Terrain currentTerrain;

    [Header("Other")]
    public SettingsDataScriptable settingsData;
    public  FlagsDataScriptable flagsData;
    public ComputeShader proceduralGenerationShader;
    public ComputeShader erosionShader;

    public Texture2D busyCursor;
    public RawImage heightmapImage;

    //objects to handle the stages of generation
    ProceduralGeneration procGen;
    bool erosion;

    private TerrainManager manager;

    private GeneratorMode mode;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("start");
        //resolution = currentTerrain.terrainData.heightmapResolution;
        procGen = new ProceduralGeneration(settingsData.defaultTerrainResolution);
        procGen.proceduralGenerationShader = proceduralGenerationShader;
        erosion = false;
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
        procGen.iterationFactor = iterationFactorSlider.value;

        procGen.voronoiOffset = new Vector2(voronoiXOffsetSlider.value, voronoiYOffsetSlider.value);
        procGen.cellSize = voronoiCellSizeSlider.value; // * 100;
        procGen.voronoiValleys = voronoiValleySlider.value;

        procGen.factor = factorSlider.value;

        procGen.clampEdges = clampToggle.isOn;
        procGen.minHeight = minimumHeightSlider.value;
        procGen.heightscale = heightScaleSlider.value;

        procGen.erosionShader = erosionShader;
        if(erodeToggle.isOn) {
            erosion = true;
            procGen.erosionIsOn = true;
            procGen.erosionIterations = (int)erosionIterationsSlider.value;
            procGen.erosionFactor = erosionFactorsetSlider.value;

            procGen.sedimentCapaFactor = erosionCapcitySlider.value;
            procGen.eroSpeed = erosionErosionSpeedSlider.value;
            procGen.depSpeed = erosionDepositSpeedSlider.value;
            procGen.evaporateSpeed = erosionEvaporationSlider.value;
            procGen.lifetime = (int)erosionLifetimeSlider.value;
            procGen.startSpeed = erosionStartSpeedSlider.value;
            procGen.startWater = erosionStartWaterSlider.value;
            procGen.inertia = erosionInertiaSlider.value;
        } else {
            erosion = false;
            procGen.erosionIsOn = false;
            procGen.erosionIterations = 0;
            procGen.erosionFactor = 0f;
        }

        procGen.ClearLayers();
        if(terraceToggle.isOn) {
            if(layer1Toggle.isOn) {
                procGen.AddLayer(Mathf.FloorToInt(layer1CountSlider.value), layer1ShaperSlider.value);
            }
            if(layer2Toggle.isOn) {
                procGen.AddLayer(Mathf.FloorToInt(layer2CountSlider.value), layer2ShaperSlider.value);
            }
            if(layer3Toggle.isOn) {
                procGen.AddLayer(Mathf.FloorToInt(layer3CountSlider.value), layer3ShaperSlider.value);
            }
        }

        manager.CreateProceduralTerrain(procGen, erosion);

        heightmapImage.GetComponent<RawImage>().texture = manager.GetHeightmapTexture();
    }

    public void CancelButtonClick()
    {
        manager.RevertChanges();
        flagsData.ProcGenOpen = false;
        gameObject.SetActive(false);
    }

    public void ApplyButtonClick()
    {
        Cursor.SetCursor(busyCursor, Vector2.zero, CursorMode.Auto); 

        //force the cursor to update
        Cursor.visible = false;
        Cursor.visible = true;

        manager.ApplyChanges(procGen, erosion);

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); 

        //force the cursor to update
        Cursor.visible = false;
        Cursor.visible = true;

        flagsData.ProcGenOpen = false;
        gameObject.SetActive(false);
    }

    public void RedrawButtonClick()
    {
        UpdateTerrain();
    }
}

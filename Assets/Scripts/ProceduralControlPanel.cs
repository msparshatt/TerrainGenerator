using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GeneratorMode {perlin, ds_perlin, ds_random, maxVal};
public class ProceduralControlPanel : MonoBehaviour
{
    [Header("Tabs")]
    public GameObject[] panels;
    public Button[] buttons;
    public Sprite unselectedTab;
    public Sprite selectedTab;

    [Header("Base Panels")]
    public GameObject[] basePanels;


    [Header("Perlin")]
    public Slider xOffsetSlider;
    public Slider yOffsetSlider;
    public Slider scaleSlider;
    public Slider iterationSlider;
    public Slider iterationFactorSlider;
    public Slider iterationRotationSlider;

    [Header("Voronoi")]
    public Slider voronoiXOffsetSlider;
    public Slider voronoiYOffsetSlider;
    public Slider voronoiCellSizeSlider;
    public TMP_Dropdown voronoiTypeDropdown;

    [Header("Factor")]
    public Slider factorSlider;
    public Slider hillAmplitudeSlider;

    [Header("Mountains")]
    public TMP_Dropdown mountainTypeDropdown;
    public GameObject mountainPanel1;
    public GameObject mountainPanel2;
    public GameObject mountainPanel3;
    public Slider mountainXOffsetSlider;
    public Slider mountainYOffsetSlider;
    public Slider mountainScaleSlider;
    public Slider mountainAmplitudeSlider;
    public Slider mountainIterationsSlider;
    public Slider mountainIterationFactorSlider;
    public Slider mountainIterationRotationSlider;
    public TMP_Dropdown mountainVoronoiTypeDropdown;

    [Header("Plateaus")]
    public Slider plateauXOffsetSlider;
    public Slider plateauYOffsetSlider;
    public Slider plateauScaleSlider;
    public Slider plateauAmplitudeSlider;
    public TMP_Dropdown plateauVoronoiTypeDropdown;
    public Toggle plateauToggle;

    [Header("Settings")]
    public Slider minimumHeightSlider;
    public Slider maximumHeightSlider;
    public Slider heightScaleSlider;
    public Toggle clampToggle;
    public Slider heightClampSlider;
    public Toggle invertToggle;


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
    public  InternalDataScriptable internalData;
    public ComputeShader proceduralGenerationShader;
    public ComputeShader erosionShader;

    public Texture2D busyCursor;
    public RawImage heightmapImage;

    //objects to handle the stages of generation
    ProceduralGeneration procGen;
    bool erosion;

    //private TerrainManager manager;
    private HeightmapController heightmapController;

    private GeneratorMode mode;

    private float[] basePanelHeights;

    // Start is called before the first frame update
    public void Start()
    {
        Debug.Log("start");
        procGen = new ProceduralGeneration(settingsData.defaultTerrainResolution);
        procGen.proceduralGenerationShader = proceduralGenerationShader;
        erosion = false;
        heightmapController = currentTerrain.GetComponent<HeightmapController>();

        gameObject.SetActive(false);
    }

    public void OnEnable()
    {
        TabButtonClick(0);

        if(heightmapController != null)
            heightmapController.SetupChanges();

        if(procGen != null)
            UpdateTerrain();
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
        procGen.iterationRotation = iterationRotationSlider.value;

        procGen.voronoiOffset = new Vector2(voronoiXOffsetSlider.value, voronoiYOffsetSlider.value);
        procGen.cellSize = voronoiCellSizeSlider.value; // * 100;
        procGen.voronoiType = voronoiTypeDropdown.value;

        procGen.factor = factorSlider.value;
        procGen.hillAmplitude = hillAmplitudeSlider.value;

        procGen.mountainType = mountainTypeDropdown.value;
        procGen.mountainOffset = new Vector2(mountainXOffsetSlider.value, mountainYOffsetSlider.value);
        procGen.mountainScale = mountainScaleSlider.value;
        procGen.mountainIterations = (int)mountainIterationsSlider.value;
        procGen.mountainIterationFactor = mountainIterationFactorSlider.value;
        procGen.mountainIterationRotation = mountainIterationRotationSlider.value;
        procGen.mountainAmplitude = mountainAmplitudeSlider.value;
        procGen.mountainVoronoiType = mountainVoronoiTypeDropdown.value;

        procGen.plateausOn = plateauToggle.isOn;
        procGen.plateauOffset = new Vector2(plateauXOffsetSlider.value, plateauYOffsetSlider.value);
        procGen.plateauScale = plateauScaleSlider.value;
        procGen.plateauAmplitude = plateauAmplitudeSlider.value;
        procGen.plateauVoronoiType = plateauVoronoiTypeDropdown.value;
        procGen.plateauHeight = plateauAmplitudeSlider.value;

        procGen.clampEdges = clampToggle.isOn;
        procGen.clampHeight = heightClampSlider.value;
        procGen.minHeight = minimumHeightSlider.value;
        procGen.maxHeight = maximumHeightSlider.value;
        procGen.invert = invertToggle.isOn;
        procGen.heightscale = heightScaleSlider.value;

        procGen.erosionShader = erosionShader;
        if(erodeToggle.isOn) {
            erosion = true;
            procGen.erosionIsOn = true;
            procGen.erosionIterations = (int)erosionIterationsSlider.value * 10000;

            procGen.sedimentCapacityFactor = erosionCapcitySlider.value;
            procGen.erodeSpeed = erosionErosionSpeedSlider.value;
            procGen.depositSpeed = erosionDepositSpeedSlider.value;
            procGen.evaporateSpeed = erosionEvaporationSlider.value;
            procGen.lifetime = (int)erosionLifetimeSlider.value;
            procGen.startSpeed = erosionStartSpeedSlider.value;
            procGen.startWater = erosionStartWaterSlider.value;
            procGen.inertia = erosionInertiaSlider.value;
        } else {
            erosion = false;
            procGen.erosionIsOn = false;
            procGen.erosionIterations = 0;
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

        heightmapController.CreateProceduralTerrain(procGen, erosion);

        heightmapImage.GetComponent<RawImage>().texture = heightmapController.GetHeightmapTexture();
    }

    public void CancelButtonClick()
    {
        heightmapController.RevertChanges();
        internalData.ProcGenOpen = false;
        gameObject.SetActive(false);
    }

    public void ApplyButtonClick()
    {
        Cursor.SetCursor(busyCursor, Vector2.zero, CursorMode.Auto); 

        //force the cursor to update
        Cursor.visible = false;
        Cursor.visible = true;

        heightmapController.ApplyChanges(procGen, erosion);

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); 

        //force the cursor to update
        Cursor.visible = false;
        Cursor.visible = true;

        internalData.ProcGenOpen = false;
        gameObject.SetActive(false);
    }

    public void RedrawButtonClick()
    {
        UpdateTerrain();
    }

    public void MountainTypeChange(int value)
    {
        if(value == 0) {
            mountainPanel1.SetActive(false);
            mountainPanel2.SetActive(false);
            mountainPanel3.SetActive(false);
        } else {
            mountainPanel1.SetActive(true);

            if(value == 1 || value == 2) {
                mountainPanel2.SetActive(true);
                mountainPanel3.SetActive(false);
            } else {
                mountainPanel2.SetActive(false);
                mountainPanel3.SetActive(true);
            }

        }
        UpdateTerrain();
    }
}

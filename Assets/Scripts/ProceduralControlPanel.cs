using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GeneratorMode {perlin, ds_perlin, ds_random, maxVal};
public class ProceduralControlPanel : MonoBehaviour
{
    [Header("UI")]
    public Text modeLabel;
    public GameObject perlinPanel;
    public GameObject randomPanel;

    [Header("Perlin UI")]
    public Slider xOffsetSlider;
    public Slider yOffsetSlider;
    public Slider scaleSlider;

    public Toggle clampToggle;

    public Slider iterationSlider;

    public InputField seedinput;

    [Header("Terrace UI")]
    public Toggle terraceToggle;
    public Toggle layer1Toggle;
    public Slider layer1CountSlider;
    public Slider layer1ShaperSlider;
    public Toggle layer1SmoothToggle;

    public Toggle layer2Toggle;
    public Slider layer2CountSlider;
    public Slider layer2ShaperSlider;
    public Toggle layer2SmoothToggle;

    public Toggle layer3Toggle;
    public Slider layer3CountSlider;
    public Slider layer3ShaperSlider;
    public Toggle layer3SmoothToggle;

    [Header("Erosion UI")]
    public Toggle erodeToggle;

    public Terrain currentTerrain;

    //objects to handle the stages of generation
    ProceduralGeneration procGen;
    TerraceSettings terrace;
    Erosion erosion;

    int resolution;

    private float width;
    private float length;
    private float height;

    private GeneratorMode mode;

    public ComputeShader shader;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("start");
        mode = GeneratorMode.perlin;
        width = length = height = 1000f;
        resolution = currentTerrain.terrainData.heightmapResolution;
        TerrainManager.instance.shader = shader;
        procGen = new ProceduralGeneration(mode, resolution);
        terrace = new TerraceSettings();
        erosion = new Erosion();

        SetPanels();

        gameObject.SetActive(false);
    }

    public void OnEnable()
    {
        if(procGen != null)
            UpdateTerrain();
    }

    public void UpdateTerrain()
    {
        procGen.perlinOffset = new Vector2(xOffsetSlider.value, yOffsetSlider.value);
        procGen.scale = scaleSlider.value;
        procGen.clampEdges = clampToggle.isOn;
        procGen.iterations = (int)iterationSlider.value;

        if(seedinput.text != "")
            procGen.seed = seedinput.text.GetHashCode();

        if(erodeToggle.isOn) {
            erosion.isOn = true;
        } else {
            erosion.isOn = false;
        }

        terrace.ClearLayers();
        if(terraceToggle.isOn) {
            if(layer1Toggle.isOn) {
                terrace.AddLayer(Mathf.FloorToInt(layer1CountSlider.value), layer1ShaperSlider.value, layer1SmoothToggle.isOn);
            }
            if(layer2Toggle.isOn) {
                terrace.AddLayer(Mathf.FloorToInt(layer2CountSlider.value), layer2ShaperSlider.value, layer2SmoothToggle.isOn);
            }
            if(layer3Toggle.isOn) {
                terrace.AddLayer(Mathf.FloorToInt(layer3CountSlider.value), layer3ShaperSlider.value, layer3SmoothToggle.isOn);
            }
        }

        TerrainManager.instance.CreateProceduralTerrain(procGen, terrace, erosion, width, length, height);
    }

    public void ModeButtonClick()
    {
        mode = (GeneratorMode)(((int)mode + 1) % (int)GeneratorMode.maxVal );

        SetPanels();

        UpdateTerrain();
    }

    private void SetPanels()
    {
    }
    public void RedrawButtonClick()
    {
        UpdateTerrain();
    }
}

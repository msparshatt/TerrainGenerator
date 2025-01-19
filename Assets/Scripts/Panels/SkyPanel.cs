using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;

public class SkyPanel : MonoBehaviour, IPanel
{
    [Header("Sun elements")]
    [SerializeField] private Light sun;
    [SerializeField] private Toggle LightToggle;

    [SerializeField] private Slider sunHeightSlider;
    [SerializeField] private Slider sunPositionSlider;
    [SerializeField] private ColorPicker sunColorPicker;
    [SerializeField] private Toggle autoColorToggle;

    [Header("Clouds")]
    [SerializeField] private CloudPresetsDataScriptable[] CloudPresets;
    [SerializeField] private TMP_Dropdown CloudPresetDropdown;
    [SerializeField] private Material SkyMaterial;
    [SerializeField] private GameObject SkyPlane;
    [SerializeField] private Toggle CloudActiveToggle;
    [SerializeField] private Slider CloudXOffsetSlider;
    [SerializeField] private Slider CloudYOffsetSlider;
    [SerializeField] private Slider CloudIterationSlider;
    [SerializeField] private Slider CloudScaleSlider;
    [SerializeField] private Slider CloudStartSlider;
    [SerializeField] private Slider CloudEndSlider;
    [SerializeField] private Slider BrightnessSlider;

    [SerializeField] private Slider WindDirectionSlider;
    [SerializeField] private Slider WindSpeedSlider;

    [SerializeField] private GameObject ocean;

    [SerializeField] private InternalDataScriptable internalData;
    [SerializeField] private InternalDataScriptable defaultData;

    [Header("Skybox")]
    [SerializeField] private Toggle advancedSkyboxToggle;
    [SerializeField] private Slider sunSizeSlider;
    [SerializeField] private Material basicSkyboxMaterial;
    [SerializeField] private Material advancedSkyboxMaterial;
    [SerializeField] private Color skyColor;
    [SerializeField] private Color horizonColor;
    [SerializeField] private Color sunsetColor;
    [SerializeField] private Color nightColor;
    [SerializeField] private Color groundColor;

    private bool autoColor = true;
    private TerrainManager manager;
    private MaterialController materialController;

    private bool presetOptionSelectedFlag = false;
    private bool advancedSkybox;
    private Material currentSkybox;

    // Update is called once per frame
    void Update()
    {
    }

    public void InitialisePanel()
    {
        manager = TerrainManager.Instance();
        materialController = manager.MaterialController;
        sunColorPicker.Awake();
        sunColorPicker.onColorChanged += delegate {ColorPickerChange(); };
        sunColorPicker.color = Color.white;

        CloudPresetDropdown.ClearOptions();

        AddTMP_DropdownOption(CloudPresetDropdown, "Custom");
        for(int index = 0; index < CloudPresets.Length; index++)
        {
            AddTMP_DropdownOption(CloudPresetDropdown, CloudPresets[index].presetName);
        }

        ResetPanel();
        MoveSun();
        InitialiseSkybox();
        UpdateSkyBox();
    }

    private void AddTMP_DropdownOption(TMP_Dropdown dropdown, string label)
    {
        TMP_Dropdown.OptionData newData;
        newData = new TMP_Dropdown.OptionData();
        newData.text = label;

        dropdown.options.Add(newData);
    }

    public void ResetPanel()
    {
        internalData.lightTerrain = defaultData.lightTerrain;
        internalData.sunHeight = defaultData.sunHeight;
        internalData.sunDirection = defaultData.sunDirection;
        internalData.automaticColor = defaultData.automaticColor;
        internalData.sunColor = defaultData.sunColor;
        internalData.cloudActive = defaultData.cloudActive;
        internalData.cloudXoffset = defaultData.cloudXoffset;
        internalData.cloudYOffset = defaultData.cloudYOffset;
        internalData.cloudScale = defaultData.cloudScale;
        internalData.cloudIterations = defaultData.cloudIterations;
        internalData.cloudStart = defaultData.cloudStart;
        internalData.cloudEnd = defaultData.cloudEnd;
        internalData.windSpeed = defaultData.windSpeed;
        internalData.windDirection = defaultData.windDirection;
        internalData.cloudBrightness = defaultData.cloudBrightness;
        internalData.sunSize = defaultData.sunSize;

        advancedSkyboxToggle.isOn = false;
        CloudPresetDropdown.value = 0;

        LoadPanel();
    }

    public void LoadPanel()
    {
        LightToggle.isOn = internalData.lightTerrain;
        sunHeightSlider.value = internalData.sunHeight;
        sunPositionSlider.value = internalData.sunDirection;

        sunColorPicker.color = internalData.sunColor;
        autoColorToggle.isOn = internalData.automaticColor;
        

        CloudActiveToggle.isOn = internalData.cloudActive;
        CloudXOffsetSlider.value = internalData.cloudXoffset;
        CloudYOffsetSlider.value = internalData.cloudYOffset;
        CloudIterationSlider.value = internalData.cloudIterations;
        CloudScaleSlider.value = internalData.cloudScale;
        CloudStartSlider.value = internalData.cloudStart;
        CloudEndSlider.value = internalData.cloudEnd;
        WindDirectionSlider.value = internalData.windDirection;
        WindSpeedSlider.value = internalData.windSpeed;
        BrightnessSlider.value = internalData.cloudBrightness;
        advancedSkyboxToggle.isOn = internalData.advancedSkybox;
        sunSizeSlider.value = internalData.sunSize;
    }

    public void FromJson(string dataString)
    {
        if(dataString != null && dataString != "") {

            SkySaveData_v1 data = JsonUtility.FromJson<SkySaveData_v1>(dataString);

            internalData.lightTerrain = data.lightTerrain;
            internalData.sunHeight = data.sunHeight;
            internalData.sunDirection = data.sunDirection;
            internalData.automaticColor = data.automaticColor;
            internalData.sunColor = data.sunColor;
            internalData.cloudActive = data.cloudActive;
            internalData.cloudXoffset = data.cloudXoffset;
            internalData.cloudYOffset = data.cloudYOffset;
            internalData.cloudScale = data.cloudScale;
            internalData.cloudIterations = data.cloudIterations;
            internalData.cloudStart = data.cloudStart;
            internalData.cloudEnd = data.cloudEnd;
            internalData.windSpeed = data.windSpeed;
            internalData.windDirection = data.windDirection;
            internalData.advancedSkybox = data.advancedSkybox;
            internalData.cloudBrightness = data.cloudBrightness;
            CloudPresetDropdown.value = data.cloudType;
        } else {
            internalData.lightTerrain = defaultData.lightTerrain;
            internalData.sunHeight = defaultData.sunHeight;
            internalData.sunDirection = defaultData.sunDirection;
            internalData.automaticColor = defaultData.automaticColor;
            internalData.sunColor = defaultData.sunColor;
            internalData.cloudActive = defaultData.cloudActive;
            internalData.cloudXoffset = defaultData.cloudXoffset;
            internalData.cloudYOffset = defaultData.cloudYOffset;
            internalData.cloudScale = defaultData.cloudScale;
            internalData.cloudIterations = defaultData.cloudIterations;
            internalData.cloudStart = defaultData.cloudStart;
            internalData.cloudEnd = defaultData.cloudEnd;
            internalData.windSpeed = defaultData.windSpeed;
            internalData.windDirection = defaultData.windDirection;
            internalData.advancedSkybox = defaultData.advancedSkybox;
            internalData.cloudBrightness = defaultData.cloudBrightness;

            CloudPresetDropdown.value = 0;
        }
    }

    public string PanelName()
    {
        return "Sky";
    }

    public Dictionary<string, string> ToDictionary()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();

        data["light_terrain"] = internalData.lightTerrain.ToString();
        data["sun_height"] = internalData.sunHeight.ToString();
        data["sun_direction"] = internalData.sunDirection.ToString();
        data["automatic_color"] = internalData.automaticColor.ToString();
        Color col = internalData.sunColor;
        float[] colorFloat = new float[4]{col.r, col.g, col.b, col.a};        
        data["sun_color"] = JsonConvert.SerializeObject(colorFloat);;
        data["cloud_active"] = internalData.cloudActive.ToString();
        data["cloud_xoffset"] = internalData.cloudXoffset.ToString();
        data["cloud_yoffset"] = internalData.cloudYOffset.ToString();
        data["cloud_scale"] = internalData.cloudScale.ToString();
        data["cloud_iterations"] = internalData.cloudIterations.ToString();
        data["cloud_start"] = internalData.cloudStart.ToString();
        data["cloud_end"] = internalData.cloudEnd.ToString();
        data["wind_speed"] = internalData.windSpeed.ToString();
        data["wind_direction"] = internalData.windDirection.ToString();
        data["advanced_skybox"] = internalData.advancedSkybox.ToString();
        data["sun_size"] = internalData.sunSize.ToString();
        data["cloud_type"] = CloudPresetDropdown.value.ToString();
        data["cloud_brightness"] = internalData.cloudBrightness.ToString();

        return data;
    }

    public void FromDictionary(Dictionary<string, string> data)
    {
        IPanel parent = (IPanel)this;

        internalData.lightTerrain = parent.TryReadValue(data, "light_terrain", defaultData.lightTerrain);
        internalData.sunHeight = parent.TryReadValue(data, "sun_height", defaultData.sunHeight);
        internalData.sunDirection = parent.TryReadValue(data, "sun_direction", defaultData.sunDirection);
        internalData.automaticColor = parent.TryReadValue(data, "automatic_color", defaultData.automaticColor);

        float[] defaultColor = new float[4]{defaultData.sunColor.r, defaultData.sunColor.g, defaultData.sunColor.b, defaultData.sunColor.a};        
        float[] colorFloat = JsonConvert.DeserializeObject<float[]>(parent.TryReadValue(data, "sun_color", JsonConvert.SerializeObject(defaultColor)));
        internalData.sunColor = new Color(colorFloat[0], colorFloat[1], colorFloat[2], colorFloat[3]);

        internalData.cloudActive = parent.TryReadValue(data, "cloud_active", defaultData.cloudActive);
        internalData.cloudXoffset = parent.TryReadValue(data, "cloud_xoffset", defaultData.cloudXoffset);
        internalData.cloudYOffset = parent.TryReadValue(data, "cloud_yoffset", defaultData.cloudYOffset);
        internalData.cloudScale = parent.TryReadValue(data, "cloud_scale", defaultData.cloudScale);
        internalData.cloudIterations = parent.TryReadValue(data, "cloud_iterations", defaultData.cloudIterations);
        internalData.cloudStart = parent.TryReadValue(data, "cloud_start", defaultData.cloudStart);
        internalData.cloudEnd = parent.TryReadValue(data, "cloud_end", defaultData.cloudEnd);
        internalData.windSpeed = parent.TryReadValue(data, "wind_speed", defaultData.windSpeed);
        internalData.windDirection = parent.TryReadValue(data, "wind_direction", defaultData.windDirection);
        internalData.advancedSkybox = parent.TryReadValue(data, "advanced_skybox", false);
        internalData.sunSize = parent.TryReadValue(data, "sun_size", defaultData.sunSize);
        internalData.cloudBrightness = parent.TryReadValue(data, "cloud_brightness", defaultData.cloudBrightness);
        CloudPresetDropdown.value = parent.TryReadValue(data, "cloud_type", 0);

        LoadPanel();
    }

    public void LightToggleChange(bool isOn)
    {
        materialController.ApplyLighting(isOn);
        internalData.lightTerrain = isOn;
    }

    public void LightHeightSliderChange()
    {
        internalData.sunHeight = sunHeightSlider.value;

        if(autoColor) {
            Color skyColor = Color.white;
            if(sunHeightSlider.value <= 15) {
                skyColor  =  Color.Lerp(Color.white, sunsetColor, (15 - sunHeightSlider.value) / 15);
            }
            SetSunColor(skyColor);
        }

        UpdateSkyBox();
        //CloudSliderChange();

        MoveSun();
    }

    public void LightDirectionSliderChange()
    {
        internalData.sunDirection = sunPositionSlider.value;

        MoveSun();
    }
    private void MoveSun()
    {
        sun.transform.localRotation = Quaternion.Euler(internalData.sunHeight, internalData.sunDirection, 0);
    }

    private void InitialiseSkybox()
    {
        advancedSkyboxMaterial.SetColor("_SkyTint", skyColor);
        advancedSkyboxMaterial.SetColor("_HorizonColor", horizonColor);
        advancedSkyboxMaterial.SetColor("_GroundColor", groundColor);
        advancedSkyboxMaterial.SetFloat("_StarBrightness", 0);

        basicSkyboxMaterial.SetColor("_SkyTint", skyColor);
        basicSkyboxMaterial.SetColor("_HorizonColor", horizonColor);
        basicSkyboxMaterial.SetColor("_GroundColor", groundColor);
    }
    private void UpdateSkyBox()
    {
        float height = sunHeightSlider.value;
        if(height > 15) {
            advancedSkyboxMaterial.SetColor("_SkyTint", skyColor);
            advancedSkyboxMaterial.SetColor("_HorizonColor", horizonColor);
            advancedSkyboxMaterial.SetFloat("_StarBrightness", 0);
        } else if(height > 0) {
            float factor = height / 15.0f;
            
            advancedSkyboxMaterial.SetColor("_SkyTint", Color.Lerp(nightColor, skyColor, factor));
            advancedSkyboxMaterial.SetColor("_HorizonColor", Color.Lerp(horizonColor, sun.color, (1 - factor)));
            advancedSkyboxMaterial.SetFloat("_StarBrightness", (1 - factor) / 2);
        } else {
            float factor = (height + 10) / 20.0f;
            advancedSkyboxMaterial.SetColor("_SkyTint", nightColor);
            advancedSkyboxMaterial.SetColor("_HorizonColor", Color.Lerp(nightColor, sun.color, factor));
            advancedSkyboxMaterial.SetFloat("_StarBrightness", 0.5f);
        }
    }
    public void AutoColorToggleChange(bool isOn)
    {
        autoColor = isOn;
        internalData.automaticColor = isOn;
        if(autoColor)
            LightHeightSliderChange();
        else
            ColorPickerChange();
    }

    public void ColorPickerChange()
    {
        if(!autoColor) {
            SetSunColor(sunColorPicker.color);
        }

        internalData.sunColor = sunColorPicker.color;
    }

    private void SetSunColor(Color sunColor)
    {
        sun.color = sunColor;
        UpdateSkyBox();
    }

    public void CloudToggleChange(bool isOn)
    {
        SkyPlane.SetActive(isOn);

        internalData.cloudActive = isOn;
    }

    public void XOffsetSliderChange(float value)
    {
        SkyMaterial.SetFloat("_XOffset", value);
        internalData.cloudXoffset = value;
    }
    public void YOffsetSliderChange(float value)
    {
        SkyMaterial.SetFloat("_YOffset", value);
        internalData.cloudYOffset = value;
    }

    public void ScaleSliderChange(float value)
    {
        SkyMaterial.SetFloat("_Scale", value);
        internalData.cloudScale = value;
    }

    public void IterationsSliderChange(float value)
    {
        SkyMaterial.SetInt("_Iterations", (int)value);
        internalData.cloudIterations = value;
    }

    public void WindDirectionSliderChange(float value)
    {
        internalData.windDirection = value;
   }

    public void WindSpeedSliderChange(float value)
    {
        internalData.windSpeed = value;
    }

    public void CloudStartSliderChange(float value)
    {
        SkyMaterial.SetFloat("_CloudStart", CloudStartSlider.value);
        internalData.cloudStart = CloudStartSlider.value;

        if(!presetOptionSelectedFlag)
            CloudPresetDropdown.value = 0;
    }

    public void CloudEndSliderChange(float value)
    {
        SkyMaterial.SetFloat("_CloudEnd", CloudEndSlider.value);
        internalData.cloudEnd = CloudEndSlider.value;

        if(!presetOptionSelectedFlag)
            CloudPresetDropdown.value = 0;
    }
    
    public void CloudBrightnessSliderChange(float value)
    {
        internalData.cloudBrightness = value;
        Color cloudColor = new Color(value, value, value, 1);

        SkyMaterial.SetColor("_CloudColor", cloudColor);

        if(!presetOptionSelectedFlag)
            CloudPresetDropdown.value = 0;
    }

    public void CloudPresetDropdownChange(int value)
    {
        if(value > 0) {
            CloudPresetsDataScriptable preset = CloudPresets[value - 1];

            presetOptionSelectedFlag = true;
            CloudStartSlider.value = preset.cloudStart;
            CloudEndSlider.value = preset.cloudEnd;
            BrightnessSlider.value = preset.cloudBrightness;
            presetOptionSelectedFlag = false;
        }
    }

    public void AdvancedSkyboxToggleChange(bool isOn)
    {
        if(isOn) {
            internalData.advancedSkybox = true;
            RenderSettings.skybox = advancedSkyboxMaterial;
            currentSkybox = advancedSkyboxMaterial;
        } else {
            internalData.advancedSkybox = false;
            RenderSettings.skybox = basicSkyboxMaterial;
            currentSkybox = basicSkyboxMaterial;
        }
    }

    public void SunSizeSliderChange(float value)
    {
        advancedSkyboxMaterial.SetFloat("_SunSize", value / 100);
        internalData.sunSize = value;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    public string ToJson()
    {
        Debug.Log("SAVE: Sky panel data");

        SkySaveData_v1 data = new SkySaveData_v1();

        data.lightTerrain = internalData.lightTerrain;
        data.sunHeight = internalData.sunHeight;
        data.sunDirection = internalData.sunDirection;
        data.automaticColor = internalData.automaticColor;
        data.sunColor = internalData.sunColor;
        data.cloudActive = internalData.cloudActive;
        data.cloudXoffset = internalData.cloudXoffset;
        data.cloudYOffset = internalData.cloudYOffset;
        data.cloudScale = internalData.cloudScale;
        data.cloudIterations = internalData.cloudIterations;
        data.cloudStart = internalData.cloudStart;
        data.cloudEnd = internalData.cloudEnd;
        data.windSpeed = internalData.windSpeed;
        data.windDirection = internalData.windDirection;
        data.advancedSkybox = internalData.advancedSkybox;
        data.cloudType = CloudPresetDropdown.value;
        data.cloudBrightness = internalData.cloudBrightness;

        return JsonUtility.ToJson(data);
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
            RenderSettings.skybox = advancedSkyboxMaterial;
            currentSkybox = advancedSkyboxMaterial;
        } else {
            RenderSettings.skybox = basicSkyboxMaterial;
            currentSkybox = basicSkyboxMaterial;
        }
    }

    public void SunSizeSliderChange(float value)
    {
        advancedSkyboxMaterial.SetFloat("_SunSize", value / 100);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkyPanel : MonoBehaviour, IPanel
{
    [Header("Sun elements")]
    [SerializeField] private Toggle LightToggle;

    [SerializeField] private Slider sunHeightSlider;
    [SerializeField] private Slider sunPositionSlider;
    [SerializeField] private ColorPicker sunColorPicker;
    [SerializeField] private Toggle autoColorToggle;

    [Header("Clouds")]
    [SerializeField] private Material SkyMaterial;
    [SerializeField] private GameObject SkyPlane;
    [SerializeField] private Toggle CloudActiveToggle;
    [SerializeField] private Slider CloudXOffsetSlider;
    [SerializeField] private Slider CloudYOffsetSlider;
    [SerializeField] private Slider CloudIterationSlider;
    [SerializeField] private Slider CloudScaleSlider;
    [SerializeField] private Slider CloudStartSlider;
    [SerializeField] private Slider CloudEndSlider;

    [SerializeField] private Slider WindDirectionSlider;
    [SerializeField] private Slider WindSpeedSlider;

    [SerializeField] private GameObject ocean;

    [SerializeField] private InternalDataScriptable internalData;
    [SerializeField] private InternalDataScriptable defaultData;


    private bool autoColor = true;
    private TerrainManager manager;

    // Update is called once per frame
    void Update()
    {
        if(WindSpeedSlider.value > 0) {
            float xmovement = WindSpeedSlider.value * Mathf.Sin(WindDirectionSlider.value * Mathf.Deg2Rad) * Time.realtimeSinceStartup / 100;
            float ymovement = WindSpeedSlider.value * Mathf.Cos(WindDirectionSlider.value * Mathf.Deg2Rad) * Time.realtimeSinceStartup / 100;
            SkyMaterial.SetFloat("_XOffset", CloudXOffsetSlider.value + xmovement);
            SkyMaterial.SetFloat("_YOffset", CloudYOffsetSlider.value + ymovement);       

            Ceto.Ocean.Instance.RenderReflection(ocean);
        }        
    }

    public void InitialisePanel()
    {
        manager = TerrainManager.instance;
        sunColorPicker.Awake();
        ResetPanel();
        sunColorPicker.onColorChanged += delegate {ColorPickerChange(); };
        sunColorPicker.color = Color.white;

        SkyMaterial.SetColor("_CloudColor", new Vector4(0.5f, 0.5f, 0.5f, 1));

        LightSliderChange();
    }

    public void ResetPanel()
    {
        LightToggle.isOn = defaultData.lightTerrain;
        sunHeightSlider.value = defaultData.sunHeight;
        sunPositionSlider.value = defaultData.sunDirection;
        sunColorPicker.color = defaultData.sunColor;
        autoColorToggle.isOn = defaultData.automaticColor;

        CloudActiveToggle.isOn = defaultData.cloudActive;
        CloudXOffsetSlider.value = defaultData.cloudXoffset;
        CloudYOffsetSlider.value = defaultData.cloudYOffset;
        CloudIterationSlider.value = defaultData.cloudIterations;
        CloudScaleSlider.value = defaultData.cloudScale;
        CloudStartSlider.value = defaultData.cloudStart;
        CloudEndSlider.value = defaultData.cloudEnd;
        WindDirectionSlider.value = defaultData.windDirection;
        WindSpeedSlider.value = defaultData.windSpeed;
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
    }

    public void LightToggleChange(bool isOn)
    {
        manager.ApplyLighting(isOn);
        internalData.lightTerrain = isOn;
    }

    public void LightSliderChange()
    {
        manager.MoveSun(sunHeightSlider.value, sunPositionSlider.value);

        if(autoColor) {
            Color skyColor = Color.white;
            if(sunHeightSlider.value <= 15) {
                skyColor  =  Color.Lerp(Color.white, Color.red, (15 - sunHeightSlider.value) / 15);
            }
            SetSunColor(skyColor);
        }

        CloudSliderChange();

        internalData.sunHeight = sunHeightSlider.value;
        internalData.sunDirection = sunPositionSlider.value;
    }

    public void AutoColorToggleChange(bool isOn)
    {
        autoColor = isOn;

        if(autoColor)
            LightSliderChange();
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
        manager.SetSunColor(sunColor);

        SkyMaterial.SetColor("_SunColor", sunColor);
    }

    public void CloudToggleChange(bool isOn)
    {
        SkyPlane.SetActive(isOn);

        internalData.cloudActive = isOn;
    }

    public void CloudSliderChange()
    {
        if(WindSpeedSlider.value > 0) {
            float xmovement = WindSpeedSlider.value * Mathf.Sin(WindDirectionSlider.value * Mathf.Deg2Rad) * Time.realtimeSinceStartup;
            float ymovement = WindSpeedSlider.value * Mathf.Cos(WindDirectionSlider.value * Mathf.Deg2Rad) * Time.realtimeSinceStartup;
            SkyMaterial.SetFloat("_XOffset", CloudXOffsetSlider.value + xmovement);
            SkyMaterial.SetFloat("_YOffset", CloudYOffsetSlider.value + ymovement);            
        } else {
            SkyMaterial.SetFloat("_XOffset", CloudXOffsetSlider.value);
            SkyMaterial.SetFloat("_YOffset", CloudYOffsetSlider.value);
        }

        SkyMaterial.SetInt("_Iterations", (int)CloudIterationSlider.value);
        SkyMaterial.SetFloat("_Scale", CloudScaleSlider.value);
        SkyMaterial.SetFloat("_CloudStart", CloudStartSlider.value);
        SkyMaterial.SetFloat("_CloudEnd", CloudEndSlider.value);

        internalData.cloudXoffset = CloudXOffsetSlider.value;
        internalData.cloudYOffset = CloudYOffsetSlider.value;
        internalData.cloudIterations = CloudIterationSlider.value;
        internalData.cloudScale = CloudScaleSlider.value;
        internalData.cloudStart = CloudStartSlider.value;
        internalData.cloudEnd = CloudEndSlider.value;
        internalData.windDirection = WindDirectionSlider.value;
        internalData.windSpeed = WindSpeedSlider.value;
    }
}

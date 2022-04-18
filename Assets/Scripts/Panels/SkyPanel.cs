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

    [Header("Clouds")]
    [SerializeField] private Material SkyMaterial;
    [SerializeField] private GameObject SkyPlane;
    [SerializeField] private Slider CloudXOffsetSlider;
    [SerializeField] private Slider CloudYOffsetSlider;
    [SerializeField] private Slider CloudIterationSlider;
    [SerializeField] private Slider CloudScaleSlider;
    [SerializeField] private Slider CloudStartSlider;
    [SerializeField] private Slider CloudEndSlider;

    [SerializeField] private Slider WindDirectionSlider;
    [SerializeField] private Slider WindSpeedSlider;



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
        }        
    }

    public void InitialisePanel()
    {
        manager = TerrainManager.instance;
        sunColorPicker.Awake();
        sunColorPicker.onColorChanged += delegate {ColorPickerChange(); };
        sunColorPicker.color = Color.white;

        SkyMaterial.SetColor("_CloudColor", new Vector4(0.5f, 0.5f, 0.5f, 1));

        LightSliderChange();
    }

    public void LightToggleChange(bool isOn)
    {
        manager.ApplyLighting(isOn);
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
    }

    private void SetSunColor(Color sunColor)
    {
        manager.SetSunColor(sunColor);

        SkyMaterial.SetColor("_SunColor", sunColor);
    }

    public void CloudToggleChange(bool isOn)
    {
        SkyPlane.SetActive(isOn);
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
    }
}

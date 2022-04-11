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


    private bool autoColor = true;
    private TerrainManager manager;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitialisePanel()
    {
        manager = TerrainManager.instance;
        sunColorPicker.Awake();
        sunColorPicker.onColorChanged += delegate {ColorPickerChange(); };
        sunColorPicker.color = Color.white;

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
            manager.SetSunColor(skyColor);
        }
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
            manager.SetSunColor(sunColorPicker.color);
        }
    }
}

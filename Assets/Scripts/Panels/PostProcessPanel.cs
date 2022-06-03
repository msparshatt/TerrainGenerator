using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessPanel : MonoBehaviour, IPanel
{
    [SerializeField] private PostProcessVolume volume;

    [Header("UI elements")]
    [SerializeField] private Slider chromaticAberationIntensitySlider;
    [SerializeField] private Slider vignetteIntensitySlider;
    [SerializeField] private Slider exposureSlider;
    [SerializeField] private Slider constrastSlider;
    [SerializeField] private Slider saturationSlider;
    [SerializeField] private Slider temperatureSlider;
    [SerializeField] private Slider tintSlider;
    [SerializeField] private Slider hueShiftSlider;
    [SerializeField] private ColorPicker colorPicker;


    private float chromaticAberrationIntensityValue;
    private float vignetteIntensityValue;
    private float exposureValue;
    private float contrastValue;
    private float saturationValue;
    private float temperatureValue;
    private float tintValue;
    private float hueShiftValue;
    private Color vignetteColor;

    public void InitialisePanel()
    {
        colorPicker.Awake();
        colorPicker.onColorChanged += delegate {ColorPickerChange(); };

        ResetPanel();
    }

    public void ResetPanel()
    {
        colorPicker.color = Color.black;

        chromaticAberationIntensitySlider.value = 0;
        vignetteIntensitySlider.value = 0;
        exposureSlider.value = 0;
        constrastSlider.value = 0;
        saturationSlider.value = 0;

        temperatureSlider.value = 0;
        tintSlider.value = 0;
        hueShiftSlider.value = 0;
    }

    public void AddButton(Texture2D texture, int index = 0)
    {   
    }

    //set the panel controls to the correct values after loading a save
    public void LoadPanel()
    {
    }

    public string ToJson()
    {
        PostProcessingSaveData_v1 data = new PostProcessingSaveData_v1();
        data.chromaticAberrationStrengh = chromaticAberrationIntensityValue;
        data.vignetteStrength = vignetteIntensityValue;
        data.vignetteColor = vignetteColor;
        data.contrast = contrastValue;
        data.exposure = exposureValue;
        data.saturation = saturationValue;
        data.temperature = temperatureValue;
        data.tint = tintValue;
        data.hueShift = hueShiftValue;
        return JsonUtility.ToJson(data);
    }

    public void FromJson(string json)
    {
        PostProcessingSaveData_v1 data = JsonUtility.FromJson<PostProcessingSaveData_v1>(json);

        chromaticAberationIntensitySlider.value = data.chromaticAberrationStrengh;
        vignetteIntensitySlider.value = data.vignetteStrength;
        colorPicker.color = data.vignetteColor;
        exposureSlider.value = data.exposure;
        constrastSlider.value = data.contrast;
        saturationSlider.value = data.saturation;
        temperatureSlider.value = data.temperature;
        tintSlider.value = data.tint;
        hueShiftSlider.value = data.hueShift;
    }

    public void ChromaticAberationIntensitySliderChange(float value)
    {
        ChromaticAberration ca = null;
        volume.profile.TryGetSettings(out ca);
        
        chromaticAberrationIntensityValue = value;
        ca.intensity.value = value;
    }

    public void VignetteIntensitySliderChange(float value)
    {
        Vignette vignette = null;
        volume.profile.TryGetSettings(out vignette);

        vignette.intensity.value = value;
        vignetteIntensityValue = value;
    }

    public void ExposureSliderChange(float value)
    {
        ColorGrading color = null;
        volume.profile.TryGetSettings(out color);
        
        exposureValue = value;
        color.postExposure.value = value;
    }

    public void ContrastSliderChange(float value)
    {
        ColorGrading color = null;
        volume.profile.TryGetSettings(out color);

        contrastValue = value;
        color.contrast.value = value;
    }

    public void SaturationSliderChange(float value)
    {
        ColorGrading color = null;
        volume.profile.TryGetSettings(out color);

        saturationValue = value;
        color.saturation.value = value;
    }

    public void TemperatureSliderChange(float value)
    {
        ColorGrading color = null;
        volume.profile.TryGetSettings(out color);

        temperatureValue = value;
        color.temperature.value = value;
    }

    public void TintSliderChange(float value)
    {
        ColorGrading color = null;
        volume.profile.TryGetSettings(out color);

        tintValue = value;
        color.tint.value = value;
    }

    public void HueShiftSliderChange(float value)
    {
        ColorGrading color = null;
        volume.profile.TryGetSettings(out color);

        hueShiftValue = value;
        color.hueShift.value = value;
    }

    public void ColorPickerChange()
    {
        Vignette vignette = null;
        volume.profile.TryGetSettings(out vignette);

        vignetteColor = colorPicker.color;
        vignette.color.value = vignetteColor;
    }
}

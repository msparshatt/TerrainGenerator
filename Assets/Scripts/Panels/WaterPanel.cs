using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaterPanel : MonoBehaviour, IPanel
{
    [SerializeField] private GameObject ocean;
    [SerializeField] private Ceto.AddAutoShoreMask terrainShoreMask;
    [SerializeField] private InternalDataScriptable internalData;
    [SerializeField] private InternalDataScriptable defaultData;

    [Header("UI elements")]
    [SerializeField] private Toggle oceanActiveToggle;
    [SerializeField] private Slider oceanHeightSlider;
    [SerializeField] private Slider waveDirectionSlider;
    [SerializeField] private Slider waveSpeedSlider;
    [SerializeField] private Slider waveHeightSlider;
    [SerializeField] private Slider choppynessSlider;
    [SerializeField] private Slider foamAmountSlider;
    [SerializeField] private Toggle shorelineActiveToggle;
    [SerializeField] private Slider shorelineFoamAmountSlider;

    private bool heightChanged;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitialisePanel()
    {
        ResetPanel();
    }

    public void ResetPanel()
    {
        oceanActiveToggle.isOn = defaultData.oceanActive;
        oceanHeightSlider.value = defaultData.oceanHeight;
        waveDirectionSlider.value = defaultData.waveDirection;
        waveSpeedSlider.value = defaultData.waveSpeed;
        waveHeightSlider.value = defaultData.waveHeight;
        choppynessSlider.value = defaultData.waveChoppyness;
        foamAmountSlider.value = defaultData.foamAmount;
        shorelineActiveToggle.isOn = defaultData.shoreLineActive;
        shorelineFoamAmountSlider.value = defaultData.shorelineFoamAmount;
    }

    public async void LoadPanel()
    {
        oceanActiveToggle.isOn = internalData.oceanActive;
        oceanHeightSlider.value = internalData.oceanHeight;
        waveDirectionSlider.value = internalData.waveDirection;
        waveSpeedSlider.value = internalData.waveSpeed;
        waveHeightSlider.value = internalData.waveHeight;
        choppynessSlider.value = internalData.waveChoppyness;
        foamAmountSlider.value = internalData.foamAmount;
        shorelineActiveToggle.isOn = internalData.shoreLineActive;
        shorelineFoamAmountSlider.value = internalData.shorelineFoamAmount;
    }

    public void OceanToggleChange(bool isOn)
    {
        ocean.SetActive(isOn);
    }

    public void HeightSliderChange(float value)
    {
        ocean.GetComponent<Ceto.Ocean>().level = value;

        heightChanged = true;
    }

    public void HeightSliderPointerUp()
    {
        if(heightChanged)
            terrainShoreMask.CreateShoreMasks();

        heightChanged = false;
    }

    public void WaveDirectionSliderChange(float value)
    {
        ocean.GetComponent<Ceto.Ocean>().windDir = value;
    }

    public void WaveSpeedSliderChange(float value)
    {
        ocean.GetComponent<Ceto.WaveSpectrum>().waveSpeed = value;
    }

    public void WaveHeightSliderChange(float value)
    {
        ocean.GetComponent<Ceto.WaveSpectrum>().windSpeed = value;
    }

    public void ChoppynessSliderChange(float value)
    {
        ocean.GetComponent<Ceto.WaveSpectrum>().choppyness = value;
    }

    public void FoamAmountSliderChange(float value)
    {
        ocean.GetComponent<Ceto.WaveSpectrum>().foamAmount = value;
    }

    public void ShoreFoamToggleChange(bool isOn)
    {
        terrainShoreMask.enabled = isOn;
    }

    public void ShoreFoamSpreadSliderChange(float value)
    {
        terrainShoreMask.foamSpread = value;
    }
}

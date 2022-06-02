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
        ocean.SetActive(true);
        ocean.SetActive(false);
        ResetPanel();
    }

    public void ResetPanel()
    {
        internalData.oceanActive = defaultData.oceanActive;
        internalData.oceanHeight = defaultData.oceanHeight;
        internalData.waveDirection = defaultData.waveDirection;
        internalData.waveSpeed = defaultData.waveSpeed;
        internalData.waveHeight = defaultData.waveHeight;
        internalData.waveChoppyness = defaultData.waveChoppyness;
        internalData.foamAmount = defaultData.foamAmount;
        internalData.shoreLineActive = defaultData.shoreLineActive;
        internalData.shorelineFoamAmount = defaultData.shorelineFoamAmount;

        LoadPanel();
    }

    public void LoadPanel()
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

    public string ToJson()
    {
        WaterSaveData_v1 data = new WaterSaveData_v1();

        Debug.Log("SAVE: Water panel data");
        data.oceanActive = internalData.oceanActive;
        data.oceanHeight = internalData.oceanHeight;
        data.waveDirection = internalData.waveDirection;
        data.waveSpeed = internalData.waveSpeed;
        data.waveHeight = internalData.waveHeight;
        data.waveChoppyness = internalData.waveChoppyness;
        data.foamAmount = internalData.foamAmount;
        data.shoreLineActive = internalData.shoreLineActive;
        data.shorelineFoamAmount = internalData.shorelineFoamAmount;

        return JsonUtility.ToJson(data);
    }

    public void FromJson(string dataString)
    {
        if(dataString != null && dataString != "") {
            WaterSaveData_v1 data = JsonUtility.FromJson<WaterSaveData_v1>(dataString);

            internalData.oceanActive = data.oceanActive;
            internalData.oceanHeight = data.oceanHeight;
            internalData.waveDirection = data.waveDirection;
            internalData.waveSpeed = data.waveSpeed;
            internalData.waveHeight = data.waveHeight;
            internalData.waveChoppyness = data.waveChoppyness;
            internalData.foamAmount = data.foamAmount;
            internalData.shoreLineActive = data.shoreLineActive;
            internalData.shorelineFoamAmount = data.shorelineFoamAmount;
        } else {
            internalData.oceanActive = defaultData.oceanActive;
            internalData.oceanHeight = defaultData.oceanHeight;
            internalData.waveDirection = defaultData.waveDirection;
            internalData.waveSpeed = defaultData.waveSpeed;
            internalData.waveHeight = defaultData.waveHeight;
            internalData.waveChoppyness = defaultData.waveChoppyness;
            internalData.foamAmount = defaultData.foamAmount;
            internalData.shoreLineActive = defaultData.shoreLineActive;
            internalData.shorelineFoamAmount = defaultData.shorelineFoamAmount;
        }
    }
    public void OceanToggleChange(bool isOn)
    {
        internalData.oceanActive = isOn;
        ocean.SetActive(isOn);
    }

    public void HeightSliderChange(float value)
    {
        internalData.oceanHeight = value;
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
        internalData.waveDirection = value;
        ocean.GetComponent<Ceto.Ocean>().windDir = value;
    }

    public void WaveSpeedSliderChange(float value)
    {
        internalData.waveSpeed = value;
        ocean.GetComponent<Ceto.WaveSpectrum>().waveSpeed = value;
    }

    public void WaveHeightSliderChange(float value)
    {
        internalData.waveHeight = value;
        ocean.GetComponent<Ceto.WaveSpectrum>().windSpeed = value;
    }

    public void ChoppynessSliderChange(float value)
    {
        internalData.waveChoppyness = value;
        ocean.GetComponent<Ceto.WaveSpectrum>().choppyness = value;
    }

    public void FoamAmountSliderChange(float value)
    {
        internalData.foamAmount = value;
        ocean.GetComponent<Ceto.WaveSpectrum>().foamAmount = value;
    }

    public void ShoreFoamToggleChange(bool isOn)
    {
        internalData.shoreLineActive = isOn;
        terrainShoreMask.enabled = isOn;
    }

    public void ShoreFoamSpreadSliderChange(float value)
    {
        internalData.shorelineFoamAmount = value;
        terrainShoreMask.foamSpread = value;
    }
}

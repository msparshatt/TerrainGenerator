using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPanel : MonoBehaviour, IPanel
{
    [SerializeField] private GameObject ocean;
    [SerializeField] private Ceto.AddAutoShoreMask terrainShoreMask;

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

    }

    public void ResetPanel()
    {

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

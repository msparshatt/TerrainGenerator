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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaterialsPanel : MonoBehaviour
{
    [Header("UI elements")]
    [SerializeField] private Slider scaleSlider;

    [Header("Data objects")]
    [SerializeField] private SettingsDataScriptable settingsData;
    [SerializeField] private InternalDataScriptable internalData;

    private TerrainManager manager;
    // Start is called before the first frame update
    void Start()
    {
        manager = TerrainManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //settings panel
    public void AOToggleChange(bool isOn)
    {
        manager.SetAO(isOn);
    }

    public void ScaleSliderChange(float value)
    {
        internalData.sliderChanged = true;
        manager.ScaleMaterial(value);
    }

    public void ResetTilingButtonClick()
    {
        scaleSlider.value = 1.0f;
        //Camera.main.GetComponent<CameraController>().sliderChanged = true;

        manager.ApplyTextures();
    }

}

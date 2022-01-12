using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private Slider movementSlider;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private SettingsDataScriptable dataScriptable;
    [SerializeField] private Dropdown ResolutionDropdown;
    [SerializeField] private GameObject aboutPanel;

    private float defaultSpeed = 40.0f;
    private float defaultSensitivity = 1.0f;
    public void MovementSliderChange(float value)
    {
        dataScriptable.movementSpeed = value;
    }

    public void SensitivitySliderChange(float value)
    {
        dataScriptable.cameraSensitivity = value;
    }

    public void ResetButtonClick()
    {
        movementSlider.value = defaultSpeed;
        sensitivitySlider.value = defaultSensitivity;
        ResolutionDropdown.value = 0;
    }

    public void ReloadButtonClick()
    {
        movementSlider.value = PlayerPrefs.GetFloat("movementSpeed", defaultSpeed);
        sensitivitySlider.value = PlayerPrefs.GetFloat("cameraSensitivity", defaultSensitivity);
        dataScriptable.defaultTerrainResolution = PlayerPrefs.GetInt("DefaultTerrainResolution");

        if(dataScriptable.defaultTerrainResolution == 0)
            dataScriptable.defaultTerrainResolution = 1025;

        ResolutionDropdown.value = PlayerPrefs.GetInt("ScreenshotResolution");
        //ResolutionDropDownChange(PlayerPrefs.GetInt("ScreenshotResolution"));
    }

    public void ResolutionDropDownChange(int index)
    {
        dataScriptable.resolutionMultiplier = (int)Mathf.Pow(2, index);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("movementSpeed", movementSlider.value);
        PlayerPrefs.SetFloat("cameraSensitivity", sensitivitySlider.value);
        PlayerPrefs.SetInt("DefaultTerrainResolution", dataScriptable.defaultTerrainResolution);
        PlayerPrefs.SetInt("ScreenshotResolution", ResolutionDropdown.value);
    }

    public void AboutButtonClick()
    {
        aboutPanel.SetActive(true);
    }

    public void CloseAboutPanelClick()
    {
        aboutPanel.SetActive(false);
    }

    public void Start()
    {
        ReloadButtonClick();
    }
}

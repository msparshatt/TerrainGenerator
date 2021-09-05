using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private Slider movementSlider;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private SettingsDataScriptable dataScriptable;

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
    }

    public void ReloadButtonClick()
    {
        movementSlider.value = PlayerPrefs.GetFloat("movementSpeed", defaultSpeed);
        sensitivitySlider.value = PlayerPrefs.GetFloat("cameraSensitivity", defaultSensitivity);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("movementSpeed", movementSlider.value);
        PlayerPrefs.SetFloat("cameraSensitivity", sensitivitySlider.value);
    }
    public void Start()
    {
        ReloadButtonClick();
    }
}

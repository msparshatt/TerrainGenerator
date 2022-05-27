using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private Slider movementSlider;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private SettingsDataScriptable dataScriptable;
    [SerializeField] private Dropdown ResolutionDropdown;
    [SerializeField] private TMP_InputField undoCountInputField;
    [SerializeField] private GameObject aboutPanel;

    [SerializeField] private PlayerInput playerInput;

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
        dataScriptable.defaultTerrainResolution = 513; //PlayerPrefs.GetInt("DefaultTerrainResolution");

        if(dataScriptable.defaultTerrainResolution == 0)
            dataScriptable.defaultTerrainResolution = 513;

        ResolutionDropdown.value = PlayerPrefs.GetInt("ScreenshotResolution");

        int undoCount = PlayerPrefs.GetInt("UndoCount");
        if(undoCount == 0)
            undoCount = 50;

        undoCountInputField.text = undoCount.ToString();
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
        PlayerPrefs.SetInt("UndoCount", dataScriptable.undoCount);
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

    public void QWERTToggle(bool isOn)
    {
        if(isOn)
            playerInput.SwitchCurrentActionMap("QWERTY");
        else
            playerInput.SwitchCurrentActionMap("AZERTY");
    }

    public void UndoBufferCountChange(string value)
    {
        int number = 50;
        undoCountInputField.GetComponent<Image>().color = Color.white;

        try {
            number = int.Parse(value);
        } catch {
            undoCountInputField.GetComponent<Image>().color = new Color(1, 0.2f, 0.2f, 1);
        }

        dataScriptable.undoCount = number;
    }
}

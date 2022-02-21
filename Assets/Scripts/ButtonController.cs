using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    [SerializeField] private RawImage[] imageButtons;
    [SerializeField] private SettingsDataScriptable settingsData;

    public void DeselectButtons()
    {
        Debug.Log("Deselecting");   
        for(int index = 0; index < imageButtons.Length; index++) {
            imageButtons[index].color = settingsData.deselectedColor;
        }
    }
}

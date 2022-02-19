using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainBar : MonoBehaviour
{
    [SerializeField] private Button[] buttons;
    [SerializeField] private GameObject[] panels;
    [SerializeField] private InternalDataScriptable internalData;

    private Color unselectedColor = Color.white;
    private Color selectedColor = Color.grey;

    public void Start()
    {
        HideAllPanels();
        ShowPanel(0);
        internalData.mode = (InternalDataScriptable.Modes)0;
    }

    public void ButtonClick(int index)
    {
        HideAllPanels();
        ShowPanel(index);
        internalData.mode = (InternalDataScriptable.Modes)index;
    }

    private void ShowPanel(int index)
    {
        panels[index].SetActive(true);
        buttons[index].GetComponent<Image>().color = selectedColor;
    }

    private void HideAllPanels()
    {
        for(int i = 0; i < panels.Length; i++) {
            panels[i].SetActive(false);
            buttons[i].GetComponent<Image>().color = unselectedColor;
        }
    }
}

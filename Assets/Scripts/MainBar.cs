using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainBar : MonoBehaviour
{
    [SerializeField] private Button[] buttons;
    [SerializeField] private GameObject[] panels;
    [SerializeField] private GameObject sidePanels;
    [SerializeField] private InternalDataScriptable internalData;

    private Color unselectedColor = Color.white;
    private Color selectedColor = Color.grey;

    private int selectedPanel;
    public void Start()
    {
        HideAllPanels();
        ShowPanel(0);
        internalData.mode = (InternalDataScriptable.Modes)0;
    }

    public void ButtonClick(int index)
    {
        if(index != selectedPanel) {
            HideAllPanels();
            ShowPanel(index);
            internalData.mode = (InternalDataScriptable.Modes)index;

            sidePanels.GetComponent<PanelController>().CloseAllPanels();
            selectedPanel = index;
        }
    }

    private void ShowPanel(int index)
    {
        panels[index].SetActive(true);
        buttons[index].GetComponent<Image>().color = selectedColor;
    }

    private void HideAllPanels()
    {
        for(int i = 0; i < panels.Length; i++) {
            ButtonController panelButtons = panels[i].GetComponent<ButtonController>();

            if(panelButtons != null) {
                panelButtons.DeselectButtons();
            }

            panels[i].SetActive(false);
            buttons[i].GetComponent<Image>().color = unselectedColor;
        }
    }
}

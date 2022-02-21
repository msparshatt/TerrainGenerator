using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelController : MonoBehaviour
{
    [SerializeField] private GameObject[] panels;
    public void CloseAllPanels()
    {
        for(int i = 0; i < panels.Length; i++) {
            panels[i].SetActive(false);

            ButtonController buttons = panels[i].GetComponent<ButtonController>();

            if(buttons != null) {
                buttons.DeselectButtons();
            }
        }
    }

    public void ShowPanel(int index)
    {
        if(index < panels.Length) {
            panels[index].SetActive(true);
        }
    }
}

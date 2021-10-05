using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpPanel : MonoBehaviour
{
    [SerializeField] private GameObject[] panels;
    [SerializeField] private Button[] buttons;
    [SerializeField] private Sprite selectedTab;
    [SerializeField] private Sprite unselectedTab;


    // Start is called before the first frame update
    void Start()
    {
        ButtonClick(0);
    }

    public void ButtonClick(int index)
    {
        HideAllText();
        panels[index].SetActive(true);
        buttons[index].GetComponent<Image>().sprite = selectedTab;
    }
    private void HideAllText()
    {
        for(int i = 0; i < panels.Length; i++) {
            panels[i].SetActive(false);
            buttons[i].GetComponent<Image>().sprite = unselectedTab;
        }
    }
}

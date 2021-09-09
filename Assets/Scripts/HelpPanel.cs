using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpPanel : MonoBehaviour
{
    [SerializeField] private GameObject generalText;
    [SerializeField] private GameObject controlText;
    [SerializeField] private GameObject interactionText;
    [SerializeField] private GameObject systemText;

    // Start is called before the first frame update
    void Start()
    {
        HideAllText();
        generalText.SetActive(true);
    }

    public void ButtonClick(int index)
    {
        HideAllText();
        switch(index)
        {
            case 0:
                generalText.SetActive(true);
                break;
            case 1:
                controlText.SetActive(true);
                break;
            case 2:
                interactionText.SetActive(true);
                break;
            case 3:
                systemText.SetActive(true);
                break;
        }
    }
    private void HideAllText()
    {
        generalText.SetActive(false);
        controlText.SetActive(false);
        interactionText.SetActive(false);
        systemText.SetActive(false);
    }
}

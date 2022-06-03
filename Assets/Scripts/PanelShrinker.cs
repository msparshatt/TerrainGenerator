using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PanelShrinker : MonoBehaviour
{
    [SerializeField] private float shrunkHeight;
    [SerializeField] private TMP_Text shrinkButtonText;
    [SerializeField] private GameObject[] elementList;

    private float baseHeight;
    private float baseWidth;
    // Start is called before the first frame update
    void Start()
    {
        RectTransform rt = gameObject.transform.GetComponent<RectTransform>();
        baseWidth =  rt.sizeDelta.x * rt.localScale.x;
        baseHeight = rt.sizeDelta.y * rt.localScale.y;
    }

    public void ShrinkPanel()
    {
        RectTransform rt = gameObject.transform.GetComponent<RectTransform>();        
        float currentHeight = rt.sizeDelta.y * rt.localScale.y;

        if(currentHeight < baseHeight) {
            //grow
            rt.sizeDelta = new Vector2(baseWidth, baseHeight);

            for(int index = 0; index < elementList.Length; index++) {
                elementList[index].SetActive(true);
            }

            shrinkButtonText.text = "V";
        } else {
            //shrink
            rt.sizeDelta = new Vector2(baseWidth, shrunkHeight);

            for(int index = 0; index < elementList.Length; index++) {
                elementList[index].SetActive(false);
            }

            shrinkButtonText.text = ">";
        }
    }
}

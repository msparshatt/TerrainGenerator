using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShowValueasTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private string text;

    public void OnPointerEnter(PointerEventData eventData)
    {
        float posX = Input.mousePosition.x + 50;
        float posY = Input.mousePosition.y - 50;
        tooltipPanel.transform.position = new Vector2(posX, posY);

        Text tooltip = tooltipPanel.transform.GetChild(0).gameObject.GetComponent<Text>();
        tooltip.text = text + ": " + gameObject.GetComponent<Slider>().value.ToString();
        tooltipPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipPanel.SetActive(false);
    }
}

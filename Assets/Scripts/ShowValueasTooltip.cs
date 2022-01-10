using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class ShowValueasTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private string text;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector2 pos = Mouse.current.position.ReadValue();
        tooltipPanel.transform.position = new Vector2(pos.x + 50 , pos.y - 50);

        TextMeshProUGUI tooltip = tooltipPanel.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        tooltip.text = text + ": " + gameObject.GetComponent<Slider>().value.ToString();
        tooltipPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipPanel.SetActive(false);
    }
}

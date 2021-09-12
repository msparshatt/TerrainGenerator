using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ShowTextAsTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private string text;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector2 pos = Mouse.current.position.ReadValue();
        tooltipPanel.transform.position = new Vector2(pos.x + 50 , pos.y - 50);

        Text tooltip = tooltipPanel.transform.GetChild(0).gameObject.GetComponent<Text>();
        tooltip.text = text;
        tooltipPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipPanel.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SimpleFileBrowser;



public class ErosionPanel : MonoBehaviour, IPanel
{
    [SerializeField] private TerrainPanel terrainPanel;

    public void FromJson(string dataString)
    {
        terrainPanel.LoadErodeSettings(dataString);
    }
}

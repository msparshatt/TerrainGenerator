using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SimpleFileBrowser;

public class StampPanel : MonoBehaviour, IPanel
{
    [SerializeField] private TerrainPanel terrainPanel;

    public void FromJson(string json) 
    {
        terrainPanel.LoadStampSettings(json);
    }
}

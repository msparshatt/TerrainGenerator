using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPanel
{
    void InitialisePanel()
    {
    }
    void ResetPanel()
    {
    }

    void AddButton(Texture2D texture, int index = 0)
    {   
    }

    void AddTerrainButton(Texture2D texture, InternalDataScriptable.TerrainModes mode = InternalDataScriptable.TerrainModes.Sculpt)
    {}

    //set the panel controls to the correct values after loading a save
    void LoadPanel()
    {
    }

    void FromJson(string data)
    {

    }

    string PanelName()
    {
        return "Default";
    }

    Dictionary<string, string> ToDictionary()
    {
        return new Dictionary<string, string>();
    }

    void FromDictionary(Dictionary<string, string> data)
    {

    }

    public string TryReadValue(Dictionary<string, string> dictionary, string key, string defaultValue)
    {
        if(dictionary.ContainsKey(key))
            return dictionary[key];
        else
            return defaultValue;
    }

    public bool TryReadValue(Dictionary<string, string> dictionary, string key, bool defaultValue)
    {
        if(dictionary.ContainsKey(key))
            return bool.Parse(dictionary[key]);
        else
            return defaultValue;
    }

    public float TryReadValue(Dictionary<string, string> dictionary, string key, float defaultValue)
    {
        if(dictionary.ContainsKey(key))
            return float.Parse(dictionary[key]);
        else
            return defaultValue;
    }

    public int TryReadValue(Dictionary<string, string> dictionary, string key, int defaultValue)
    {
        if(dictionary.ContainsKey(key))
            return int.Parse(dictionary[key]);
        else
            return defaultValue;
    }
}

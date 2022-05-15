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

    //set the panel controls to the correct values after loading a save
    void LoadPanel()
    {
    }

    string ToJson()
    {
        return "";
    }

    void FromJson(string data)
    {

    }
}

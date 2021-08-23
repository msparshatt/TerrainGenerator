using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameLoader : MonoBehaviour
{
    AsyncOperation loadingOperation;
    [SerializeField] private Slider progressBar;

    void Start() 
    {
        //accessing the instance property creates an instance of the class which loads all game resources
        GameResources gameResources = GameResources.instance; 
        int count = 0;
        while(gameResources.icons == null && count < 1000)
            count++;
        loadingOperation = SceneManager.LoadSceneAsync("MainScene");
    }

    // Update is called once per frame
    void Update()
    {
        progressBar.value = Mathf.Clamp01(loadingOperation.progress / 0.9f);
    }
}

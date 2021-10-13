using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameLoader : MonoBehaviour
{
    AsyncOperation loadingOperation;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image loadingScreen;
    [SerializeField] private Sprite[] loadingScreenImages;

    void Start() 
    {
        int index = Random.Range(0, loadingScreenImages.Length);
        loadingScreen.GetComponent<Image>().sprite = loadingScreenImages[index];
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
        if(progressBar != null)
            progressBar.value = Mathf.Clamp01(loadingOperation.progress / 0.9f);
    }
}

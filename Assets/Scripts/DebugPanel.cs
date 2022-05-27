using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Profiling;

public class DebugPanel : MonoBehaviour
{
    [SerializeField] private InternalDataScriptable internalData;
    [SerializeField] private TMP_Text totalMemory;
    [SerializeField] private TMP_Text gcMemory;
    [SerializeField] private TMP_Text textureMemory;
    [SerializeField] private TMP_Text meshMemory;

    private bool profiling;

    ProfilerRecorder _totalReservedMemoryRecorder;
    ProfilerRecorder _gcReservedMemoryRecorder;
    ProfilerRecorder _textureMemoryRecorder;
    ProfilerRecorder _meshMemoryRecorder;    

    // Start is called before the first frame update
    void Start()
    {
        profiling = false;   
    }

    // Update is called once per frame
    void Update()
    {
        if(profiling) {
            totalMemory.text = "Total Memory: " + ( _totalReservedMemoryRecorder.LastValue / (1024 * 1024)) + " MB";
            gcMemory.text = "GC Memory: " + (_gcReservedMemoryRecorder.LastValue / (1024 * 1024)) + " MB";
            textureMemory.text = "Texture Memory: " + (_textureMemoryRecorder.LastValue / (1024 * 1024)) + " MB";
            meshMemory.text = "Mesh Memory: " + (_meshMemoryRecorder.LastValue / (1024 * 1024)) + " MB";
        }
    }

    public void DeleteAllButtonClick()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    public void DeleteImportsButtonClick()
    {
        int count = PlayerPrefs.GetInt("CustomBrushCount");

        if(count > 0) {
            for(int i = 0; i < count; i++) {
                PlayerPrefs.DeleteKey("CustomBrush_" + i);
            }
        }
        PlayerPrefs.SetInt("CustomBrushCount", 0);
        internalData.customSculptBrushes.Clear();

        count = PlayerPrefs.GetInt("CustomPaintBrushCount");

        if(count > 0) {
            for(int i = 0; i < count; i++) {
                PlayerPrefs.DeleteKey("CustomPaintBrush_" + i);
            }
        }
        PlayerPrefs.SetInt("CustomPaintBrushCount", 0);
        internalData.customPaintBrushes.Clear();

        count = PlayerPrefs.GetInt("CustomErosionBrushCount");

        if(count > 0) {
            for(int i = 0; i < count; i++) {
                PlayerPrefs.DeleteKey("CustomErosionBrush_" + i);
            }
        }
        PlayerPrefs.SetInt("CustomErosionBrushCount", 0);
        internalData.customErosionBrushes.Clear();

        count = PlayerPrefs.GetInt("CustomStampCount");

        if(count > 0) {
            for(int i = 0; i < count; i++) {
                PlayerPrefs.DeleteKey("CustomStamp_" + i);
            }
        }
        PlayerPrefs.SetInt("CustomStampCount", 0);
        internalData.customStampBrushes.Clear();

        count = PlayerPrefs.GetInt("CustomtextureCount");

        if(count > 0) {
            for(int i = 0; i < count; i++) {
                PlayerPrefs.DeleteKey("Customtexture_" + i);
            }
        }
        PlayerPrefs.SetInt("CustomTextureCount", 0);
        internalData.customTextures.Clear();

        count = PlayerPrefs.GetInt("CustomMaterialCount");

        if(count > 0) {
            for(int i = 0; i < count; i++) {
                PlayerPrefs.DeleteKey("CustomMaterialBrush_" + i);
            }
        }
        PlayerPrefs.SetInt("CustomMaterialCount", 0);
        internalData.customMaterials.Clear();

        PlayerPrefs.Save();
    }

    public void StartButtonClick()
    {
        if(!profiling) {
            _totalReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
            _gcReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
            _textureMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Memory");
            _meshMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Mesh Memory");

            profiling = true;
        }
    }

    public void StopButtonClick()
    {
        if(profiling) {
            _totalReservedMemoryRecorder.Dispose();
            _gcReservedMemoryRecorder.Dispose();
            _textureMemoryRecorder.Dispose();
            _meshMemoryRecorder.Dispose();
    
            profiling = false;
        }
    }
}

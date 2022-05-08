using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPanel : MonoBehaviour
{
    [SerializeField] private InternalDataScriptable internalData;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
}

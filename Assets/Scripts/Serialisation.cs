using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class Serialisation : MonoBehaviour
{
    [SerializeField] private Texture2D busyCursor;
    [SerializeField] private InternalDataScriptable internalData;
    private string savefileName;
    private TerrainManager manager;
    private GameResources gameResources;
    // Start is called before the first frame update
    void Start()
    {
        manager = TerrainManager.instance;
        gameResources = GameResources.instance;
    }
    public void Load(string filename)
    {
        if(filename != "") {
            savefileName = filename;

            //change the cursor then force it to update
            Cursor.SetCursor(busyCursor, Vector2.zero, CursorMode.Auto);            
            Cursor.visible = false;
            Cursor.visible = true;

            //create a stream reader and read the file contents
            var sr = new StreamReader(filename);
            string fileContents = sr.ReadToEnd();
            sr.Close();        

            //read the file version
            Version data = JsonUtility.FromJson<Version>(fileContents);

            if(data.version <= 1) {
                Version1Load(fileContents);
            } else {
                Version2Load(fileContents);
            }

            //reset the cursor
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }        
    }

    public void Version1Load(string fileContents)
    {
        SaveData_v1 data = JsonUtility.FromJson<SaveData_v1>(fileContents);
        manager.CreateTerrainFromHeightmap(data.heightmap);

        int materialPanelIndex = 0;
        int no_textures = 5;
        
        //fix
        if(data.baseTexture == -1) {
            //SelectMaterialIcon(0, AddBaseTexture(data.baseTexture_colors));
        } else {
            //SelectMaterialIcon(0, RemapTextureIndex(data.baseTexture));
        }

        if(data.baseTexture2 == -1) {
            //SelectMaterialIcon(1, AddBaseTexture(data.baseTexture2_colors));
        } else {
            //SelectMaterialIcon(1, RemapTextureIndex(data.baseTexture2));
        }

        manager.mixTypes[1] = data.mixType;
/*        if(data.mixType == 2)
            slopeToggles[1].isOn = true;
        else
            heightToggles[1].isOn = true;*/

        manager.mixFactors[1] = 1- data.mixFactor;
        //mixFactorSliders[1].value = 1 - data.mixFactor;

        for(int index = 2; index < 5; index++) {
            //SelectMaterialIcon(index, index);
            manager.mixTypes[index] = 1;
            //heightToggles[index].isOn = true;

            manager.mixFactors[index] = 0f;
            //mixFactorSliders[index].value = 0f;
        }

        if(data.tiling == 0)
            data.tiling = 1;

        //scaleSlider.value = data.tiling;

        if(data.paintTiling == 0)
            data.paintTiling = 1;
        //paintScaleSlider.value = data.paintTiling;

        //aoToggle.isOn = data.aoActive;
        
        Texture2D texture = new Texture2D(10,10);
        ImageConversion.LoadImage(texture, data.overlayTexture);

        manager.SetOverlay(texture);

        //OldSavePanel.SetActive(true);
    }

    public void Version2Load(string fileContents)
    {
        SaveData_v2 data = JsonUtility.FromJson<SaveData_v2>(fileContents);
        manager.CreateTerrainFromHeightmap(data.heightmap);

        int materialPanelIndex = 0;
        int no_textures = 5;
        
        for(int index = 0; index < no_textures; index++) {
            if(data.baseTexture[index] == -1) {
            //    SelectMaterialIcon(index, AddBaseTexture(data.baseTexture_colors[index]));
            } else {
            //    SelectMaterialIcon(index, data.baseTexture[index]);
            }

            if(index > 0)
            {
                manager.mixTypes[index] = data.mixType[index];
/*                if(data.mixType[index] == 1)
                    heightToggles[index].isOn = true;
                else
                    slopeToggles[index].isOn = true;*/

                manager.mixFactors[index] = data.mixFactor[index];
                //mixFactorSliders[index].value = data.mixFactor[index];
            } else {
                manager.mixTypes[0] = 0;
                manager.mixFactors[0] = 0f;
            }
        }

        if(data.tiling == 0)
            data.tiling = 1;

        //scaleSlider.value = data.tiling;

        if(data.paintTiling == 0)
            data.paintTiling = 1;
        //paintScaleSlider.value = data.paintTiling;

        //aoToggle.isOn = data.aoActive;
        
        Texture2D texture = new Texture2D(10,10);
        ImageConversion.LoadImage(texture, data.overlayTexture);

        manager.SetOverlay(texture);
    }

    public void UpdateOldSaveFile()
    {
        //backup save file
        File.Copy(savefileName, savefileName + ".bak");

        //save new version
        Save(savefileName, false);

        //OldSavePanel.SetActive(false);
    }

    public void DontUpdateOldSaveFile()
    {
        //OldSavePanel.SetActive(false);
    }

    public void Save(string filename, bool exitOnSave)
    {
        /*if(filename != null && filename != "") {
            savefileName = filename;
            Debug.Log("Saving to " + filename);
            Cursor.SetCursor(busyCursor, Vector2.zero, CursorMode.Auto); 

            //force the cursor to update
            Cursor.visible = false;
            Cursor.visible = true;

            Debug.Log("SAVE: Creating SaveData object");
            SaveData_v2 data = new SaveData_v2();
            Texture2D texture;

            data.version = 2;
            Debug.Log("SAVE: Store heightmap");
            data.heightmap = manager.GetHeightmapAsBytes();
            Debug.Log("SAVE: Store base textures");

            int no_textures = 5;
            data.baseTexture = new int[no_textures];
            data.baseTexture_colors = new byte[no_textures][];
            data.mixFactor = new float[no_textures];
            data.mixType = new int[no_textures];
            
            for(int index = 0; index < no_textures; index++) {
                if(currentMaterialIndices[index] >= (gameResources.materials.Count - customMaterials.Count)) {
                    data.baseTexture[index] = -1;
                    texture = (Texture2D)gameResources.materials[currentMaterialIndices[index]].mainTexture;
                    data.baseTexture_colors[index] = texture.EncodeToPNG();
                } else {
                    data.baseTexture[index] = currentMaterialIndices[index];
                    data.baseTexture_colors[index] = null;
                }

                data.mixType[index] = manager.mixTypes[index];
                data.mixFactor[index] = manager.mixFactors[index];
            }

            data.tiling = scaleSlider.value;
            data.aoActive = aoToggle.isOn;

            Debug.Log("SAVE: Store overlay texture");
            texture = manager.GetOverlay();
            data.overlayTexture = texture.EncodeToPNG();
            data.paintTiling = paintScaleSlider.value;

            Debug.Log("SAVE: Create json string");
            string json = JsonUtility.ToJson(data);

            Debug.Log("SAVE: Write to file");
            var sr = File.CreateText(filename);
            sr.WriteLine (json);
            sr.Close();
            Debug.Log("SAVE: Finish");

            if(exitOnSave)
                gameObject.GetComponent<Controller>().DoExit();
        }
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        internalData.unsavedChanges = false;*/
    }

}

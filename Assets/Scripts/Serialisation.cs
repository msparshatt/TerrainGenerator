using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class Serialisation : MonoBehaviour
{
    [SerializeField] private Texture2D busyCursor;
    [SerializeField] private InternalDataScriptable internalData;
    [SerializeField] private InternalDataScriptable defaultData;
    [SerializeField] private GameObject oldSavePanel;

    [SerializeField] private GameObject materialsPanel;
    [SerializeField] private GameObject sculptPanel;
    [SerializeField] private GameObject paintPanel;

    [SerializeField] private GameObject waterPanel;
    [SerializeField] private GameObject skyPanel;

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
                bool version4 = fileContents.Contains("baseTexture2");

                Version1Load(fileContents, version4);
            } else {
                Version2Load(fileContents);
            }

            //reset the cursor
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }        
    }

    public void Version1Load(string fileContents, bool version4)
    {
        SaveData_v1 data = JsonUtility.FromJson<SaveData_v1>(fileContents);
        manager.CreateTerrainFromHeightmap(data.heightmap);

        int materialPanelIndex = 0;

        MaterialsPanel materials = materialsPanel.GetComponent<MaterialsPanel>();   

        if(version4) {
            if(data.baseTexture == -1) {
                materials.SelectMaterialIcon(0, materials.AddBaseTexture(data.baseTexture_colors));
            } else {
                materials.SelectMaterialIcon(0, RemapTextureIndex(data.baseTexture, true));
            }

            if(data.baseTexture2 == -1) {
                materials.SelectMaterialIcon(1, materials.AddBaseTexture(data.baseTexture2_colors));
            } else {
                materials.SelectMaterialIcon(1, RemapTextureIndex(data.baseTexture2, true));
            }
        } else {
            if(data.baseTexture == -1) {
                materials.SelectMaterialIcon(0, materials.AddBaseTexture(data.baseTexture_colors));
            } else {
                materials.SelectMaterialIcon(0, RemapTextureIndex(data.baseTexture, false));
            }

            materials.SelectMaterialIcon(1, 1);
        }

        internalData.mixTypes[1] = data.mixType;

        if(data.mixFactor > 0)
            data.mixFactor = 1 - data.mixFactor;

        internalData.mixFactors[1] = data.mixFactor;

        for(int index = 2; index < InternalDataScriptable.NUMBER_MATERIALS; index++) {
            internalData.mixTypes[index] = 1;

            internalData.mixFactors[index] = 0f;
        }
        if(data.tiling == 0)
            data.tiling = 1;

        internalData.materialScale = data.tiling;
        internalData.ambientOcclusion = data.aoActive;

        manager.doNotApply = true;
        materials.LoadPanel();
        manager.doNotApply = false;
        manager.ApplyTextures();


        if(data.paintTiling == 0)
            data.paintTiling = 1;

        internalData.paintScale = data.paintTiling;
        paintPanel.GetComponent<PaintPanel>().LoadPanel();

        Texture2D texture = new Texture2D(10,10);
        ImageConversion.LoadImage(texture, data.overlayTexture);

        manager.SetOverlay(texture);

        oldSavePanel.SetActive(true);
    }

    public void Version2Load(string fileContents)
    {
        SaveData_v2 data = JsonUtility.FromJson<SaveData_v2>(fileContents);
        manager.CreateTerrainFromHeightmap(data.heightmap);

        int materialPanelIndex = 0;

        manager.doNotApply = true;
        MaterialsPanel materials = materialsPanel.GetComponent<MaterialsPanel>();   

        int[] mixTypes = new int[InternalDataScriptable.NUMBER_MATERIALS];
        float[] mixFactors = new float[InternalDataScriptable.NUMBER_MATERIALS];
        for(int index = 0; index < InternalDataScriptable.NUMBER_MATERIALS; index++) {
            if(data.baseTexture[index] == -1) {
                materials.SelectMaterialIcon(index, materials.AddBaseTexture(data.baseTexture_colors[index]));
            } else {
                materials.SelectMaterialIcon(index, data.baseTexture[index]);
            }

            if(index > 0)
            {
                internalData.mixTypes[index] = data.mixType[index];
                internalData.mixFactors[index] = data.mixFactor[index];
            } else {
                internalData.mixTypes[0] = 0;
                internalData.mixFactors[0] = 0f;
            }
        }

        if(data.tiling == 0)
            data.tiling = 1;

        internalData.materialScale = data.tiling;
        internalData.ambientOcclusion = data.aoActive;

        manager.doNotApply = false;

        manager.ApplyTextures();

        if(data.paintTiling == 0)
            data.paintTiling = 1;

        internalData.paintScale = data.paintTiling;

        Texture2D texture = new Texture2D(10,10);
        ImageConversion.LoadImage(texture, data.overlayTexture);

        manager.SetOverlay(texture);


        GameObject[] panels = gameObject.GetComponent<Controller>().GetPanels();

        for(int index = 0; index < panels.Length; index++) {
            if(data.panelData != null && data.panelData.Count > index)
                panels[index].GetComponent<IPanel>().FromJson(data.panelData[index]);
            else
                panels[index].GetComponent<IPanel>().FromJson(null);

            panels[index].GetComponent<IPanel>().LoadPanel();
        }
    }

    public void UpdateOldSaveFile()
    {
        //backup save file
        File.Copy(savefileName, savefileName + ".bak");

        //save new version
        Save(savefileName, false);

        oldSavePanel.SetActive(false);
    }

    public void DontUpdateOldSaveFile()
    {
        oldSavePanel.SetActive(false);
    }

    public void Save(string filename, bool exitOnSave)
    {
        if(filename != null && filename != "") {
            savefileName = filename;
            Debug.Log("Saving to " + filename);

            //change cursor to busy icon and then force update
            Cursor.SetCursor(busyCursor, Vector2.zero, CursorMode.Auto); 
            Cursor.visible = false;
            Cursor.visible = true;

            Debug.Log("SAVE: Creating SaveData object");
            SaveData_v2 data = new SaveData_v2();
            Texture2D texture;

            data.version = 2;
            Debug.Log("SAVE: Store heightmap");
            //save the heightmap
            data.heightmap = manager.GetHeightmapAsBytes();
            Debug.Log("SAVE: Store base textures");

            //save the selected materials
            data.baseTexture = new int[InternalDataScriptable.NUMBER_MATERIALS];
            data.baseTexture_colors = new byte[InternalDataScriptable.NUMBER_MATERIALS][];
            data.mixFactor = new float[InternalDataScriptable.NUMBER_MATERIALS];
            data.mixType = new int[InternalDataScriptable.NUMBER_MATERIALS];
            data.mixOffset = new float[InternalDataScriptable.NUMBER_MATERIALS];
            
            for(int index = 0; index < InternalDataScriptable.NUMBER_MATERIALS; index++) {
                if(internalData.currentMaterialIndices[index] >= (gameResources.materials.Count - internalData.customMaterials.Count)) {
                    data.baseTexture[index] = -1;
                    texture = (Texture2D)gameResources.materials[internalData.currentMaterialIndices[index]].mainTexture;
                    data.baseTexture_colors[index] = texture.EncodeToPNG();
                } else {
                    data.baseTexture[index] = internalData.currentMaterialIndices[index];
                    data.baseTexture_colors[index] = null;
                }

                data.mixType[index] = internalData.mixTypes[index];
                data.mixFactor[index] = internalData.mixFactors[index];
                data.mixOffset[index] = internalData.mixOffsets[index];
            }

            data.tiling = internalData.materialScale;
            data.aoActive = internalData.ambientOcclusion;

            Debug.Log("SAVE: Store overlay texture");
            texture = manager.GetOverlay();
            data.overlayTexture = texture.EncodeToPNG();
            data.paintTiling = internalData.paintScale;

            //data.skyData = skyPanel.GetComponent<IPanel>().ToJson();
            //data.waterData = waterPanel.GetComponent<IPanel>().ToJson();
            GameObject[] panels = gameObject.GetComponent<Controller>().GetPanels();

            data.panelData = new List<string>();
            for(int index = 0; index < panels.Length; index++) {
                data.panelData.Add(panels[index].GetComponent<IPanel>().ToJson());
            }

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

        internalData.unsavedChanges = false;
    }

    public int RemapTextureIndex(int index, bool version4)
    {
        int result = 0;
        if(version4) {
            int[] newIndices = {0, 1, 7, 8, 9, 17, 18, 25, 26, 27, 28, 29, 35, 36, 37, 44, 45, 52, 22, 69, 70, 56, 57, 53, 54};
            result = newIndices[index];
        } else {
            int[] newIndices = {0, 7, 8, 9, 36, 17, 18, 43, 28, 35, 45, 52, 22, 69, 70, 77, 56, 53, 54};
            result = newIndices[index];
        }

        return result;
    }
}

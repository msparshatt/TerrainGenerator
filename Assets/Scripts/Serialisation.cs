using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class Serialisation : MonoBehaviour
{
    [SerializeField] private Texture2D busyCursor;
    [SerializeField] private InternalDataScriptable internalData;
    [SerializeField] private InternalDataScriptable defaultData;
    [SerializeField] private MaterialSettings materialSettings;
    [SerializeField] private MaterialSettings defaultMaterialSettings;
    [SerializeField] private GameObject oldSavePanel;

    [SerializeField] private GameObject materialsPanel;
    [SerializeField] private GameObject sculptPanel;
    [SerializeField] private GameObject paintPanel;

    [SerializeField] private GameObject waterPanel;
    [SerializeField] private GameObject skyPanel;
    [SerializeField] private CameraController cameraController;


    private string savefileName;
    private GameResources gameResources;
    private TerrainManager manager;
    private HeightmapController heightmapController;
    private MaterialController materialController;
    // Start is called before the first frame update
    void Start()
    {
        manager = TerrainManager.Instance();
        heightmapController = manager.HeightmapController;
        materialController = manager.MaterialController;

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
            } else if(data.version == 2) {
                Version2Load(fileContents);
            } else if (data.version == 3) {
                Version3Load(fileContents);
            }

            //reset the cursor
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }        
    }

    public void Version1Load(string fileContents, bool version4)
    {
        SaveData_v1 data = JsonUtility.FromJson<SaveData_v1>(fileContents);
        heightmapController.CreateTerrainFromHeightmap(data.heightmap);

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

        materialSettings.mixTypes[1] = data.mixType;

        if(data.mixFactor > 0)
            data.mixFactor = 1 - data.mixFactor;

        materialSettings.mixFactors[1] = data.mixFactor;

        for(int index = 2; index < MaterialSettings.NUMBER_MATERIALS; index++) {
            materialSettings.mixTypes[index] = 1;

            materialSettings.mixFactors[index] = 0f;
        }
        if(data.tiling == 0)
            data.tiling = 1;

        materialSettings.materialScale = data.tiling;
        materialSettings.ambientOcclusion = data.aoActive;

        materials.LoadPanel();
        materialController.ApplyTextures();


        if(data.paintTiling == 0)
            data.paintTiling = 1;

        internalData.paintScale = data.paintTiling;
        paintPanel.GetComponent<PaintPanel>().LoadPanel();

        Texture2D texture = new Texture2D(10,10);
        ImageConversion.LoadImage(texture, data.overlayTexture);

        materialController.SetOverlay(texture);

        oldSavePanel.SetActive(true);
    }

    public void Version2Load(string fileContents)
    {
        SaveData_v2 data = JsonUtility.FromJson<SaveData_v2>(fileContents);
        heightmapController.CreateTerrainFromHeightmap(data.heightmap);

        MaterialsPanel materials = materialsPanel.GetComponent<MaterialsPanel>();   

        for(int index = 0; index < MaterialSettings.NUMBER_MATERIALS; index++) {
            materialSettings.useTexture[index] = data.useTexture[index];

            materialSettings.colors[index] = data.colors[index];
            if(data.baseTexture[index] == -1) {
                materials.SelectMaterialIcon(index, materials.AddBaseTexture(data.baseTexture_colors[index]));
            } else {
                materials.SelectMaterialIcon(index, data.baseTexture[index]);
            }

            if(index > 0)
            {
                materialSettings.mixTypes[index] = data.mixType[index];
                materialSettings.mixFactors[index] = data.mixFactor[index];
            } else {
                materialSettings.mixTypes[0] = 0;
                materialSettings.mixFactors[0] = 0f;
            }
        }

        if(data.tiling == 0)
            data.tiling = 1;

        materialSettings.materialScale = data.tiling;
        materialSettings.ambientOcclusion = data.aoActive;

        materialController.ApplyTextures();

        if(data.paintTiling == 0)
            data.paintTiling = 1;

        internalData.paintScale = data.paintTiling;

        Texture2D texture = new Texture2D(10,10);
        ImageConversion.LoadImage(texture, data.overlayTexture);

        materialController.SetOverlay(texture);

        GameObject[] panels = gameObject.GetComponent<Controller>().GetPanels();

        for(int index = 0; index < panels.Length; index++) {
            if(data.panelData != null && data.panelData.Count > index)
                panels[index].GetComponent<IPanel>().FromJson(data.panelData[index]);
            else
                panels[index].GetComponent<IPanel>().FromJson(null);

            panels[index].GetComponent<IPanel>().LoadPanel();
        }
    }

    public void Version3Load(string fileContents)
    {
        Debug.Log("Version 3");
        SaveData_v3 data = JsonUtility.FromJson<SaveData_v3>(fileContents);
        heightmapController.CreateTerrainFromHeightmap(data.heightmap);

        MaterialsPanel materials = materialsPanel.GetComponent<MaterialsPanel>();   

        Texture2D texture = new Texture2D(10,10);
        ImageConversion.LoadImage(texture, data.overlayTexture);

        materialController.SetOverlay(texture);

        cameraController.FromJson(data.cameraData);
        
        GameObject[] panels = gameObject.GetComponent<Controller>().GetPanels();

        for(int index = 0; index < panels.Length; index++) {
            if(data.panelData != null && data.panelData.Count > index)
                panels[index].GetComponent<IPanel>().FromJson(data.panelData[index]);
            else
                panels[index].GetComponent<IPanel>().FromJson(null);

            panels[index].GetComponent<IPanel>().LoadPanel();
        }

        materialController.ApplyTextures();
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
            SaveData_v3 data = new SaveData_v3();
            Texture2D texture;

            data.version = 3;
            Debug.Log("SAVE: Store heightmap");
            //save the heightmap
            data.terrainResolution = heightmapController.HeightmapResolution();
            data.heightmap = heightmapController.GetHeightmapAsBytes();

            Debug.Log("SAVE: Store overlay texture");
            texture = materialController.GetOverlay();
            data.overlayTexture = texture.EncodeToPNG();

            Debug.Log("SAVE: Store panel settings");
            GameObject[] panels = gameObject.GetComponent<Controller>().GetPanels();
            data.panelData = new List<string>();
            for(int index = 0; index < panels.Length; index++) {
                data.panelData.Add(panels[index].GetComponent<IPanel>().ToJson());
            }

            Debug.Log("SAVE: Store camera positions");
            data.cameraData = cameraController.ToJson();

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

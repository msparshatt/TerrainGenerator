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

        Debug.Log(version4);
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

        //check if the save contains sky data
        if(fileContents.Contains("lightTerrain")) {
            internalData.lightTerrain = data.lightTerrain;
            internalData.sunHeight = data.sunHeight;
            internalData.sunDirection = data.sunDirection;
            internalData.automaticColor = data.automaticColor;
            internalData.sunColor = data.sunColor;
            internalData.cloudActive = data.cloudActive;
            internalData.cloudXoffset = data.cloudXoffset;
            internalData.cloudYOffset = data.cloudYOffset;
            internalData.cloudScale = data.cloudScale;
            internalData.cloudIterations = data.cloudIterations;
            internalData.cloudStart = data.cloudStart;
            internalData.cloudEnd = data.cloudEnd;
            internalData.windSpeed = data.windSpeed;
            internalData.windDirection = data.windDirection;
        } else {
            internalData.lightTerrain = defaultData.lightTerrain;
            internalData.sunHeight = defaultData.sunHeight;
            internalData.sunDirection = defaultData.sunDirection;
            internalData.automaticColor = defaultData.automaticColor;
            internalData.sunColor = defaultData.sunColor;
            internalData.cloudActive = defaultData.cloudActive;
            internalData.cloudXoffset = defaultData.cloudXoffset;
            internalData.cloudYOffset = defaultData.cloudYOffset;
            internalData.cloudScale = defaultData.cloudScale;
            internalData.cloudIterations = defaultData.cloudIterations;
            internalData.cloudStart = defaultData.cloudStart;
            internalData.cloudEnd = defaultData.cloudEnd;
            internalData.windSpeed = defaultData.windSpeed;
            internalData.windDirection = defaultData.windDirection;
        }

        //check if ocean data exists
        if(fileContents.Contains("oceanActive")) {
            internalData.oceanActive = data.oceanActive;
            internalData.oceanHeight = data.oceanHeight;
            internalData.waveDirection = data.waveDirection;
            internalData.waveSpeed = data.waveSpeed;
            internalData.waveHeight = data.waveHeight;
            internalData.waveChoppyness = data.waveChoppyness;
            internalData.foamAmount = data.foamAmount;
            internalData.shoreLineActive = data.shoreLineActive;
            internalData.shorelineFoamAmount = data.shorelineFoamAmount;
        } else {
            internalData.oceanActive = defaultData.oceanActive;
            internalData.oceanHeight = defaultData.oceanHeight;
            internalData.waveDirection = defaultData.waveDirection;
            internalData.waveSpeed = defaultData.waveSpeed;
            internalData.waveHeight = defaultData.waveHeight;
            internalData.waveChoppyness = defaultData.waveChoppyness;
            internalData.foamAmount = defaultData.foamAmount;
            internalData.shoreLineActive = defaultData.shoreLineActive;
            internalData.shorelineFoamAmount = defaultData.shorelineFoamAmount;
        }

        GameObject[] panels = gameObject.GetComponent<Controller>().GetPanels();


        for(int index = 0; index < panels.Length; index++) {
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
            
            for(int index = 0; index < InternalDataScriptable.NUMBER_MATERIALS; index++) {
                if(internalData.currentMaterialIndices[index] >= (gameResources.materials.Count - internalData.customMaterials.Count)) {
                    data.baseTexture[index] = -1;
                    texture = (Texture2D)gameResources.materials[internalData.currentMaterialIndices[index]].mainTexture;
                    data.baseTexture_colors[index] = texture.EncodeToPNG();
                } else {
                    data.baseTexture[index] = internalData.currentMaterialIndices[index];
                    data.baseTexture_colors[index] = null;
                }

                data.mixType[index] = manager.mixTypes[index];
                data.mixFactor[index] = manager.mixFactors[index];
            }

            data.tiling = internalData.materialScale;
            data.aoActive = internalData.ambientOcclusion;

            Debug.Log("SAVE: Store overlay texture");
            texture = manager.GetOverlay();
            data.overlayTexture = texture.EncodeToPNG();
            data.paintTiling = internalData.paintScale;

            Debug.Log("SAVE: Sky panel data");
            data.lightTerrain = internalData.lightTerrain;
            data.sunHeight = internalData.sunHeight;
            data.sunDirection = internalData.sunDirection;
            Debug.Log(data.sunDirection);
            data.automaticColor = internalData.automaticColor;
            data.sunColor = internalData.sunColor;
            data.cloudActive = internalData.cloudActive;
            data.cloudXoffset = internalData.cloudXoffset;
            data.cloudYOffset = internalData.cloudYOffset;
            data.cloudScale = internalData.cloudScale;
            data.cloudIterations = internalData.cloudIterations;
            data.cloudStart = internalData.cloudStart;
            data.cloudEnd = internalData.cloudEnd;
            data.windSpeed = internalData.windSpeed;
            data.windDirection = internalData.windDirection;

            Debug.Log("SAVE: Water panel data");
            data.oceanActive = internalData.oceanActive;
            data.oceanHeight = internalData.oceanHeight;
            data.waveDirection = internalData.waveDirection;
            data.waveSpeed = internalData.waveSpeed;
            data.waveHeight = internalData.waveHeight;
            data.waveChoppyness = internalData.waveChoppyness;
            data.foamAmount = internalData.foamAmount;
            data.shoreLineActive = internalData.shoreLineActive;
            data.shorelineFoamAmount = internalData.shorelineFoamAmount;

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

        Debug.Log(index + " : " + result);
        return result;
    }
}

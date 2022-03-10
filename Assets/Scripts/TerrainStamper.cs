using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainStamper : MonoBehaviour
{
    public enum StampMode {Raise, Lower}
    [SerializeField] private BrushDataScriptable brushData;

    private Terrain terrain;

    public void Start()
    {
        terrain = gameObject.GetComponent<Terrain>();
    }

    public void ModifyTerrain(StampMode mode, Vector3 location, Operation operation)
    {
        TerrainData terrainData = terrain.terrainData;

        ModifyRectangle rectangle = new ModifyRectangle(location, brushData, terrain, new Vector2Int(terrainData.heightmapResolution, terrainData.heightmapResolution));
        float[,] heights = terrainData.GetHeights(rectangle.topLeft.x, rectangle.topLeft.y, rectangle.size.x, rectangle.size.y);
        float[,] changes = new float[rectangle.size.y, rectangle.size.x];

        for (int x = 0; x < rectangle.size.x; x++)
        {
            for (int y = 0; y < rectangle.size.y; y++)
            {                   
                float maskValue = rectangle.GetMaskValue(new Vector2(x, y), -brushData.brushRotation, brushData.brushStrength);
                float strength = brushData.brushStrength;
                if(mode == StampMode.Lower) 
                    strength *= -1;

                heights[y, x] += maskValue  * strength;
                changes[y,x] =  maskValue  * strength;
            }
        }

        terrainData.SetHeights(rectangle.topLeft.x, rectangle.topLeft.y, heights);
        operation.AddSubOperation(new SculptSubOperation(terrain, rectangle.topLeft, rectangle.size, changes));
    }
}

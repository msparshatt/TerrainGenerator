using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainSculpter : MonoBehaviour
{
    public enum SculptMode {Raise, Lower, Flatten}
    [SerializeField] private BrushDataScriptable brushData;

    private Terrain terrain;

    public void Start()
    {
        terrain = gameObject.GetComponent<Terrain>();
    }

    public void SculptTerrain(SculptMode mode, Vector3 location, Operation sculptOperation)
    {
        if (mode == SculptMode.Raise) {
            ModifyTerrain(location, brushData.brushStrength, sculptOperation);
        } else if (mode == SculptMode.Lower) {
            ModifyTerrain(location, -brushData.brushStrength, sculptOperation);
        } else {
            FlattenTerrain(location, sculptOperation);
        }
    }

    public void UndoSculpt(Vector2Int topLeft, Vector2Int size, float[,] changes)
    {
        TerrainData terrainData = terrain.terrainData;
        float[,] heights = terrainData.GetHeights(topLeft.x, topLeft.y, size.x, size.y);

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {                   
                heights[y, x] -= changes[y,x];
            }
        }

        terrainData.SetHeights(topLeft.x, topLeft.y, heights);
    }
    public void RedoSculpt(Vector2Int topLeft, Vector2Int size, float[,] changes)
    {
        TerrainData terrainData = terrain.terrainData;
        float[,] heights = terrainData.GetHeights(topLeft.x, topLeft.y, size.x, size.y);

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {                   
                heights[y, x] += changes[y,x];
            }
        }

        terrainData.SetHeights(topLeft.x, topLeft.y, heights);
    }

    private void ModifyTerrain(Vector3 location, float effectIncrement, Operation sculptOperation)
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

                heights[y, x] += (effectIncrement * Time.smoothDeltaTime * maskValue);
                changes[y,x] =  (effectIncrement * Time.smoothDeltaTime * maskValue);
            }
        }

        terrainData.SetHeights(rectangle.topLeft.x, rectangle.topLeft.y, heights);
        sculptOperation.AddSubOperation(new SculptSubOperation(terrain, rectangle.topLeft, rectangle.size, changes));
    }

    private void FlattenTerrain(Vector3 location, Operation sculptOperation)
    {

        TerrainData terrainData = terrain.terrainData;

        ModifyRectangle rectangle = new ModifyRectangle(location, brushData, terrain, new Vector2Int(terrainData.heightmapResolution, terrainData.heightmapResolution));
        float[,] heights = terrainData.GetHeights(rectangle.topLeft.x, rectangle.topLeft.y, rectangle.size.x, rectangle.size.y);
        float[,] changes = new float[rectangle.size.y, rectangle.size.x];
      
        float averageHeight = CalculateAverageHeight(rectangle, heights);

        //move each height value towards the average depending on the mask value and the strength
        for (int x = 0; x < rectangle.size.x; x++)
        {
            for (int y = 0; y < rectangle.size.y; y++)
            {
                float maskValue = rectangle.GetMaskValue(new Vector2(x, y), -brushData.brushRotation, brushData.brushStrength);

                float height = heights[y, x];
                heights[y, x] += (averageHeight - height) * maskValue * brushData.brushStrength;
                changes[y, x] = (averageHeight - height) * maskValue * brushData.brushStrength;
            }
        }

        terrainData.SetHeights(rectangle.topLeft.x, rectangle.topLeft.y, heights);
        sculptOperation.AddSubOperation(new SculptSubOperation(terrain, rectangle.topLeft, rectangle.size, changes));
    }

    public void SetHeight(Vector3 location, Operation sculptOperation)
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
                float heightChange = brushData.brushHeight - heights[y,x];

                heights[y, x] += (heightChange * Time.smoothDeltaTime * maskValue);
                changes[y,x] =  (heightChange * Time.smoothDeltaTime * maskValue);
            }
        }

        terrainData.SetHeights(rectangle.topLeft.x, rectangle.topLeft.y, heights);
        sculptOperation.AddSubOperation(new SculptSubOperation(terrain, rectangle.topLeft, rectangle.size, changes));        
    }

    private float CalculateAverageHeight(ModifyRectangle rectangle, float[,] heights)
    {
        //Calculate the average height
        int counter = 0;
        float total = 0f;
        for (int x = 0; x < rectangle.size.x; x++)
        {
            for (int y = 0; y < rectangle.size.y; y++)
            {
                counter++;
                total += heights[y, x];
            }
        }

        float averageHeight = total/counter;

        return averageHeight;
    }

    public float GetHeightAtPoint(Vector3 location)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainSize = terrainData.size;

        Vector3 tempCoord = (location - terrain.GetPosition()); //get target position relative to the terrain
        Vector2 locationInTerrain = TranslateCoordinates(new Vector2(tempCoord.x, tempCoord.z), new Vector2(terrainSize.x, terrainSize.z), new Vector2(terrainData.heightmapResolution, terrainData.heightmapResolution));

        return terrainData.GetHeight((int)locationInTerrain.x, (int)locationInTerrain.y);
    }

    private Vector2 TranslateCoordinates(Vector2 coords, Vector2 terrainSize, Vector2 mapSize)
    {
        float x = coords.x * mapSize.x / terrainSize.x;
        float y = coords.y * mapSize.y / terrainSize.y;
        return new Vector2(x, y);
    }

    public void SetTerrainHeight(float height)
    {
        TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.heightmapResolution;

        float[,] heights = new float[resolution, resolution];

        for(int x = 0; x < resolution; x++) {
            for(int y = 0; y < resolution; y++) {
                heights[y, x] = height;
            }
        }

        terrainData.SetHeights(0,0, heights);
    }
}

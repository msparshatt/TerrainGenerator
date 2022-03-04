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

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector2 RotateVector(float oldX, float oldY, float degrees)
    {
        float radians = Mathf.Deg2Rad * degrees;
        float newX = oldX * Mathf.Cos(radians) - oldY * Mathf.Sin(radians);
        float newY = oldX * Mathf.Sin(radians) + oldY * Mathf.Cos(radians);
        return new Vector2(newX, newY);
    }

    public void ModifyTerrain(StampMode mode, Vector3 location, Operation operation)
    {
        TerrainData terrainData = terrain.terrainData;

        ModifyRectangle rectangle = GetModifyRectangle(location);
        float[,] heights = terrainData.GetHeights(rectangle.topLeft.x, rectangle.topLeft.y, rectangle.size.x, rectangle.size.y);
        float[,] changes = new float[rectangle.size.y, rectangle.size.x];

        for (int x = 0; x < rectangle.size.x; x++)
        {
            for (int y = 0; y < rectangle.size.y; y++)
            {                   
                float posX = x - (rectangle.size.x / 2);
                float posY = y - (rectangle.size.y / 2);

                Vector2 rotatedVector = RotateVector(posX, posY, -brushData.brushRotation);

                int newX = Mathf.RoundToInt(rotatedVector.x) + (rectangle.size.x / 2);
                int newY = Mathf.RoundToInt(rotatedVector.y) + (rectangle.size.y / 2);

                float maskValue = 0;
                if(newY >= 0 && newY < rectangle.size.y && newX >= 0 && newX < rectangle.size.x) {
                    maskValue = rectangle.mask[newY + rectangle.offset.y, newX + rectangle.offset.x];
                }
                   
                float strength = brushData.brushStrength;
                if(mode == StampMode.Lower)
                    strength *= -1;

                heights[y, x] += maskValue  * brushData.brushStrength;
                changes[y,x] =  maskValue  * brushData.brushStrength;
            }
        }

        terrainData.SetHeights(rectangle.topLeft.x, rectangle.topLeft.y, heights);
        operation.AddSubOperation(new SculptSubOperation(terrain, rectangle.topLeft, rectangle.size, changes));
    }

    //utility functions
    //translate coordinate from gamespace to heightmap/texture space
    private Vector2 TranslateCoordinates(Vector2 coords, Vector2 terrainSize, Vector2 mapSize)
    {
        float x = coords.x * mapSize.x / terrainSize.x;
        float y = coords.y * mapSize.y / terrainSize.y;
        return new Vector2(x, y);
    }

    private ModifyRectangle GetModifyRectangle(Vector3 location)
    {
        int offset = brushData.brushRadius / 2;

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainSize = terrainData.size;
        int terrainMapSize = terrainData.heightmapResolution;

        Vector3 tempCoord = (location - terrain.GetPosition()); //get target position relative to the terrain
        Vector2 locationInTerrain = TranslateCoordinates(new Vector2(tempCoord.x, tempCoord.z), new Vector2(terrainSize.x, terrainSize.z), new Vector2(terrainMapSize, terrainMapSize));

        //translate from the center of the sculpt brush to the top left
        int startX = (int)(locationInTerrain.x - (offset / terrainSize.x * terrainMapSize)); 
        int startY = (int)(locationInTerrain.y - (offset / terrainSize.y * terrainMapSize));

        //calculate the size of the brush within the heightmap
        int width = (int)(brushData.brushRadius / terrainSize.x * terrainMapSize);
        int length = (int)(brushData.brushRadius / terrainSize.z * terrainMapSize);
        float[,] mask = brushData.getMask(length, width);

        int maskOffsetX = 0;
        int maskOffsetY = 0;

        //check if the brush goes over the edge of the terrain
        if(startX + width > terrainMapSize) {
            width = terrainMapSize - startX;
        } else if(startX < 0) {
            width += startX;
            maskOffsetX = - startX;
            startX = 0;
        }
        if(startY + length > terrainMapSize) {
            length = terrainMapSize- startY;
        } else if(startY< 0) {
            length += startY;
            maskOffsetY = - startY;
            startY = 0;
        }

        ModifyRectangle result = new ModifyRectangle();
        result.topLeft = new Vector2Int(startX, startY);
        result.size = new Vector2Int(width, length);
        result.offset = new Vector2Int(maskOffsetX, maskOffsetY);
        result.mask = mask;

        return result;
    }
}

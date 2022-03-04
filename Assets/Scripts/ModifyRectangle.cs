using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifyRectangle
{
    public Vector2Int topLeft;
    public Vector2Int size;
    public Vector2Int fullSize;
    public Vector2Int offset;

    public float[,] mask;

    private Vector2 TranslateCoordinates(Vector2 coords, Vector2 terrainSize, Vector2 mapSize)
    {
        float x = coords.x * mapSize.x / terrainSize.x;
        float y = coords.y * mapSize.y / terrainSize.y;
        return new Vector2(x, y);
    }

    public ModifyRectangle(Vector3 location, BrushDataScriptable brushData, Terrain terrain, Vector2Int resolution)
    {
        int midPoint = brushData.brushRadius / 2;

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainSize = terrainData.size;

        Vector3 tempCoord = (location - terrain.GetPosition()); //get target position relative to the terrain
        Vector2 locationInTerrain = TranslateCoordinates(new Vector2(tempCoord.x, tempCoord.z), new Vector2(terrainSize.x, terrainSize.z), resolution);

        //translate from the center of the sculpt brush to the top left
        int startX = (int)(locationInTerrain.x - (midPoint / terrainSize.x * resolution.x)); 
        int startY = (int)(locationInTerrain.y - (midPoint / terrainSize.y * resolution.y));

        //calculate the size of the brush within the heightmap
        int width = (int)(brushData.brushRadius / terrainSize.x * resolution.x);
        int length = (int)(brushData.brushRadius / terrainSize.z * resolution.y);
        mask = brushData.getMask(length, width);

        int maskOffsetX = 0;
        int maskOffsetY = 0;

        fullSize = new Vector2Int(width, length);

        //check if the brush goes over the edge of the terrain
        if(startX + width > resolution.x) {
            width = resolution.x - startX;
        } else if(startX < 0) {
            width += startX;
            maskOffsetX = - startX;
            startX = 0;
        }
        if(startY + length > resolution.y) {
            length = resolution.y - startY;
        } else if(startY< 0) {
            length += startY;
            maskOffsetY = - startY;
            startY = 0;
        }

        topLeft = new Vector2Int(startX, startY);
        size = new Vector2Int(width, length);
        offset = new Vector2Int(maskOffsetX, maskOffsetY);
    }

    public Vector2 RotateVector(Vector2 position, float degrees)
    {
        float radians = Mathf.Deg2Rad * degrees;
        float newX = position.x * Mathf.Cos(radians) - position.y * Mathf.Sin(radians);
        float newY = position.x * Mathf.Sin(radians) + position.y * Mathf.Cos(radians);
        return new Vector2(newX, newY);
    }

    public Vector2 RotatePointRoundCenter(Vector2 point, float degrees)
    {
        Vector2 newPoint = (point + offset) - (fullSize / 2);
        return  RotateVector(newPoint, degrees) + (fullSize / 2);
    }

    public float GetMaskValue(Vector2 point, float degrees, float strength)
    {
        Vector2 position = RotatePointRoundCenter(point, degrees);
        float maskValue = 0;
        if(position.y >= 0 && position.y < fullSize.y && position.x >= 0 && position.x < fullSize.x) {
            maskValue = mask[(int)position.y, (int)position.x] * strength * 5;
        }

        return maskValue;
    }
}

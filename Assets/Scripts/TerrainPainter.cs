using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainPainter : MonoBehaviour
{
    public enum PaintMode {Paint, Erase}
    public BrushDataScriptable brushData;

    private Terrain terrain;
    public void Start()
    {
        terrain = gameObject.GetComponent<Terrain>();
    }

    public void PaintTerrain(PaintMode mode, Texture2D texture, Vector3 location, Operation operation)
    {
        //get terrain data
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainSize = terrainData.size;

        //get texture data
        int textureSizeX = texture.width;
        int textureSizeY = texture.height;
        int offset = brushData.brushRadius / 2;

        //calculate the position within the overlay
        Vector3 tempCoord = (location - terrain.GetPosition());
        Vector2 locationInTerrain = TranslateCoordinates(new Vector2(tempCoord.x, tempCoord.z), new Vector2(terrainSize.x, terrainSize.z), new Vector2(textureSizeX, textureSizeY));

        int startX = (int)(locationInTerrain.x - ((offset * textureSizeX)  / terrainSize.x ));
        int startY = (int)(locationInTerrain.y - ((offset * textureSizeY)  / terrainSize.y));

        int width = (int)((brushData.brushRadius * textureSizeX) / terrainSize.x);
        int length = (int)((brushData.brushRadius * textureSizeY) / terrainSize.y);

        float[,] mask = brushData.getMask(width, length);
        int maskOffsetX = 0;
        int maskOffsetY = 0;

        if(startX + width >= textureSizeX) {
            width = (int)(textureSizeX - startX - 1);
        } else if(startX < 0) {
            width += startX;
            maskOffsetX = - startX;
            startX = 0;
        }
        if(startY + length >= textureSizeY) {
            length = (int)(textureSizeY - startY - 1);
        } else if(startY < 0) {
            length += startY;
            maskOffsetY = - startY;
            startY = 0;
        }

        Color[] pixels = texture.GetPixels(startX, startY, width, length); 
        Color[] paint = brushData.paintTexture.GetPixels(startX, startY, width, length);
        int arrayLength = width * length;

        Color[] changes = texture.GetPixels(startX, startY, width, length); //get another copy of the color data in order to store any changes

        int index = 0;
        float maskValue = 0;
        for (int y = 0; y < length; y++)
        {
            for (int x = 0; x < width; x++)
            {
                maskValue = mask[y + maskOffsetY, x + maskOffsetX] * brushData.brushStrength * 5;

                if(mode == PaintMode.Erase) {
                    pixels[index].a -= maskValue;
                    
                    if(pixels[index].a < 0)
                        pixels[index].a = 0;
                } else {
                    if(pixels[index].a == 0) {
                        pixels[index] = paint[index];
                        pixels[index].a = maskValue;
                    } else {
                        pixels[index] = pixels[index] * (1 - maskValue) + paint[index] * maskValue;
                    }
                }
                index++;
            }
        }        

        for(int i = 0; i < changes.Length; i++) {
            changes[i] = pixels[i] - changes[i];
        }

        texture.SetPixels(startX, startY, width, length, pixels);
        texture.Apply(true);
        operation.AddSubOperation(new PaintSubOperation(terrain, texture, new Vector2Int(startX, startY), new Vector2Int(width, length), changes));
    }

    public void UndoPaint(Texture2D texture, Vector2Int topLeft, Vector2Int size, Color[] changes)
    {
        Color[] pixels = texture.GetPixels(topLeft.x, topLeft.y, size.x, size.y); 
        for(int i = 0; i < changes.Length; i++) {
            pixels[i] = pixels[i] - changes[i];
        }
        texture.SetPixels(topLeft.x, topLeft.y, size.x, size.y, pixels);
        texture.Apply(true);        
    }

    public void RedoPaint(Texture2D texture, Vector2Int topLeft, Vector2Int size, Color[] changes)
    {
        Color[] pixels = texture.GetPixels(topLeft.x, topLeft.y, size.x, size.y); 
        for(int i = 0; i < changes.Length; i++) {
            pixels[i] = pixels[i] + changes[i];
        }
        texture.SetPixels(topLeft.x, topLeft.y, size.x, size.y, pixels);
        texture.Apply(true);        
    }

    //utility functions
    //translate coordinate from gamespace to heightmap/texture space
    private Vector2 TranslateCoordinates(Vector2 coords, Vector2 terrainSize, Vector2 mapSize)
    {
        float x = coords.x * mapSize.x / terrainSize.x;
        float y = coords.y * mapSize.y / terrainSize.y;
        return new Vector2(x, y);
    }
}

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

    public Vector2 RotateVector(float oldX, float oldY, float degrees)
    {
        float radians = Mathf.Deg2Rad * degrees;
        float newX = oldX * Mathf.Cos(radians) - oldY * Mathf.Sin(radians);
        float newY = oldX * Mathf.Sin(radians) + oldY * Mathf.Cos(radians);
        return new Vector2(newX, newY);
    }

    public async void PaintTerrain(PaintMode mode, Texture2D texture, Vector3 location, Operation operation)
    {
        //get terrain data
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainSize = terrainData.size;

        //get texture data
        int textureSizeX = texture.width;
        int textureSizeY = texture.height;
        int offset = brushData.brushRadius / 2;

        ModifyRectangle rectangle = new ModifyRectangle(location, brushData, terrain, new Vector2Int(textureSizeX, textureSizeY));

        Color[] pixels = texture.GetPixels(rectangle.topLeft.x, rectangle.topLeft.y, rectangle.size.x, rectangle.size.y); 

        int arrayLength = rectangle.fullSize.x * rectangle.fullSize.y;
        Color[] paint = new Color[arrayLength];
        int count = 0;
        for(int i = 0; i < rectangle.size.y; i++) {
            int y = (int)(i * brushData.textureScale % brushData.paintTexture.height);
            for(int j = 0; j < rectangle.size.x; j++) {
                int x = (int)(j * brushData.textureScale % brushData.paintTexture.width);

                paint[count] = brushData.paintTexture.GetPixel(x, y);                
                count++;
            }
        }

/*        for(int li = 0; li < arrayLength; li++) {
            Debug.Log(paint_old[li] + ":" + paint[li]);
        }*/

        Color[] changes = texture.GetPixels(rectangle.topLeft.x, rectangle.topLeft.y, rectangle.size.x, rectangle.size.y); //get another copy of the color data in order to store any changes

        int index = 0;
        float maskValue = 0;
        for (int y = 0; y < rectangle.size.y; y++)
        {
            for (int x = 0; x < rectangle.size.x; x++)
            {
                maskValue = rectangle.GetMaskValue(new Vector2(x, y), -brushData.brushRotation, brushData.brushStrength);

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
                        pixels[index].a += maskValue;
                    }
                }
                changes[index] = pixels[index] - changes[index];
                index++;
            }
        }        

        texture.SetPixels(rectangle.topLeft.x, rectangle.topLeft.y, rectangle.size.x, rectangle.size.y, pixels);
        texture.Apply(true);
        operation.AddSubOperation(new PaintSubOperation(terrain, texture, new Vector2Int(rectangle.topLeft.x, rectangle.topLeft.y), new Vector2Int(rectangle.size.x, rectangle.size.y), changes));
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

    public void ClearTexture(Texture2D texture)
    {
        int sizeX = texture.width;
        int sizeY = texture.height;
        Color[] data = new Color[sizeX * sizeY];
        int index = 0;

        for(int u = 0; u < sizeX; u++) {
            for(int v = 0; v < sizeY; v++) {
                data[index] = new Color(0, 0, 0, 0);
                index++;
            }
        }

        texture.SetPixels(0,0, sizeX, sizeY, data);
        texture.Apply(true);
    }
}
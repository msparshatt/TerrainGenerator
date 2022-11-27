using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainModifier : MonoBehaviour
{
    public enum SculptMode {Raise, Lower, Flatten}
    public enum StampMode {Raise, Lower}

    [SerializeField] private BrushDataScriptable brushData;
    [SerializeField] private BrushDataScriptable setHeightBrushData;

    [SerializeField] private BrushDataScriptable erosionBrushData;
    [SerializeField] private ErosionDataScriptable erosionData;
    [SerializeField] private ComputeShader erosionShader;

    [SerializeField] private BrushDataScriptable stampBrushData;

    private int erosionIterations = 16;

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

        ModifyRectangle rectangle = new ModifyRectangle(location, setHeightBrushData, terrain, new Vector2Int(terrainData.heightmapResolution, terrainData.heightmapResolution));
        float[,] heights = terrainData.GetHeights(rectangle.topLeft.x, rectangle.topLeft.y, rectangle.size.x, rectangle.size.y);
        float[,] changes = new float[rectangle.size.y, rectangle.size.x];

        for (int x = 0; x < rectangle.size.x; x++)
        {
            for (int y = 0; y < rectangle.size.y; y++)
            {                   
                float maskValue = rectangle.GetMaskValue(new Vector2(x, y), -setHeightBrushData.brushRotation, setHeightBrushData.brushStrength);
                float heightChange = setHeightBrushData.brushHeight - heights[y,x];

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

    public void ErodeTerrain(Vector3 location, Operation sculptOperation)
    {
        TerrainData terrainData = terrain.terrainData;

        //read the current height values
        ModifyRectangle rectangle = new ModifyRectangle(location, erosionBrushData, terrain, new Vector2Int(terrainData.heightmapResolution, terrainData.heightmapResolution));
        float[,] heights = terrainData.GetHeights(rectangle.topLeft.x, rectangle.topLeft.y, rectangle.size.x, rectangle.size.y);    

        int left = rectangle.topLeft.x - erosionData.erosionBrushRadius;
        int top = rectangle.topLeft.y - erosionData.erosionBrushRadius;
        int width = rectangle.size.x + 2 * erosionData.erosionBrushRadius;
        int length = rectangle.size.y + 2 * erosionData.erosionBrushRadius;

        float[,] newHeights;

        if(left < 0 || top < 0 || (left + width) >= terrainData.heightmapResolution || (top + length) >= terrainData.heightmapResolution) {

            newHeights = new float[length, width];

            int leftOffset = 0;
            int topOffset = 0;
            if(left < 0) {
                leftOffset = - left;
                width += left;
                left = 0;
            }
            if(left + width >= terrainData.heightmapResolution) {
                width = terrainData.heightmapResolution - left;
            }
            if(top < 0) {
                topOffset = - top;
                length += top;
                top = 0;
            }
            if(top + length >= terrainData.heightmapResolution) {
                length = terrainData.heightmapResolution - top;
            }

            float[,] heightArray = terrainData.GetHeights(left, top, width, length);

            for(int i = 0; i < width; i++) {
                for(int j = 0; j < length; j++) {
                    float height = heightArray[j, i];
                    newHeights[j + topOffset, i + leftOffset] = height;
                }
            }
        } else {
            newHeights = terrainData.GetHeights(left, top , width, length);        
        }
        float[,] changes = new float[rectangle.size.y, rectangle.size.x];

        erosionIterations = rectangle.size.x * rectangle.size.y;

        if(erosionIterations < 16)
            erosionIterations = 16;

        //erode the newheight values
        newHeights = Erosion(newHeights, rectangle.size.x, rectangle.size.y);

        //modify the heights based on the brush
        for (int x = 0; x < rectangle.size.x; x++)
        {
            for (int y = 0; y < rectangle.size.y; y++)
            {                   
                float maskValue = rectangle.GetMaskValue(new Vector2(x, y), -erosionBrushData.brushRotation, erosionBrushData.brushStrength);

                changes[y,x] =  (newHeights[y,x] - heights[y,x]) * maskValue * 0.05f;
                heights[y, x] += changes[y,x];
            }
        }

        terrainData.SetHeights(rectangle.topLeft.x, rectangle.topLeft.y, heights);
        sculptOperation.AddSubOperation(new SculptSubOperation(terrain, rectangle.topLeft, rectangle.size, changes));
    }

    public float[,] Erosion (float[,] heightmap, int mapWidth, int mapLength) 
    {
        float[] map = ArrayHelper.ConvertTo1DFloatArray(heightmap);

        int numThreads = erosionIterations / 16;

        // Create brush
        List<int> brushIndexOffsets = new List<int> ();
        List<float> brushWeights = new List<float> ();

        float weightSum = 0;
        for (int brushY = -erosionData.erosionBrushRadius; brushY <= erosionData.erosionBrushRadius; brushY++) {
            for (int brushX = -erosionData.erosionBrushRadius; brushX <= erosionData.erosionBrushRadius; brushX++) {
                float sqrDst = brushX * brushX + brushY * brushY;
                if (sqrDst < erosionData.erosionBrushRadius * erosionData.erosionBrushRadius) {
                    brushIndexOffsets.Add (brushY * mapWidth + brushX);
                    float brushWeight = 1 - Mathf.Sqrt (sqrDst) / erosionData.erosionBrushRadius;
                    weightSum += brushWeight;
                    brushWeights.Add (brushWeight);
                }
            }
        }
        for (int i = 0; i < brushWeights.Count; i++) {
            brushWeights[i] /= weightSum;
        }

        // Send brush data to compute shader

        int erodeKernel = erosionShader.FindKernel("Erode2");
        ComputeBuffer brushIndexBuffer = new ComputeBuffer (brushIndexOffsets.Count, sizeof (int));
        ComputeBuffer brushWeightBuffer = new ComputeBuffer (brushWeights.Count, sizeof (float));
        brushIndexBuffer.SetData (brushIndexOffsets);
        brushWeightBuffer.SetData (brushWeights);
        erosionShader.SetBuffer (erodeKernel, "brushIndices", brushIndexBuffer);
        erosionShader.SetBuffer (erodeKernel, "brushWeights", brushWeightBuffer);

        // Generate random indices for droplet placement
        int[] randomIndices = new int[erosionIterations];
        for (int i = 0; i < erosionIterations; i++) {
            int randomX = Random.Range (erosionData.erosionBrushRadius, mapWidth + erosionData.erosionBrushRadius);
            int randomY = Random.Range (erosionData.erosionBrushRadius, mapLength + erosionData.erosionBrushRadius);
            randomIndices[i] = randomY * (mapWidth + 2 * erosionData.erosionBrushRadius) + randomX;
        }

        // Send random indices to compute shader
        ComputeBuffer randomIndexBuffer = new ComputeBuffer (randomIndices.Length, sizeof (int));
        randomIndexBuffer.SetData (randomIndices);
        erosionShader.SetBuffer (erodeKernel, "randomIndices", randomIndexBuffer);

        // Heightmap buffer
        ComputeBuffer mapBuffer = new ComputeBuffer (map.Length, sizeof (float));
        mapBuffer.SetData (map);
        erosionShader.SetBuffer (erodeKernel, "map", mapBuffer);

        // Settings
        erosionShader.SetInt ("borderSize", erosionData.erosionBrushRadius);
        erosionShader.SetInt ("mapWidth", mapWidth + erosionData.erosionBrushRadius * 2);
        erosionShader.SetInt ("mapLength", mapLength + erosionData.erosionBrushRadius * 2);
        erosionShader.SetInt ("brushLength", brushIndexOffsets.Count);
        erosionShader.SetInt ("maxLifetime", erosionData.lifetime);
        erosionShader.SetFloat ("inertia", erosionData.inertia);
        erosionShader.SetFloat ("sedimentCapacityFactor", erosionData.sedimentCapacityFactor);
        erosionShader.SetFloat ("minSedimentCapacity", erosionData.minSedimentCapacity);
        erosionShader.SetFloat ("depositSpeed", erosionData.depositSpeed);
        erosionShader.SetFloat ("erodeSpeed", erosionData.erodeSpeed);
        erosionShader.SetFloat ("evaporateSpeed", erosionData.evaporateSpeed);
        erosionShader.SetFloat ("gravity", erosionData.gravity);
        erosionShader.SetFloat ("startSpeed", erosionData.startSpeed);
        erosionShader.SetFloat ("startWater", erosionData.startWater);

        // Run compute shader
        erosionShader.Dispatch (erodeKernel, numThreads, 1, 1);
        mapBuffer.GetData (map);

        // Release buffers
        mapBuffer.Release ();
        randomIndexBuffer.Release ();
        brushIndexBuffer.Release ();
        brushWeightBuffer.Release ();

        return ArrayHelper.ConvertTo2DArray(map, mapWidth, mapLength, erosionData.erosionBrushRadius);
    }

    public void StampTerrain(StampMode mode, Vector3 location, Operation operation)
    {
        TerrainData terrainData = terrain.terrainData;

        ModifyRectangle rectangle = new ModifyRectangle(location, stampBrushData, terrain, new Vector2Int(terrainData.heightmapResolution, terrainData.heightmapResolution));
        float[,] heights = terrainData.GetHeights(rectangle.topLeft.x, rectangle.topLeft.y, rectangle.size.x, rectangle.size.y);
        float[,] changes = new float[rectangle.size.y, rectangle.size.x];

        for (int x = 0; x < rectangle.size.x; x++)
        {
            for (int y = 0; y < rectangle.size.y; y++)
            {                   
                float maskValue = rectangle.GetMaskValue(new Vector2(x, y), -stampBrushData.brushRotation, stampBrushData.brushStrength);
                float strength = stampBrushData.brushStrength;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainEroder : MonoBehaviour
{
    [SerializeField] private BrushDataScriptable brushData;
    [SerializeField] private ErosionDataScriptable erosionData;
    [SerializeField] private ComputeShader erosionShader;

    private int erosionIterations = 16;
/*    private int erosionBrushRadius = 5;
    private int lifetime = 30;
    private float sedimentCapacityFactor = 9f;
    private float inertia = 0.3f;
    private float depositSpeed = 0.5f;
    private float erodeSpeed = 0.5f;
    private float startSpeed = 1f;
    private float evaporateSpeed = 0.01f;
    private float startWater = 1f;
    private float gravity = 10f;*/

    private Terrain terrain;
    private TerrainManager manager;

    // Start is called before the first frame update
    void Start()
    {
        terrain = gameObject.GetComponent<Terrain>();        
        manager = TerrainManager.instance;
    }
  
    public async void ErodeTerrain(Vector3 location, Operation sculptOperation)
    {
        TerrainData terrainData = terrain.terrainData;

        //read the current height values
        ModifyRectangle rectangle = new ModifyRectangle(location, brushData, terrain, new Vector2Int(terrainData.heightmapResolution, terrainData.heightmapResolution));
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
                float maskValue = rectangle.GetMaskValue(new Vector2(x, y), -brushData.brushRotation, brushData.brushStrength);

                changes[y,x] =  (newHeights[y,x] - heights[y,x]) * maskValue * 0.05f;
                heights[y, x] += changes[y,x];
            }
        }

        terrainData.SetHeights(rectangle.topLeft.x, rectangle.topLeft.y, heights);
        sculptOperation.AddSubOperation(new SculptSubOperation(terrain, rectangle.topLeft, rectangle.size, changes));
    }

    public float[,] Erosion (float[,] heightmap, int mapWidth, int mapLength) 
    {
        float[] map = manager.ConvertTo1DFloatArray(heightmap);

        float minSedimentCapacity = 0.01f;

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

        return manager.ConvertTo2DArray(map, mapWidth, mapLength, erosionData.erosionBrushRadius);
    }
}

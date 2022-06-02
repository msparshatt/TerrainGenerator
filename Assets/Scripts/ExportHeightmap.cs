using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExportHeightmap 
{
    private static ExportHeightmap _instance;
    public static ExportHeightmap instance {
        get {
            if(_instance == null)
                _instance = new ExportHeightmap();

            return _instance;
        }
    }

    public void Export(string fileName)
    {
        if(fileName == "")
            return;

        WriteHeightmap(fileName);
    }

    public byte[] GetHeightmap()
    {
        TerrainData data = TerrainManager.Instance().TerrainData;
        //get the terrain resolution
        int h = data.heightmapResolution;
        int w = data.heightmapResolution;
             
        //read the height data
        float[,] rawHeights = data.GetHeights(0, 0, w, h);

        //convert the height array into a byte array
        byte[] bytes = ConvertTo1DArray(rawHeights);

        return bytes;
    }
    public void WriteHeightmap(string path)
    {
        if (path != null && path.Length != 0)
        {
            byte[] bytes = GetHeightmap();

            //create the output file
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
            System.IO.FileStream stream = fileInfo.Create();
            

            //write the data and close the stream
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
        }
    }

    //convert a 2D float array into a 1D byte array in order to write it to a file
    private byte[] ConvertTo1DArray(float[,] heights)
    {
        //create the byte array
        byte[] byteArray = new byte[heights.GetLength(0) * heights.GetLength(1) * 2];
        int k = 0;
        for (int i = 0; i < heights.GetLength(0); i++)
        {
            for (int j = 0; j < heights.GetLength(1); j++)
            {
                float height = heights[i, j];

                //Get rid of low sections at top and right
                //need a better solution
                if(height == 0) {
                    height = 0.5f;
                }
                int value = (int) (height * 65535); //convert from float to an integer
                byteArray[k++] = (byte)(value % 256); // write LSB
                byteArray[k++] = (byte)(value >> 8); // write MSB                
            }
        }
        return byteArray;
    }    
}

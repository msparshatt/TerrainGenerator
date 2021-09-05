// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// C # manual conversion work by Yun Kyu Choi
 
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.IO;
using System.Text;
using Crosstales.FB;
using System.Collections.Generic;
 
enum SaveFormat { Triangles, Quads }
enum SaveResolution { Full=0, Half, Quarter, Eighth, Sixteenth }

struct ObjMaterial
{
	public string name;
	public Texture2D texture;
    public Texture2D aoTexture;
    public Texture2D detailTexture;

//    public Texture2D normalMap;

} 
class ExportTerrain 
{
    SaveFormat saveFormat = SaveFormat.Triangles;
    SaveResolution saveResolution = SaveResolution.Half;
 
    public Terrain terrainObject;
    public Dropdown scaleDropDown;
 
    public Texture2D busyCursor;

    private TerrainData terrain;
    private Vector3 terrainPos;

    private int totalCount;
    private int progressUpdateInterval = 10000;
 
    private static ExportTerrain _instance;

    public static ExportTerrain instance {
        get {
            if(_instance == null)
                _instance = new ExportTerrain();

            return _instance;
        }
    }
      
   public void Export(bool applyAO)
   {
        if (terrainObject)
        {
            terrain = terrainObject.terrainData;
            terrainPos = terrainObject.transform.position;
        }

        string fileName = FileBrowser.SaveFile("terrain.obj", "obj");
        float scalefactor = 0.02f * Mathf.Pow(2, scaleDropDown.value); //reduce the size so it isn't too large for FlowScape

        if(fileName == "")
            return;

        Cursor.SetCursor(busyCursor, Vector2.zero, CursorMode.Auto); 

        //force the cursor to update
        Cursor.visible = false;
        Cursor.visible = true;

        terrain = terrainObject.terrainData;
        int w = terrain.heightmapResolution;
        int h = terrain.heightmapResolution;
        Vector3 meshScale = terrain.size;
        int tRes = (int)Mathf.Pow(2, (int)saveResolution );
        meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
        Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
        float[,] tData = terrain.GetHeights(0, 0, w, h);
 
        w = (w - 1) / tRes + 1;
        h = (h - 1) / tRes + 1;
        Vector3[] tVertices = new Vector3[w * h];
        Vector2[] tUV = new Vector2[w * h];
 
        int[] tPolys;
 
        if (saveFormat == SaveFormat.Triangles)
        {
            tPolys = new int[(w - 1) * (h - 1) * 6];
        }
        else
        {
            tPolys = new int[(w - 1) * (h - 1) * 4];
        }
 
        float height;
        // Build vertices and UVs
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                height = tData[x * tRes, y * tRes];
                if(height == 0) 
                    height = 0.5f;

                tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(-y, height, x)) + terrainPos;
                tUV[y * w + x] = Vector2.Scale( new Vector2(x * tRes, y * tRes), uvScale);
            }
        }   

        int  index = 0;
        if (saveFormat == SaveFormat.Triangles)
        {
            // Build triangle indices: 3 indices into vertex array for each triangle
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    // For each grid cell output two triangles
                    tPolys[index++] = (y * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = (y * w) + x + 1;
    
                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x + 1;
                    tPolys[index++] = (y * w) + x + 1;
                }
            }
        }
        else
        {
            // Build quad indices: 4 indices into vertex array for each quad
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    // For each grid cell output one quad
                    tPolys[index++] = (y * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x + 1;
                    tPolys[index++] = (y * w) + x + 1;
                }
            }
        }


        // Export to .obj
        Debug.Log("Writing obj file to " + fileName);
        StreamWriter sw = new StreamWriter(fileName);
        try
        { 
            string mtlname = Path.ChangeExtension(fileName, "mtl");
            mtlname = Path.GetFileName(mtlname);
            sw.WriteLine("# Unity terrain OBJ File");
            sw.WriteLine("mtllib " + mtlname);
    
            // Write vertices
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            //counter  = 0;
            totalCount = (tVertices.Length * 2 + (saveFormat == SaveFormat.Triangles ? tPolys.Length / 3 : tPolys.Length / 4)) / progressUpdateInterval;
            for (int i = 0; i < tVertices.Length; i++)
            {
                StringBuilder sb = new StringBuilder("v ", 20);
                // StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}"etc. format
                // Which is important when you're exporting huge terrains.
                Vector3 vertex = tVertices[i] * scalefactor;
                sb.Append(vertex.x.ToString()).Append(" ").
                Append(vertex.y.ToString()).Append(" ").
                Append(vertex.z.ToString());
                sw.WriteLine(sb);
            }


            // Write UVs
            for (int i = 0; i < tUV.Length; i++)
            {
                StringBuilder sb = new StringBuilder("vt ", 22);
                sb.Append(tUV[i].x.ToString()).Append(" ").
                Append(tUV[i].y.ToString());
                sw.WriteLine(sb);
            }

            Dictionary<string, ObjMaterial> materialList = new Dictionary<string, ObjMaterial>();
            Material mat = terrainObject.materialTemplate;
            ObjMaterial objMaterial = new ObjMaterial();
            
            if (saveFormat == SaveFormat.Triangles)
            {
                Debug.Log("triangles");
                sw.WriteLine("\n");
                sw.WriteLine("usemtl " + mat.name);
                sw.WriteLine("usemap " + mat.name);
    
                try
                {    
                    objMaterial.name = mat.name;
    
                    if (mat.mainTexture) {
                        Texture texture = mat.mainTexture;
                        objMaterial.texture = (Texture2D)texture;
                        objMaterial.aoTexture = (Texture2D)mat.GetTexture("_AOTexture");
                        objMaterial.detailTexture = (Texture2D)mat.GetTexture("_OverlayTexture");
//                        objMaterial.normalMap = (Texture2D)mat.GetTexture("_NormalMap");
                    }
                    else 
                        objMaterial.texture = null;
                }
                catch (ArgumentException)
                {
                    //Already in the dictionary
                }

                // Write triangles
                for (int i = 0; i < tPolys.Length; i += 3)
                {
                    StringBuilder sb = new StringBuilder("f ", 43);
                    sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
                        Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
                        Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1);
                    sw.WriteLine(sb);
                }
            }
            else
            {
                // Write quads
                for (int i = 0; i < tPolys.Length; i += 4)
                {
                StringBuilder sb = new StringBuilder("f ", 57);
                sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
                    Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
                    Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1).Append(" ").
                    Append(tPolys[i + 3] + 1).Append("/").Append(tPolys[i + 3] + 1);
                sw.WriteLine(sb);
                }
            }

            MaterialsToFile(objMaterial, fileName, applyAO);
            Debug.Log("Finished Saving");
        }
        catch(Exception err)
        {
            Debug.Log("Error saving file: " + err.Message);
        }
        sw.Close();

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        terrain = null;
    } 
 
	private static void MaterialsToFile(ObjMaterial material, string filename, bool applyAO)
	{
        filename = Path.ChangeExtension(filename, "mtl");
        string folder = Path.GetDirectoryName (Path.GetFullPath(filename)) + "/";
        Debug.Log("Writing mtl file to " + filename);
		using (StreamWriter sw = new StreamWriter(filename)) 
		{
            //write the mtl file
            sw.Write("\n");
            sw.Write("newmtl {0}\n", material.name);
            sw.Write("Ka  1.0 1.0 1.0\n");
            sw.Write("Kd  1.0 1.0 1.0 \n");
            sw.Write("Ks  0.2 0.2 0.2\n");
            sw.Write("d  1.0\n");
            sw.Write("Ns  0.0\n");
            sw.Write("illum 2\n");
 
            if (material.texture != null)
            {
                //combine the base texture and overlay texture and save it to a png file
                string destinationFile = Path.ChangeExtension(filename, "png");
                Debug.Log("Writing texture to " + destinationFile);
 
                Texture2D source = material.texture;
                Texture2D readableTexture = MakeTextureCompressable(source, false);
                Color pixel;
                for (int xx = 0; xx < readableTexture.width; xx++)
                {
                    for (int yy = 0; yy < readableTexture.height; yy++)
                    {
                        //combine the base texture and the overlay
                        Color basePixel = source.GetPixel(xx, yy);
                        if(applyAO) {
                            basePixel *= material.aoTexture.GetPixel(xx,yy);
                        }
                        Color overlay = material.detailTexture.GetPixel(xx, yy);
                        float mask = overlay.a;

                            
                        pixel = (basePixel * (1- mask)) + (overlay * mask);
                        readableTexture.SetPixel(yy, xx, pixel);
                    }
                }        

                byte[] bytes;
                bytes = readableTexture.EncodeToPNG();

                System.IO.File.WriteAllBytes(destinationFile, bytes);   
                destinationFile = Path.GetFileName(destinationFile);
                sw.Write("map_Kd {0}", destinationFile);
        
                //write the normal map
/*              
                Doesn't work
                Debug.Log("Saving normal map to " + destinationFile);
                source = material.normalMap;
                //readableTexture = MakeTextureCompressable(source, true);
                Color32[] src = source.GetPixels32();
                int length = 4 * src.Length;
                byte[] srcBytes = new byte[length];
                IntPtr ptr = Marshal.AllocHGlobal(length);
                Marshal.StructureToPtr(src, ptr, true);
                Marshal.Copy(ptr, srcBytes, 0, length);
                Marshal.FreeHGlobal(ptr);

                //byte[] srcBytes = source.GetRawTextureData<byte>();
                //GraphicsFormat.RGBA_DXT5_UNorm
                Debug.Log((uint)source.width + " : " + (uint)source.height);
                bytes = ImageConversion.EncodeArrayToPNG(srcBytes, GraphicsFormat.R8G8B8A8_UNorm, (uint)source.width, (uint)source.height, (uint)(source.width * 4));

                destinationFile = Path.ChangeExtension(filename, null) + "_Normal.png";
                System.IO.File.WriteAllBytes(destinationFile, bytes);   
                destinationFile = Path.GetFileName(destinationFile);
                sw.Write("map_bump {0}", destinationFile);*/
            }
 
            sw.Write("\n\n\n");
        }
    }

    private static Texture2D MakeTextureCompressable(Texture2D source, bool isNormal)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;

        Texture2D readableTexture;
        if(isNormal)
            readableTexture = new Texture2D(source.width, source.height, TextureFormat.DXT5, true);
        else
            readableTexture = new Texture2D(source.width, source.height);

        readableTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0, false);
        readableTexture.Apply();                    

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);

        return readableTexture;
    }
}
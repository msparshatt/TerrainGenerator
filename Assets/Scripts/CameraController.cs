using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Experimental.Rendering;

public class CameraController : MonoBehaviour
{
    [Header("Movement settings")]
    public float Normal_Speed = 40.0f; //Normal movement speed
    public float Shift_Speed = 80.0f; //multiplies movement speed by how long shift is held down.
    public float Speed_Cap = 80.0f; //Max cap for speed when shift is held down
    public float Camera_Sensitivity = 0.6f; //How sensitive it with mouse
    private Vector3 camera_Rotation = new Vector3(0, 0, 0); //Mouse location on screen during play (Set to near the middle of the screen)    
    private float Total_Speed = 1.0f; //Total speed variable for shift

    [Header("Terrain settings")]
    [SerializeField] private BrushDataScriptable brushData;
    [SerializeField] private Terrain brushTerrain;
    [SerializeField] private Terrain mainTerrain;

    //private terrain settings
    private TerrainData mainTerrainData;
    private Vector3 mainTerrainSize;
    private int mainTerrainMapSize;

    [Header("UI References")]
    [SerializeField] private GameObject userInterface;
    private bool uiVisible = true;
    //sliders which can be controlled using the scroll wheel
    [SerializeField] private Slider radiusSlider;
    [SerializeField] private Slider strengthSlider;


    //store the operation which is currently being performed
    private Operation operation;

    void Start()
    {
        mainTerrainData = mainTerrain.terrainData;
        mainTerrainSize = mainTerrainData.size;
        mainTerrainMapSize = mainTerrainData.heightmapResolution;

        operation = null;
    }

    void Update()
    {
        //Camera angles based on mouse position while holding right mouse button
        if(Input.GetButton("Look")) {
            Cursor.lockState = CursorLockMode.Locked;
            camera_Rotation.x -= Input.GetAxis("Mouse Y");
            camera_Rotation.y += Input.GetAxis("Mouse X");
            transform.eulerAngles = camera_Rotation;
        } else {
            Cursor.lockState = CursorLockMode.None;
        }

        //Keyboard controls       
        Vector3 Cam = GetBaseInput();
        if (Input.GetButton("Modifier1"))
        {
            Total_Speed += Time.deltaTime;          
            Cam = Cam * Total_Speed * Shift_Speed;           
            Cam.x = Mathf.Clamp(Cam.x, -Speed_Cap, Speed_Cap);           
            Cam.y = Mathf.Clamp(Cam.y, -Speed_Cap, Speed_Cap);           
            Cam.z = Mathf.Clamp(Cam.z, -Speed_Cap, Speed_Cap);
        } else {                    
            Total_Speed = Mathf.Clamp(Total_Speed * 0.5f, 1f, 1000f);        
            Cam = Cam * Normal_Speed;
        }

        Cam = Cam * Time.deltaTime;
        transform.Translate(Cam);

        if(Input.GetButtonDown("Hide UI")) {
            uiVisible = !uiVisible;
            userInterface.SetActive(uiVisible);
        }

        float scrollWheel = Input.GetAxis("Mouse ScrollWheel"); //Input for horizontal movement        

        if(scrollWheel != 0.0f) {
            if(Input.GetButton("Modifier1"))
                strengthSlider.value += scrollWheel / 5;
            else
                radiusSlider.value += scrollWheel * 100;
        }

        //start recording undo information on mouse down
        if(Input.GetButtonDown("Interact")) {
            RaycastHit raycastTarget;

            //detect if the ray hits an object
            if(SendRaycastFromCameraToMousePointer(out raycastTarget)) {
                //access the hit object
                GameObject targetObject = raycastTarget.collider.gameObject;
                if (targetObject == mainTerrain.gameObject)
                {
                    operation = new Operation();
                }
            }
        }

        //undo/redo
        #if UNITY_EDITOR
            if(Input.GetKeyDown(KeyCode.Z))
        #else
            if((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(KeyCode.Z))
        #endif    
        {
            gameObject.GetComponent<OperationList>().UndoCommand();
        }        
        
        //use y for undo when running in editor, ctrl+y when standalone
        #if UNITY_EDITOR
            if(Input.GetKeyDown(KeyCode.Y))
        #else
            if((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(KeyCode.Y))
        #endif    
        {
            gameObject.GetComponent<OperationList>().RedoCommand();
        }        

        //don't allow interaction with the terrain if over the UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        //sculpt/paint on left mouse button       
        if(Input.GetButton("Interact") && operation != null) {
            //variable to hold the data for a raycast collision
            RaycastHit raycastTarget;

            //detect if the ray hits an object
            if(SendRaycastFromCameraToMousePointer(out raycastTarget)) {
                //access the hit object
                GameObject targetObject = raycastTarget.collider.gameObject;
                if (targetObject != mainTerrain.gameObject)
                {
                    return;
                }
            
                if(brushData.brushMode == BrushDataScriptable.Modes.Sculpt) {
                    TerrainSculpter.SculptMode mode = TerrainSculpter.SculptMode.Raise;
                    if(Input.GetButton("Modifier1")) {
                        mode = TerrainSculpter.SculptMode.Lower;
                    } else if(Input.GetButton("Modifier2")) {
                        mode = TerrainSculpter.SculptMode.Flatten;
                    }

                    mainTerrain.GetComponent<TerrainSculpter>().SculptTerrain(mode, raycastTarget.point, operation);
                } else {
                    TerrainPainter painter = mainTerrain.GetComponent<TerrainPainter>();
                    TerrainPainter.PaintMode mode = TerrainPainter.PaintMode.Paint;
                    if(Input.GetButton("Modifier1")) {
                        mode = TerrainPainter.PaintMode.Erase;
                    }
                    Texture2D overlayTexture = (Texture2D)mainTerrain.materialTemplate.GetTexture("_OverlayTexture");

                    painter.PaintTerrain(mode, overlayTexture, raycastTarget.point, operation);
                }
            }
        }

        //store undo information to the undo list on mouse up
        if(Input.GetButtonUp("Interact")) {
            if(operation != null) {
                gameObject.GetComponent<OperationList>().AddOperation(operation);
                operation = null;
            }
        }

        //project an image of the brush onto the terrain
        projectImage();
    }

    private Vector3 GetBaseInput()
    {   
        Vector3 Camera_Velocity = new Vector3();        
        float leftRight = Input.GetAxis("Left/Right"); //Input for horizontal movement        
        float forwardBack = Input.GetAxis("Forward/Back"); //Input for Vertical movement        
        float upDown = Input.GetAxis("Up/Down"); //Input for Vertical movement        
        Camera_Velocity += new Vector3(leftRight, upDown, forwardBack);
        //Camera_Velocity += new Vector3(0, 0, VerticalInput);       
        return Camera_Velocity;
    }

    private void projectImage()
    {
        //display an overlay where the terrain will be sculpted/painted
        RaycastHit raycastTarget;

        if(SendRaycastFromCameraToMousePointer(out raycastTarget)) {
            if(raycastTarget.collider.gameObject != mainTerrain.gameObject) {
                brushTerrain.enabled = false;            
                return;
            }

            Vector3 location = raycastTarget.point;

            float scaleFactor = 1.0f;
            if(brushData.brushMode == BrushDataScriptable.Modes.Paint) {
                scaleFactor = brushData.textureScale;
            }

            float radius = brushData.brushRadius / scaleFactor;
            int offset = (int)(radius /2);

            float posX = ((location.x - mainTerrain.transform.position.x) / scaleFactor) + mainTerrain.transform.position.x - offset;
            float posZ = ((location.z - mainTerrain.transform.position.z) / scaleFactor) + mainTerrain.transform.position.z - offset;
            brushTerrain.transform.position = new Vector3(posX, location.y, posZ);
            brushTerrain.terrainData.size = new Vector3(radius, 10, radius);
            brushTerrain.materialTemplate.mainTexture = brushData.brush;

            float[,] heights = mainTerrain.GetComponent<TerrainSculpter>().ReadTerrainHeights(location);

            for (int xx = 0; xx < heights.GetLength(1); xx++)
            {
                for (int yy = 0; yy < heights.GetLength(0); yy++)
                {                   
                    heights[yy, xx] += 0.1f; //0.2f;
                }
            }
            
            brushTerrain.terrainData.SetHeights(0,0, heights);
            brushTerrain.enabled = true;
        } else {
            brushTerrain.enabled = false;            
        }
    }

    //utility functions
    //send a raycast out towards the mousepointer
    private bool SendRaycastFromCameraToMousePointer(out RaycastHit raycastTarget) 
    {
        Vector3 target = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        Ray ray = Camera.main.ScreenPointToRay(target); 

        return Physics.Raycast(ray, out raycastTarget, Mathf.Infinity);
    }
}
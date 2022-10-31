using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;
using SimpleFileBrowser;

public class PositionAndRotation
{
    public Vector3 position;
    public Quaternion rotation;
}

public class CameraController : MonoBehaviour
{
    [Header("Terrain settings")]
    [SerializeField] private BrushDataScriptable sculptBrushData;
    [SerializeField] private PaintBrushDataScriptable paintBrushData;
    [SerializeField] private BrushDataScriptable erosionBrushData;
    [SerializeField] private BrushDataScriptable stampBrushData;

    [Header("Settings")]
    [SerializeField] private SettingsDataScriptable settingsData;
    [SerializeField] private InternalDataScriptable internalData;


    //private terrain settings
    private TerrainManager manager;
    private Terrain currentTerrain;
    private TerrainData currentTerrainData;
    private Vector3 currentTerrainSize;
    private int currentTerrainMapSize;

    [Header("UI References")]
    [SerializeField] private GameObject userInterface;
    [SerializeField] private GameObject[] panels;
    [SerializeField] private MainBar mainBar;
    private bool uiVisible = true;

    //sliders which can be controlled using the scroll wheel
    [Header("Sculpt Sliders")]
    [SerializeField] private Slider sculptRadiusSlider;
    [SerializeField] private Slider sculptStrengthSlider;
    [SerializeField] private Slider sculptRotationSlider;

    [SerializeField] private Slider sculptHeightSlider;

    [Header("Paint Sliders")]
    [SerializeField] private Slider paintRadiusSlider;
    [SerializeField] private Slider paintStrengthSlider;
    [SerializeField] private Slider paintRotationSlider;

    [Header("Stamp Sliders")]
    [SerializeField] private Slider stampRadiusSlider;
    [SerializeField] private Slider stampRotationSlider;
    [SerializeField] private Slider stampHeightSlider;

    [Header("Erosion Sliders")]
    [SerializeField] private Slider erosionRadiusSlider;
    [SerializeField] private Slider erosionRotationSlider;
    [SerializeField] private Slider erosionStrengthSlider;

    [Header("Other")]
    [SerializeField] private Texture2D busyCursor;

    //store the operation which is currently being performed
    private Operation operation;

    //variables used by new input system
    Vector3 moveValue;
    Vector2 rotation;

    bool modifier1;
    bool modifier2;

    //true when the mouseDown button is held down
    private bool mouseDown;

    //true after applying a stamp to avoid applying the stamp multiple times on a single mouse click
    private bool stampApplied;
    MaterialController materialController;

    //store the position of each camera
    const int NUMBER_OF_CAMERAS = 4;
    private PositionAndRotation[] cameras;
    private int cameraNumber;


    void Start()
    {
        manager = TerrainManager.Instance();
        currentTerrain = manager.Terrain;
        materialController = manager.MaterialController;
        currentTerrainData = manager.TerrainData;
        currentTerrainSize = currentTerrainData.size;
        currentTerrainMapSize = currentTerrainData.heightmapResolution;

        operation = null;

        moveValue = new Vector3();
        rotation = new Vector2();

        Cursor.lockState = CursorLockMode.None;
        modifier1 = false;
        modifier2 = false;

        internalData.sliderChanged = false;
        internalData.unsavedChanges = false;
        stampApplied = false;

        cameraNumber = 0;
        cameras = new PositionAndRotation[NUMBER_OF_CAMERAS];
        for(int index = 0; index < NUMBER_OF_CAMERAS; index++) {
            cameras[index] = new PositionAndRotation();
            cameras[index].position = transform.position;
            cameras[index].rotation = transform.rotation;
        }
        //currentTerrain.GetComponent<Ceto.AddAutoShoreMask>().CreateShoreMasks();
    }

    //Callback functions for new input system
    void OnLeftRight(InputValue input)
    {
        float movement = input.Get<float>();
        moveValue.x = movement;
    }

    void OnUpDown(InputValue input)
    {
        float movement = input.Get<float>();
        moveValue.y = movement; 
    }

    void OnForwardBack(InputValue input)
    {
        float movement = input.Get<float>();
        moveValue.z = movement; 
    }

    void OnLook(InputValue input)
    {
        rotation = (Cursor.lockState == CursorLockMode.Locked) ? input.Get<Vector2>().normalized * settingsData.cameraSensitivity: Vector2.zero;
    }

    public void OnLookToggle(InputValue input)
    {
        Cursor.lockState = input.isPressed ? CursorLockMode.Locked : CursorLockMode.None;
    }

    public void OnModifier1(InputValue input)
    {
        modifier1 = input.isPressed;
    }

    public void OnModifier2(InputValue input)
    {
        modifier2 = input.isPressed;
    }

    public void OnHideUI(InputValue input)
    {
        uiVisible = !uiVisible;
        userInterface.SetActive(uiVisible);
    }

    public void OnMouseWheel(InputValue input)
    {
        if(!EventSystem.current.IsPointerOverGameObject()) {
            float value = input.Get<float>();

            if (Application.platform == RuntimePlatform.WindowsPlayer)
                value *= -1;

            if(value > 0) {
                value = 1;
            } else if (value < 0) {
                value = -1;
            }

            Slider strengthSlider;
            Slider rotationSlider;
            Slider radiusSlider;

            if(internalData.mode == InternalDataScriptable.Modes.Sculpt) {
                strengthSlider = sculptStrengthSlider;
                rotationSlider = sculptRotationSlider;
                radiusSlider = sculptRadiusSlider;
            } else if(internalData.mode == InternalDataScriptable.Modes.Paint) {
                strengthSlider = paintStrengthSlider;
                rotationSlider = paintRotationSlider;
                radiusSlider = paintRadiusSlider;
            } else if(internalData.mode == InternalDataScriptable.Modes.Stamp) {
                strengthSlider = stampHeightSlider;
                rotationSlider = stampRotationSlider;
                radiusSlider = stampRadiusSlider;
            }  else if(internalData.mode == InternalDataScriptable.Modes.Erode) {
                strengthSlider = erosionStrengthSlider;
                rotationSlider = erosionRotationSlider;
                radiusSlider = erosionRadiusSlider;
            } else {
                return;
            }

            if(modifier1 && strengthSlider != null)
                strengthSlider.value -= value / 50;
            else if(modifier2)
                rotationSlider.value -= value * 5;
            else
                radiusSlider.value -= value * 50;
        }
    }

    public void OnInteract(InputValue input)
    {
        mouseDown = input.isPressed;

        if(!mouseDown && internalData.sliderChanged) {
            materialController.ApplyTextures();
            internalData.sliderChanged = false;
        }
    }

    public void OnUndo(InputValue input)
    {
        if(modifier2) {
            Cursor.SetCursor(busyCursor, Vector2.zero, CursorMode.Auto); 

            //force the cursor to update
            Cursor.visible = false;
            Cursor.visible = true;

            gameObject.GetComponent<OperationList>().UndoCommand();

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); 
        }
    }

    public void OnRedo(InputValue input)
    {
        if(modifier2) {
            Cursor.SetCursor(busyCursor, Vector2.zero, CursorMode.Auto); 

            //force the cursor to update
            Cursor.visible = false;
            Cursor.visible = true;

            gameObject.GetComponent<OperationList>().RedoCommand();
        
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); 
        }
    }


    public void OnScreenshot()
    {
        DateTime now = DateTime.Now;
        string filename = Application.persistentDataPath + "/" + now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
        ScreenCapture.CaptureScreenshot(filename, settingsData.resolutionMultiplier);
        Debug.Log(filename);

        AudioSource audio = GetComponent<AudioSource>();
        audio.Play();
    }

    void Update()
    {
        transform.Rotate(new Vector3(-rotation.y, 0f, 0f), Space.Self); 
        transform.Rotate(new Vector3(0f, rotation.x, 0f), Space.World); 

        Vector3 movement = settingsData.movementSpeed * moveValue * Time.deltaTime;

        if(modifier1)
            movement *= 10;

        transform.position += transform.TransformDirection(movement);

        //save the new position
        cameras[cameraNumber].position = transform.position;
        cameras[cameraNumber].rotation = transform.rotation;

        if(internalData.ProcGenOpen) {
            for(int i = 0; i < panels.Length; i++) {
                panels[i].SetActive(false);
            }
            return;
        } else {
            panels[0].SetActive(true);
        }

        //don't allow mouseDownion with the terrain if the mouse is over the UI
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        
        if(internalData.mode == InternalDataScriptable.Modes.Sculpt || internalData.mode == InternalDataScriptable.Modes.Paint || internalData.mode == InternalDataScriptable.Modes.Stamp || internalData.mode == InternalDataScriptable.Modes.Erode) {
            projectImage(); 
        } else {
            currentTerrain.materialTemplate.SetVector("_CursorLocation", new Vector4(0f, 0f, 0f, 0f));            
        }

        //sculpt/paint on left mouse button       
        if(mouseDown) {
            //variable to hold the data for a raycast collision
            RaycastHit raycastTarget;

            //detect if the ray hits an object
            if(SendRaycastFromCameraToMousePointer(out raycastTarget)) {
                //access the hit object
                GameObject targetObject = raycastTarget.collider.gameObject;
                if (targetObject != currentTerrain.gameObject)
                {
                    return;
                }
            
                //start recording undo information on mouse down
                if(operation == null)
                    operation = new Operation();

                if(internalData.mode == InternalDataScriptable.Modes.Sculpt) {
                    if(sculptBrushData.mode == 0) {
                        TerrainSculpter.SculptMode mode = TerrainSculpter.SculptMode.Raise;
                        if(modifier1) {
                            mode = TerrainSculpter.SculptMode.Lower;
                        } else if(modifier2) {
                            mode = TerrainSculpter.SculptMode.Flatten;
                        }
                        manager.TerrainSculpter.SculptTerrain(mode, raycastTarget.point, operation);
                    } else {
                        if(modifier1) {
                            float height = manager.TerrainSculpter.GetHeightAtPoint(raycastTarget.point);
                            Debug.Log(height);
                            sculptHeightSlider.value = height/ 1000.0f;
                        } else {
                            manager.TerrainSculpter.SetHeight(raycastTarget.point, operation);
                        }
                    }

                } else if(internalData.mode == InternalDataScriptable.Modes.Paint) {
                    TerrainPainter painter = manager.TerrainPainter;
                    TerrainPainter.PaintMode mode = TerrainPainter.PaintMode.Paint;
                    if(modifier1) {
                        mode = TerrainPainter.PaintMode.Erase;
                    }
                    Texture2D overlayTexture = (Texture2D)currentTerrain.materialTemplate.GetTexture("_OverlayTexture");

                    painter.PaintTerrain(mode, overlayTexture, raycastTarget.point, operation);
                } else if(internalData.mode == InternalDataScriptable.Modes.Stamp && !stampApplied) {
                    TerrainStamper stamper = manager.TerrainStamper;
                    TerrainStamper.StampMode mode = TerrainStamper.StampMode.Raise;

                    if(modifier1) {
                        mode = TerrainStamper.StampMode.Lower;
                    }
                    stamper.ModifyTerrain(mode, raycastTarget.point, operation);
                    stampApplied = true;
                } else if(internalData.mode == InternalDataScriptable.Modes.Erode){
                    TerrainEroder eroder = manager.TerrainEroder;

                    eroder.ErodeTerrain(raycastTarget.point, operation);
                }
            }
        }

        //store undo information to the undo list on mouse up
        if(!mouseDown && operation != null) {
            gameObject.GetComponent<OperationList>().AddOperation(operation);
            operation = null;
            stampApplied = false;

            if(internalData.mode == InternalDataScriptable.Modes.Sculpt || internalData.mode == InternalDataScriptable.Modes.Stamp || internalData.mode == InternalDataScriptable.Modes.Erode) {
                materialController.ApplyTextures();

                if(internalData.oceanActive)
                    currentTerrain.GetComponent<Ceto.AddAutoShoreMask>().CreateShoreMasks();
            } else if(internalData.mode == InternalDataScriptable.Modes.Paint) {
                materialController.ApplyMask();
            }

        }
    }

    private void projectImage()
    {
        //display an overlay where the terrain will be sculpted/painted
        RaycastHit raycastTarget;

        if(SendRaycastFromCameraToMousePointer(out raycastTarget)) {
            if(raycastTarget.collider.gameObject != currentTerrain.gameObject) {
                currentTerrain.materialTemplate.SetVector("_CursorLocation", new Vector4(0f, 0f, 0f, 0f));            
                return;
            }

            float radius = 0;
            float rotation = 0;
            Texture2D shape = null;
            if(internalData.mode == InternalDataScriptable.Modes.Sculpt) {
                radius = sculptBrushData.brushRadius / (currentTerrain.terrainData.size.x);
                rotation = sculptBrushData.brushRotation;
                shape = sculptBrushData.brush;
            } else if (internalData.mode == InternalDataScriptable.Modes.Paint) {
                radius = paintBrushData.brushRadius / (currentTerrain.terrainData.size.x);
                rotation = paintBrushData.brushRotation;
                shape = paintBrushData.brush;
            } else if (internalData.mode == InternalDataScriptable.Modes.Stamp) {
                radius = stampBrushData.brushRadius / (currentTerrain.terrainData.size.x);
                rotation = stampBrushData.brushRotation;
                shape = stampBrushData.brush;
            } else if (internalData.mode == InternalDataScriptable.Modes.Erode) {
                radius = erosionBrushData.brushRadius / (currentTerrain.terrainData.size.x);
                rotation = erosionBrushData.brushRotation;
                shape = erosionBrushData.brush;
            }

            Vector3 location = raycastTarget.point;
            float posX = ((location.x - currentTerrain.transform.position.x) / currentTerrain.terrainData.size.x);
            float posZ = ((location.z - currentTerrain.transform.position.z) / currentTerrain.terrainData.size.z);

//            float cusorHeight = currentTerrain.terrainData.GetHeight((int)(posX * currentTerrainMapSize), (int)(posZ * currentTerrainMapSize));
//            Debug.Log(posX + ":" + posZ + ":" + cusorHeight);

            posX -=  (radius / 2);
            posZ -=  (radius / 2);

            //Debug.Log(posX + ":" + posZ);
            currentTerrain.materialTemplate.SetVector("_CursorLocation", new Vector4(posX, posZ, radius, radius));
            currentTerrain.materialTemplate.SetTexture("_CursorTexture", shape);
            currentTerrain.materialTemplate.SetFloat("_CursorRotation", -rotation);
        } else {
            currentTerrain.materialTemplate.SetVector("_CursorLocation", new Vector4(0f, 0f, 0f, 0f));            
        }
    }

    //utility functions
    //send a raycast out towards the mousepointer
    private bool SendRaycastFromCameraToMousePointer(out RaycastHit raycastTarget) 
    {
        Ray ray = Camera.main.GetComponent<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());

        return Physics.Raycast(ray, out raycastTarget, Mathf.Infinity);
    }

    public bool IsMouseButtonDown()
    {
        Debug.Log(mouseDown);
        return mouseDown;
    }

    public void SwitchCamera(int camera)
    {
        if(cameras != null) {
            cameraNumber = camera;
            transform.position = cameras[camera].position;
            transform.rotation = cameras[camera].rotation;
        }
    }

    public string ToJson()
    {
        CameraSaveData_v1 data = new CameraSaveData_v1();
        data.cameraPositions = new List<string>();
        for(int index = 0; index < NUMBER_OF_CAMERAS; index++) {
            string posString = JsonUtility.ToJson(new PositionAndRotationSaveData(cameras[index].position, cameras[index].rotation));
            data.cameraPositions.Add(posString);
        }

        data.cameraIndex = cameraNumber;
        return JsonUtility.ToJson(data);
    }

    public void FromJson(string json)
    {
        CameraSaveData_v1 data = JsonUtility.FromJson<CameraSaveData_v1>(json);

        for(int index = 0; index < data.cameraPositions.Count; index++) {
            PositionAndRotationSaveData posData = JsonUtility.FromJson<PositionAndRotationSaveData>(data.cameraPositions[index]);

            cameras[index].position = posData.position;
            cameras[index].rotation = posData.rotation;
        }

        mainBar.SwitchCamera(data.cameraIndex);
    }
}
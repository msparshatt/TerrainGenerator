using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;
using SimpleFileBrowser;

public class CameraController : MonoBehaviour
{
    [Header("Terrain settings")]
    [SerializeField] private BrushDataScriptable brushData;
    [SerializeField] private Terrain mainTerrain;

    [Header("Settings")]
    [SerializeField] private SettingsDataScriptable settingsData;

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

    //variables used by new input system
    Vector3 moveValue;
    Vector2 rotation;

    bool modifier1;
    bool modifier2;

    //true when the interact button is held down
    bool interact;

    void Start()
    {
        mainTerrainData = mainTerrain.terrainData;
        mainTerrainSize = mainTerrainData.size;
        mainTerrainMapSize = mainTerrainData.heightmapResolution;

        operation = null;

        moveValue = new Vector3();
        rotation = new Vector2();

        Cursor.lockState = CursorLockMode.None;
        modifier1 = false;
        modifier2 = false;
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

            if(modifier1)
                strengthSlider.value -= value / 50;
            else
                radiusSlider.value -= value * 50;
        }
    }

    public void OnInteract(InputValue input)
    {
        interact = input.isPressed;
    }

    public void OnUndo(InputValue input)
    {
        if(modifier2)
            gameObject.GetComponent<OperationList>().UndoCommand();
    }

    public void OnRedo(InputValue input)
    {
        if(modifier2)
            gameObject.GetComponent<OperationList>().RedoCommand();
    }


    public void OnScreenshot()
    {
        DateTime now = DateTime.Now;
        string filename = Application.persistentDataPath + "/" + now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
        ScreenCapture.CaptureScreenshot(filename);
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

        //don't allow interaction with the terrain if the mouse is over the UI
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        projectImage(); 

        //sculpt/paint on left mouse button       
        if(interact) {
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
            
                //start recording undo information on mouse down
                if(operation == null)
                    operation = new Operation();

                if(brushData.brushMode == BrushDataScriptable.Modes.Sculpt) {
                    TerrainSculpter.SculptMode mode = TerrainSculpter.SculptMode.Raise;
                    if(modifier1) {
                        mode = TerrainSculpter.SculptMode.Lower;
                    } else if(modifier2) {
                        mode = TerrainSculpter.SculptMode.Flatten;
                    }

                    mainTerrain.GetComponent<TerrainSculpter>().SculptTerrain(mode, raycastTarget.point, operation);
                } else {
                    TerrainPainter painter = mainTerrain.GetComponent<TerrainPainter>();
                    TerrainPainter.PaintMode mode = TerrainPainter.PaintMode.Paint;
                    if(modifier1) {
                        mode = TerrainPainter.PaintMode.Erase;
                    }
                    Texture2D overlayTexture = (Texture2D)mainTerrain.materialTemplate.GetTexture("_OverlayTexture");

                    painter.PaintTerrain(mode, overlayTexture, raycastTarget.point, operation);
                }
            }
        }

        //store undo information to the undo list on mouse up
        if(!interact && operation != null) {
            gameObject.GetComponent<OperationList>().AddOperation(operation);
            operation = null;
        }
    }

    private void projectImage()
    {
        //display an overlay where the terrain will be sculpted/painted
        RaycastHit raycastTarget;

        if(SendRaycastFromCameraToMousePointer(out raycastTarget)) {
            if(raycastTarget.collider.gameObject != mainTerrain.gameObject) {
                mainTerrain.materialTemplate.SetVector("_CursorLocation", new Vector4(0f, 0f, 0f, 0f));            
                return;
            }

            float radius = brushData.brushRadius / (mainTerrain.terrainData.size.x);

            Vector3 location = raycastTarget.point;
            float posX = ((location.x - mainTerrain.transform.position.x) / mainTerrain.terrainData.size.x);
            float posZ = ((location.z - mainTerrain.transform.position.z) / mainTerrain.terrainData.size.z);

//            float cusorHeight = mainTerrain.terrainData.GetHeight((int)(posX * mainTerrainMapSize), (int)(posZ * mainTerrainMapSize));
//            Debug.Log(posX + ":" + posZ + ":" + cusorHeight);

            posX -=  (radius / 2);
            posZ -=  (radius / 2);

            //Debug.Log(posX + ":" + posZ);
            mainTerrain.materialTemplate.SetVector("_CursorLocation", new Vector4(posX, posZ, radius, radius));
            mainTerrain.materialTemplate.SetTexture("_CursorTexture", brushData.brush);

        } else {
            mainTerrain.materialTemplate.SetVector("_CursorLocation", new Vector4(0f, 0f, 0f, 0f));            
        }
    }

    //utility functions
    //send a raycast out towards the mousepointer
    private bool SendRaycastFromCameraToMousePointer(out RaycastHit raycastTarget) 
    {
        Ray ray = Camera.main.GetComponent<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());

        return Physics.Raycast(ray, out raycastTarget, Mathf.Infinity);
    }
}
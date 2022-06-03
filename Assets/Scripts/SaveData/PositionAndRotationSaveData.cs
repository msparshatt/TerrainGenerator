using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionAndRotationSaveData 
{
    public Vector3 position;
    //public Vector3 eulerRotation;
    public Quaternion rotation;

    public PositionAndRotationSaveData(Vector3 _position, Quaternion _rotation)
    {
        position = _position;
        //eulerRotation = _rotation.eulerAngles;
        rotation = _rotation;
    }
}

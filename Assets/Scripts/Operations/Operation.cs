using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SubOperation
{
    public abstract void Do();
    public abstract void Undo();
}

public class Operation 
{
    private List<SubOperation> subOperations;
    public bool operationOkay;

    public Operation()
    {
        subOperations = new List<SubOperation>();
        operationOkay = true;
    }

    public virtual void Do()
    {
        if(subOperations.Count > 0) {
            foreach(SubOperation subOperation in subOperations) {
                subOperation.Do();
            }

            if(subOperations[0] is SculptSubOperation) {
                TerrainManager.instance.ApplyTextures();
            }
        }
    }


    public virtual void Undo()
    {
        if(subOperations.Count > 0) {
            foreach(SubOperation subOperation in subOperations) {
                subOperation.Undo();
            }
            if(subOperations[0] is SculptSubOperation) {
                TerrainManager.instance.ApplyTextures();
            }
        }
    }

    public virtual void AddSubOperation(SubOperation subOperation)
    {
        subOperations.Add(subOperation);
    }
}

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
        foreach(SubOperation subOperation in subOperations) {
            subOperation.Do();
        }
    }


    public virtual void Undo()
    {
        foreach(SubOperation subOperation in subOperations) {
            subOperation.Undo();
        }
    }

    public virtual void AddSubOperation(SubOperation subOperation)
    {
        subOperations.Add(subOperation);
    }
}

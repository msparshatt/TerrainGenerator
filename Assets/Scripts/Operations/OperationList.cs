using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OperationList : MonoBehaviour
{
    private List<Operation> operationList;
    private int executionIndex;
    [SerializeField] private InternalDataScriptable internalData;
    [SerializeField] private SettingsDataScriptable settingsData;

    // Start is called before the first frame update
    void Start()
    {
        operationList = new List<Operation>();
    }

   //adds a command to the history
    public void AddOperation(Operation operation)
    {
        if(operation.operationOkay) {
            //clear out redo buffer
            for (int i = operationList.Count - 1; i >= executionIndex; i--)
            {
                operationList.RemoveAt(i);
            }
        
            //add new command to history
            operationList.Add(operation);
            executionIndex++;

            if(operationList.Count > settingsData.undoCount) {
                operationList.RemoveAt(0);
                executionIndex--;
            }
            internalData.unsavedChanges = true;
        }
    }
    
    //undoes the command at the previous point in the command history
    public void UndoCommand()
    {
        if (executionIndex > 0)
        {
            executionIndex--;
            operationList[executionIndex].Undo();

            if(executionIndex == 0)
                internalData.unsavedChanges = false;
        }        
    }
    //executes the command at the current point in the command history
    public void RedoCommand()
    {
        if (executionIndex < operationList.Count)
        {
            operationList[executionIndex].Do();
            executionIndex++;
            internalData.unsavedChanges = true;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SculptSubOperation : SubOperation
{
    private Vector2Int topLeft;
    private Vector2Int size;
    private float[,] changes;

    private Terrain terrain;

    public SculptSubOperation(Terrain _terrain, Vector2Int _topLeft, Vector2Int _size, float[,] _changes)
    {
        terrain = _terrain;
        topLeft = _topLeft;
        size = _size;
        changes = _changes;
    }

    public override void Do()
    {
        terrain.GetComponent<TerrainModifier>().RedoSculpt(topLeft, size, changes);
    }

    public override void Undo()
    {
        terrain.GetComponent<TerrainModifier>().UndoSculpt(topLeft, size, changes);
    }
}

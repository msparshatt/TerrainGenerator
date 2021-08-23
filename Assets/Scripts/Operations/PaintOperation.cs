using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintSubOperation : SubOperation
{
    private Vector2Int topLeft;
    private Vector2Int size;
    private Color[] changes;

    private Terrain terrain;
    private Texture2D texture;

    public PaintSubOperation(Terrain _terrain, Texture2D _texture, Vector2Int _topLeft, Vector2Int _size, Color[] _changes)
    {
        terrain = _terrain;
        texture = _texture;
        topLeft = _topLeft;
        size = _size;
        changes = _changes;
    }

    public override void Do()
    {
        terrain.GetComponent<TerrainPainter>().RedoPaint(texture, topLeft, size, changes);
    }

    public override void Undo()
    {
        terrain.GetComponent<TerrainPainter>().UndoPaint(texture, topLeft, size, changes);
    }
}

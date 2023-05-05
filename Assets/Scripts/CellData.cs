using UnityEngine;

public class CellData
{
    public Vector2Int Position { get; }
    public bool IsAlive { get; set; }

    public CellData(Vector2Int position)
    {
        Position = position;
        IsAlive = false;
    }
}
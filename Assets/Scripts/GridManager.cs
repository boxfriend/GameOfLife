using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    private Dictionary<Vector2Int, CellData> _cellsToDraw, _previousRoundCells;

    [Header("Generate Info")]
    [SerializeField, Range(0,120)] private float _secondsPerRound;
    [SerializeField, Range(0,1)] private float _startingAliveMultiplier = 0.1f;
    private int _startingAlive = 0;

    private float _currentTime;

    [Header("Tilemap Info")]
    [SerializeField] private Tilemap _cellMap;
    [SerializeField, Tooltip("Retrieved at edit time using the TileGetter component")] 
    private Vector2Int _bottomLeftCell, _topRightCell;

    private bool _roundStarted = false;

    public int CellsAlive { get; private set; }
    public int CellsDead { get; private set; }
    public int RoundCount { get; private set; }

    public delegate void RoundData(int cellsAlive, int cellsDead, int roundCount);
    public event RoundData OnRoundComplete;

    private void Awake ()
    {
        _cellsToDraw = new();
        _previousRoundCells = new();

        for(var x = _bottomLeftCell.x; x <= _topRightCell.x; x++)
        {
            for(var y = _bottomLeftCell.y; y <= _topRightCell.y; y++)
            {
                var position = new Vector2Int(x, y);
                _cellsToDraw.Add(position, new CellData(position));
                _previousRoundCells.Add(position, new CellData(position));
            }
        }

        _startingAlive = Mathf.FloorToInt(_cellsToDraw.Count * _startingAliveMultiplier);
    }

    public void NewRound()
    {
        var values = _cellsToDraw.Values;

        ResetAliveStatus(values);
        GenerateRandomAliveCells(values, _startingAlive);

        DrawCells();
        _currentTime = 0;
        _roundStarted = true;

        RoundCount = 0;
        OnRoundComplete?.Invoke(CellsAlive,CellsDead, RoundCount);
    }

    private static void ResetAliveStatus(IEnumerable<CellData> cells)
    {
        foreach (var cell in cells)
            cell.IsAlive = false;
    }

    private static void GenerateRandomAliveCells(IEnumerable<CellData> cells, int maxAlive)
    {
        var alive = 0;
        while (alive < maxAlive)
        {
            foreach (var cell in cells)
            {
                if (!cell.IsAlive && Random.value > 0.95f)
                {
                    cell.IsAlive = true;
                    alive++;
                }

                if (alive >= maxAlive)
                    break;
            }
        }
    }

    private void ApplyRules()
    {
        (_cellsToDraw, _previousRoundCells) = (_previousRoundCells, _cellsToDraw);

        foreach (var cell in _previousRoundCells.Values)
        {
            var neighbors = CountAliveNeighbors(cell, _previousRoundCells);
            var changingCell = _cellsToDraw[cell.Position];
            changingCell.IsAlive = (cell.IsAlive, neighbors) switch
            {
                (true, <= 1) => false, //Any live cell with 0 or 1 live neighbors becomes dead, because of underpopulation
                (true, >= 2 and <= 3) => true, //Any live cell with 2 or 3 live neighbors stays alive, because its neighborhood is just right
                (true, > 3) => false, //Any live cell with more than 3 live neighbors becomes dead, because of overpopulation
                (false, 3) => true, //Any dead cell with exactly 3 live neighbors becomes alive, by reproduction
                (_,_) => false,
            };
        }
    }

    private int CountAliveNeighbors(CellData cell, Dictionary<Vector2Int, CellData> toCheck)
    {
        var count = 0;
        for(var x = -1; x <= 1; x++)
        {
            for(var y = -1; y <= 1; y++)
            {
                var dir = new Vector2Int(x, y);
                if(dir == Vector2Int.zero) continue;

                var neighbor = dir + cell.Position;
                if(toCheck.TryGetValue(neighbor, out var data))
                {
                    if(data.IsAlive)
                        count++;
                }
            }
        }
        return count;
    }

    private void DrawCells()
    {
        var alive = 0;
        foreach(var cell in _cellsToDraw.Values)
        {
            var position = (Vector3Int)cell.Position;
            SetCellData(position, cell.IsAlive);

            if (cell.IsAlive)
                alive++;
        }

        CellsAlive = alive;
        CellsDead = _cellsToDraw.Count - alive;

        _cellMap.RefreshAllTiles();
    }

    private void SetCellData(Vector3Int position, bool alive)
    {
        var color = alive ? Color.green : Color.white;
        _cellMap.SetTileFlags(position, TileFlags.None);
        _cellMap.SetColor(position, color);
    }

    public void StopRound()
    {
        _roundStarted = false;
    }

    private void Update()
    {
        if (!_roundStarted)
            return;

        _currentTime += Time.deltaTime;

        if( _currentTime > _secondsPerRound )
        {
            ApplyRules();
            DrawCells();
            RoundCount++;
            _currentTime -= _secondsPerRound;
            OnRoundComplete(CellsAlive, CellsDead, RoundCount);
        }
    }

    public bool CellInRange(Vector2Int position)
    {
        var min = _bottomLeftCell;
        var max = _topRightCell;
        return (position.x >= min.x && position.x <= max.x)
            && (position.y >= min.y  && position.y <= max.y);
    }
}

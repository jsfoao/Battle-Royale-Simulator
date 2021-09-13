using System;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    private Transform _transform;
    [Header("Map Spawn")] 
    [SerializeField] 
    public Vector2Int size = new Vector2Int(10, 10);
    [SerializeField] 
    public float offset = 1f;
    [SerializeField] 
    private GameObject _tilePrefab;

    [NonSerialized] public Tile[,] _tileGrid;
    [NonSerialized] public Vector3 worldSize;
    
    private void TileNeighbours(Tile tile)
    {
        for (int xx = tile.gridPosition.x - 1; xx <= tile.gridPosition.x + 1; xx++)
        {
            for (int yy = tile.gridPosition.y - 1; yy <= tile.gridPosition.y + 1; yy++)
            {
                Vector2Int currentPos = new Vector2Int(xx, yy);
                
                if (currentPos == tile.gridPosition)
                    continue;
                if (currentPos.x >= size.x || currentPos.x < 0)
                    continue;
                if (currentPos.y >= size.y || currentPos.y < 0)
                    continue;
                
                // Add to neighbour tiles list
                tile.neighbourTiles.Add(_tileGrid[currentPos.x, currentPos.y]);
            }
        }
    }

    private void FindAllNeighbours()
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                TileNeighbours(_tileGrid[x, y]);
            }
        }
    }

    private void SpawnTiles()
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2 worldPos = new Vector3(x, y) * offset;
                _tileGrid[x, y] = Instantiate(_tilePrefab, worldPos, Quaternion.identity).GetComponent<Tile>();
                _tileGrid[x, y].gridPosition = new Vector2Int(x, y);
                _tileGrid[x, y].worldPosition = _tileGrid[x, y].transform.position;
                
                // Set Tiles as children of Map
                _tileGrid[x, y].transform.SetParent(_transform);
                _tileGrid[x, y].name = $"Tile({x}, {y})";
            }
        }
    }

    public Tile TileFromWorldPosition(Vector3 worldPosition)
    {
        Vector2Int convertedPosition = new Vector2Int();
        convertedPosition.x = Mathf.RoundToInt(((size.x - 1) * worldPosition.x) / worldSize.x);
        convertedPosition.y = Mathf.RoundToInt(((size.y - 1) * worldPosition.y) / worldSize.y);
        return _tileGrid[convertedPosition.x, convertedPosition.y];
    }
    
    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _tileGrid = new Tile[size.x, size.y];
        
        SpawnTiles();
        FindAllNeighbours();
        worldSize = new Vector3(_tileGrid[size.x - 1, 0].transform.position.x,
                                        _tileGrid[0, size.y - 1].transform.position.y,
                                            0f);
    }
}
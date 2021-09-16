using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Map : MonoBehaviour
{
    private Transform _transform;
    [Header("Map Spawn")] 
    [SerializeField] public Vector2Int size = new Vector2Int(10, 10);
    [NonSerialized] public float offset = 1f;
    [SerializeField] private GameObject[] tilePrefabs;
    
    [Header("Perlin Settings")]
    [SerializeField] private int seedNoise;
    [SerializeField] private int scaleNoise;
    [SerializeField] private float _groundWeight;

    [NonSerialized] public Tile[,] tileGrid;
    [NonSerialized] public Vector3 worldSize;
    
    private void TileNeighbours(Tile tile)
    {
        // Iterates on 3x3 grid around tile and checks if valid tile
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
                tile.neighbourTiles.Add(tileGrid[currentPos.x, currentPos.y]);
            }
        }
    }

    private void FindAllNeighbours()
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                TileNeighbours(tileGrid[x, y]);
            }
        }
    }

    private void PerlinGenerate()
    {
        // Randomly generate noise
        if (seedNoise == 0)
            seedNoise = Random.Range(0, 1000);
    
        if (scaleNoise == 0) 
            scaleNoise = Random.Range(5, 10);
        
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2 worldPos = new Vector3(x, y) * offset;
                
                // Tile to spawn
                GameObject tilePrefab = TileFromPerlin(x, y, seedNoise, scaleNoise);
                tileGrid[x, y] = Instantiate(tilePrefab, worldPos, Quaternion.identity).GetComponent<Tile>();
                
                tileGrid[x, y].gridPosition = new Vector2Int(x, y);
                tileGrid[x, y].worldPosition = tileGrid[x, y].transform.position;
                
                // Set Tiles as children of Map
                tileGrid[x, y].transform.SetParent(_transform);
                tileGrid[x, y].name = $"Tile({x}, {y})";
            }
        }
    }

    private GameObject TileFromPerlin(int x, int y, int noise, int scale)
    {
        float xCoord = ((float)(x + noise) / size.x * scale);
        float yCoord = ((float)(y + noise) / size.y * scale);
        
        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        int i = sample < _groundWeight ? 0 : 1;
        return tilePrefabs[i];
    }
    
    public Tile TileFromWorldPosition(Vector3 worldPosition)
    {
        Vector2Int convertedPosition = new Vector2Int();
        convertedPosition.x = Mathf.RoundToInt(((size.x - 1) * worldPosition.x) / worldSize.x);
        convertedPosition.y = Mathf.RoundToInt(((size.y - 1) * worldPosition.y) / worldSize.y);
        return tileGrid[convertedPosition.x, convertedPosition.y];
    }
    
    private void Awake()
    {
        _transform = GetComponent<Transform>();
        tileGrid = new Tile[size.x, size.y];
        
        PerlinGenerate();
        FindAllNeighbours();
        worldSize = new Vector3(tileGrid[size.x - 1, 0].transform.position.x,
                                        tileGrid[0, size.y - 1].transform.position.y,
                                            0f);
    }
}